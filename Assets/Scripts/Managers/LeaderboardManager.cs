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
    [SerializeField] private TMP_Text newHighScoreText;
    [SerializeField] private TMP_Text notificationText;
    [SerializeField] private float notificationDuration = 3f;
    [SerializeField] private TMP_InputField usernameInput;
    [SerializeField] private PlayerEntry playerEntryPrefab;
    [SerializeField] private int maxEntries = 50;
    [SerializeField] private Color selfEntryColor;
    [SerializeField] private GameObject canvas;
    [SerializeField] private Vector3 canvasSlideDistance;
    private List<PlayerEntry> _playerEntries = new List<PlayerEntry>();
    private PlayerEntry _selfEntry;
    private Tween _notificationTween;
    private Coroutine _loadMainMenuSceneCoroutine;
    private AudioSource _bgmAudioSource;
    
    [Header("Debug")]
    [SerializeField] private string testUsername;
    [SerializeField] private int testScore;
    // Start is called before the first frame update
    void Start()
    {
        canvas.transform.localPosition -= canvasSlideDistance;
        Vector3 targetPosition = canvas.transform.localPosition + canvasSlideDistance;
        canvas.transform.DOLocalMove(targetPosition, 3f).SetEase(Ease.OutQuart).OnComplete(() =>
        {
            if (SceneManager.GetSceneByName(SceneNames.Game.ToString()).isLoaded)
                SceneManager.UnloadSceneAsync(SceneNames.Game.ToString());
            if (SceneManager.GetSceneByName(SceneNames.MainMenu.ToString()).isLoaded)
                SceneManager.UnloadSceneAsync(SceneNames.MainMenu.ToString());
        });
        SoundManager.Instance.PlayBGM(BGMTypes.Leaderboard, out _bgmAudioSource);
        SoundManager.Instance.PlaySoundFX(SoundFXTypes.Celebrate, out _);
        newHighScoreText.gameObject.SetActive(false);
        scoreText.text = LoadSceneManager.Instance.Score.ToString();
        notificationText.transform.localScale = Vector3.zero;
        notificationText.text = "";
        CheckNewHighScore();
        GetEntries();
    }
    
    private void CheckNewHighScore()
    {
        GetPersonalEntry(true);
    }
    
    [Button("Test Get Entries")]
    public void GetEntries(bool scrollToSelf = false)
    {
        Leaderboards.FitMeIn.GetEntries(a => OnGetEntries(a, scrollToSelf), OnGetEntriesFailed);
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
    
    public void GetPersonalEntry(bool checkNewHighScore = false)
    {
        Leaderboards.FitMeIn.GetPersonalEntry((a) => OnPersonalEntryLoaded(a, checkNewHighScore), OnGetEntriesFailed);
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
    
    private void OnGetEntries(Entry[] entries, bool scrollToSelf = false)
    {
        ResetEntriesView();
        for (var i = 0; i < maxEntries; i++)
        {
            if (i >= entries.Length) break;
            CreateEntryDisplay(entries[i]);
        }
        if (!scrollToSelf) return;
        //scroll to self entry
        if (_selfEntry && _playerEntries.Contains(_selfEntry))
        {
            Canvas.ForceUpdateCanvases();
            float normalizedPosition =
                1 - (_playerEntries.FindIndex(a => _selfEntry == a) / (float)(_playerEntries.Count - 1));
            scrollRect.verticalNormalizedPosition = normalizedPosition;
            SoundManager.Instance.PlaySoundFX(SoundFXTypes.NameTag, out _);
        }
        else
        {
            ShowNotification("You are not on the top 50 list...");
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
            _selfEntry = newPlayerEntry;
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
            GetEntries(true);
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
    
    private void OnPersonalEntryLoaded(Entry entry, bool checkNewHighScore = false)
    {
        if (checkNewHighScore)
        {
            if (LoadSceneManager.Instance.Score < entry.Score) return;
            newHighScoreText.gameObject.SetActive(true);
            SoundManager.Instance.PlaySoundFX(SoundFXTypes.NewHighScore, out _);
            return;
        }
        if (entry.Rank == 0)
        {
            ShowNotification("No personal entry found");
            return;
        }
        ResetEntriesView();
        CreateEntryDisplay(entry);
        SoundManager.Instance.PlaySoundFX(SoundFXTypes.NameTag, out _);
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
        if (SceneManager.sceneCount > 1) return;
        if (_loadMainMenuSceneCoroutine != null) return;
        SoundManager.Instance.PlaySoundFX(SoundFXTypes.SceneTransition, out _);
        SoundManager.Instance.StopSound(_bgmAudioSource);
        LoadSceneManager.Instance.Score = 0;
        _loadMainMenuSceneCoroutine = StartCoroutine(LoadMainMenuScene());
    }
    
    private IEnumerator LoadMainMenuScene()
    {
        AsyncOperation loadSceneAsync = SceneManager.LoadSceneAsync(SceneNames.MainMenu.ToString(), LoadSceneMode.Additive);
        while (!loadSceneAsync.isDone)
        {
            yield return null;
        }
        canvas.transform.DOLocalMove(canvasSlideDistance, 3f).SetEase(Ease.OutQuart).OnComplete(() =>
        {
            SceneManager.UnloadSceneAsync(SceneNames.Leaderboard.ToString());
        });
    }
    public void RetryButton()
    {
        if (SceneManager.sceneCount > 1) return;
        LoadSceneManager.Instance.Retry = true;
        LoadSceneManager.Instance.Score = 0;
        SoundManager.Instance.StopSound(_bgmAudioSource);
        SceneManager.LoadScene(SceneNames.Game.ToString());
    }
}
