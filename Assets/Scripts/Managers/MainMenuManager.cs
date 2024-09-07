using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField] private Texture2D cursorSprite;
    [SerializeField] private GameObject canvas;
    [SerializeField] private Vector3 canvasSlideDistance;
    private AudioSource _bgmAudioSource;
    private Coroutine _loadGameSceneCoroutine;

    private void Awake()
    {
        Cursor.SetCursor(cursorSprite, Vector2.zero, CursorMode.Auto);
    }
    
    private void Start()
    {
        SoundManager.Instance.PlayBGM(BGMTypes.MainMenu, out _bgmAudioSource);
    }

    public void StartGame()
    {
        if (_loadGameSceneCoroutine != null) return;
        SoundManager.Instance.StopSound(_bgmAudioSource);
        SoundManager.Instance.PlaySoundFX(SoundFXTypes.SceneTransition, out _);
        _loadGameSceneCoroutine = StartCoroutine(LoadGameScene());
    }

    private IEnumerator LoadGameScene()
    {
        //Tween the canvas out of the screen
        AsyncOperation loadSceneAsync = SceneManager.LoadSceneAsync("UI", LoadSceneMode.Additive); 
        while (!loadSceneAsync.isDone)
        {
            yield return null;
        }
        canvas.transform.DOLocalMove(canvasSlideDistance, 2f).SetEase(Ease.OutBack).OnComplete(() =>
        {
            GameManager.Instance.ActivateScene();
            SceneManager.UnloadSceneAsync("MainMenu");
        });
    }
    
    public void ExitGame()
    {
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
        Application.Quit();
    }
}
