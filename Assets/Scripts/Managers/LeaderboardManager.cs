using System.Collections;
using System.Collections.Generic;
using Dan.Main;
using Dan.Models;
using DG.Tweening;
using NaughtyAttributes;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class LeaderboardManager : MonoBehaviour
{
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text notificationText;
    [SerializeField] private float notificationDuration = 3f;
    [SerializeField] private TMP_InputField usernameInput;
    [SerializeField] private PlayerEntry playerEntryPrefab;
    [SerializeField] private int maxEntries = 50;
    [SerializeField] private Color selfEntryColor;
    [SerializeField] private GameObject canvas;
    [SerializeField] private Vector3 canvasSlideDistance;
    private List<PlayerEntry> _playerEntries = new List<PlayerEntry>();
    private Tween _notificationTween;
    
    [Header("Debug")]
    [SerializeField] private string testUsername;
    [SerializeField] private int testScore;
    // Start is called before the first frame update
    void Start()
    {
        canvas.transform.localPosition -= canvasSlideDistance;
        Vector3 targetPosition = canvas.transform.localPosition + canvasSlideDistance;
        canvas.transform.DOLocalMove(targetPosition, 2f).SetEase(Ease.OutQuint).OnComplete(() =>
        {
            SceneManager.UnloadSceneAsync("Game");
        });
        scoreText.text = LoadSceneManager.Instance.Score.ToString();
        notificationText.transform.localScale = Vector3.zero;
        notificationText.text = "";
        GetEntries();
    }
    
    [Button("Test Get Entries")]
    public void GetEntries()
    {
        Leaderboards.FitMeIn.GetEntries(OnGetEntries, OnGetEntriesFailed);
    }
    
    public void RefreshButton()
    {
        GetEntries();
    }

    public void UploadEntry(string username, int score)
    {
        Leaderboards.FitMeIn.UploadNewEntry(username, score, OnUploadEntrySuccess, OnUploadEntryFailed);
    }

    [Button("Upload Test Entry")]
    private void UploadTestEntry()
    {
        UploadEntry(testUsername, testScore);
    }
    
    public void SubmitButton()
    {
        if (string.IsNullOrEmpty(usernameInput.text))
        {
            ShowNotification("Please enter a username");
            return;
        }
        UploadEntry(usernameInput.text, LoadSceneManager.Instance.Score);
    }

    public void DeleteEntry()
    {
        Leaderboards.FitMeIn.DeleteEntry(OnDeleteEntrySuccess, OnDeleteEntryFailed);
    }
    
    [Button("Delete Test Entry")]
    private void DeleteTestEntry()
    {
        DeleteEntry();
    }

    public void DeleteButton()
    {
        DeleteEntry();
    }
    
    public void GetPersonalEntry()
    {
        Leaderboards.FitMeIn.GetPersonalEntry(OnPersonalEntryLoaded, OnGetEntriesFailed);
    }
    
    public void PersonalButton()
    {
        GetPersonalEntry();
    }
    
    private void ResetEntriesView()
    {
        foreach (var playerEntry in _playerEntries)
        {
            Destroy(playerEntry.gameObject);
        }
        _playerEntries.Clear();
    }
    
    private void OnGetEntries(Entry[] entries)
    {
        ResetEntriesView();
        for (var i = 0; i < maxEntries; i++)
        {
            if (i >= entries.Length) break;
            CreateEntryDisplay(entries[i]);
        }
    }
    
    private void CreateEntryDisplay(Entry entry)
    {
        PlayerEntry newPlayerEntry = Instantiate(playerEntryPrefab, scrollRect.content);
        newPlayerEntry.SetEntry(entry.Rank, entry.Username, entry.Score);
        _playerEntries.Add(newPlayerEntry);
        bool isMine = entry.IsMine();
        if (isMine)
        {
            newPlayerEntry.SetTextColor(selfEntryColor);
        }
    }

    private void OnGetEntriesFailed(string error)
    {
        Debug.LogWarning(error);
        ShowNotification(error);
    }
    
    private void OnUploadEntrySuccess(bool success)
    {
        if (success)
        {
            GetEntries();
        }
    }
    
    private void OnUploadEntryFailed(string error)
    {
        Debug.LogWarning(error);
        ShowNotification(error);
    }
    
    private void OnDeleteEntrySuccess(bool success)
    {
        if (success)
        {
            GetEntries();
        }
    }
    
    private void OnDeleteEntryFailed(string error)
    {
        Debug.LogWarning(error);
        ShowNotification($"{error}Deleting entry failed!");
    }
    
    private void OnPersonalEntryLoaded(Entry entry)
    {
        if (entry.Rank == 0)
        {
            ShowNotification("No personal entry found");
            return;
        }
        ResetEntriesView();
        CreateEntryDisplay(entry);
    }
    
    private void ShowNotification(string message)
    {
        if (_notificationTween.IsActive()) return;
        notificationText.text = message;
        _notificationTween = notificationText.transform.DOScale(Vector3.one, 0.2f).OnComplete(() =>
        {
            _notificationTween = DOVirtual.DelayedCall(notificationDuration, () =>
            {
                _notificationTween = notificationText.transform.DOScale(Vector3.zero, 0.2f);
            });
        });
    }
    
    [Button("Test Notification")]
    private void TestNotification()
    {
        ShowNotification("This is a test notification");
    }
    
    public void MainMenuButton()
    {
        SceneManager.LoadScene(SceneNames.MainMenu.ToString());
    }
    
    public void RetryButton()
    {
        LoadSceneManager.Instance.Retry = true;
        SceneManager.LoadScene(SceneNames.Game.ToString());
    }
}
