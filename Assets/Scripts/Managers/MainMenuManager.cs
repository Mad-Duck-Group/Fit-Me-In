using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField] private Texture2D cursorSprite;
    private AudioSource _bgmAudioSource;

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
        SoundManager.Instance.StopSound(_bgmAudioSource);
        SoundManager.Instance.PlaySoundFX(SoundFXTypes.SceneTransition, out _);
        SceneManager.LoadScene("UI");
    }
    
    public void ExitGame()
    {
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
        Application.Quit();
    }
}
