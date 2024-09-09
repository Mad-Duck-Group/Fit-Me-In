using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField] private Texture2D cursorSprite;
    [SerializeField] private GameObject canvas;
    [SerializeField] private Vector3 canvasSlideDistance;
    [SerializeField] private Slider volumeSlider;
    private AudioSource _bgmAudioSource;
    private Coroutine _loadGameSceneCoroutine;
    private Coroutine _loadLeaderboardSceneCoroutine;

    private void Awake()
    {
        Cursor.SetCursor(cursorSprite, Vector2.zero, CursorMode.Auto);
    }

    private void Start()
    {
        volumeSlider.gameObject.SetActive(false);
        volumeSlider.value = SoundManager.Instance.MasterVolume;
        volumeSlider.onValueChanged.AddListener((_) =>
        {
            SoundManager.Instance.ChangeMixerVolume(volumeSlider.value);
        });
        SoundManager.Instance.PlayBGM(BGMTypes.MainMenu, out _bgmAudioSource);
    }

    public void StartGame()
    {
        if (SceneManager.sceneCount > 1) return;
        if (_loadGameSceneCoroutine != null) return;
        SoundManager.Instance.StopSound(_bgmAudioSource);
        SoundManager.Instance.PlaySoundFX(SoundFXTypes.SceneTransition, out _);
        _loadGameSceneCoroutine = StartCoroutine(LoadGameScene());
    }

    private IEnumerator LoadGameScene()
    {
        //Tween the canvas out of the screen
        AsyncOperation loadSceneAsync = SceneManager.LoadSceneAsync(SceneNames.Game.ToString(), LoadSceneMode.Additive);
        while (!loadSceneAsync.isDone)
        {
            yield return null;
        }

        canvas.transform.DOLocalMove(canvasSlideDistance, 3f).SetEase(Ease.OutQuart).OnComplete(() =>
        {
            GameManager.Instance.ActivateScene();
            SceneManager.UnloadSceneAsync(SceneNames.MainMenu.ToString());
        });
    }

    public void ExitGame()
    {
#if (UNITY_EDITOR)
        UnityEditor.EditorApplication.isPlaying = false;
#elif (UNITY_STANDALONE) 
    Application.Quit();
#elif (UNITY_WEBGL)
    Application.OpenURL("https://madduckteam.itch.io");
#endif
    }

    public void ToLeaderboard()
    {
        if (SceneManager.sceneCount > 1) return;
        if (_loadLeaderboardSceneCoroutine != null) return;
        SoundManager.Instance.StopSound(_bgmAudioSource);
        SoundManager.Instance.PlaySoundFX(SoundFXTypes.SceneTransition, out _);
        _loadLeaderboardSceneCoroutine = StartCoroutine(LoadLeaderboardScene());
    }

    private IEnumerator LoadLeaderboardScene()
    {
        //Tween the canvas out of the screen
        AsyncOperation loadSceneAsync =
            SceneManager.LoadSceneAsync(SceneNames.Leaderboard.ToString(), LoadSceneMode.Additive);
        while (!loadSceneAsync.isDone)
        {
            yield return null;
        }

        canvas.transform.DOLocalMove(canvasSlideDistance, 3f).SetEase(Ease.OutQuart);
    }

    public void ToggleVolumeSlider()
    {
        volumeSlider.gameObject.SetActive(!volumeSlider.gameObject.activeSelf);
    }
}
    
