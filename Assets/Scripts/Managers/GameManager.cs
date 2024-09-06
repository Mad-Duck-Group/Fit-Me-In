using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public enum ScoreTypes
{
    Placement,
    Combo,
    Bomb,
    FitMe,
}
public class GameManager : MonoBehaviour
{
    private static GameManager _instance;
    public static GameManager Instance
    {
        get
        {
            if (_instance == null)
            {
                Debug.LogError("Game Manager is null");
            }
            return _instance;
        }
    }
    [Header("Time Settings")]
    [SerializeField] private float gameTimer = 60f;
    [SerializeField] private float countOffTime = 3f;
    [SerializeField] private Slider timerSlider;
    [SerializeField] private Image timerFill;
    [SerializeField] private Color startColor = Color.green;
    [SerializeField] private Color endColor = Color.red;
    [SerializeField] private float bombTimeBonus = 10f;
    
    [Header("Pause Settings")]
    [SerializeField] private GameObject pausePanel;
    
    [Header("Game Over Settings")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TMP_Text gameOverScoreText;
    
    [Header("Score Settings")]
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private int scorePerPlacement = 100;
    [SerializeField] private int scorePerCombo = 100;
    [SerializeField] private int scorePerBomb = 200;
    [SerializeField] private int scorePerFitMe = 10000;


    private float _currentGameTimer;
    private float _countOffTimer;
    private bool _isGameOver;
    private bool _isPaused;
    private bool _gameStarted;
    private int _score;
    private bool _countDownPlayed;
    private AudioSource _bgmAudioSource;
    
    public bool IsGameOver => _isGameOver;
    public bool GameStarted => _gameStarted;
    
    public bool IsPaused => _isPaused;
    // Start is called before the first frame update

    private void Awake()
    {
        _instance = this;
    }

    void Start()
    {
        _currentGameTimer = gameTimer;
        gameOverPanel.SetActive(false);
        pausePanel.SetActive(false);
        UpdateScoreText(false);
        SoundManager.Instance.PlaySoundFX(SoundFXTypes.CountOff, out _);
    }

    // Update is called once per frame
    void Update()
    {
        UpdateCountOff();
        UpdateGameTimer();
    }
    
    /// <summary>
    /// Change the score by the given value
    /// </summary>
    /// <param name="value"></param>
    public void ChangeScore(int value)
    {
        _score += value;
        UpdateScoreText();
    }

    public void AddScore(ScoreTypes scoreType, int contactedAmount = 0)
    {
        switch (scoreType)
        {
            case ScoreTypes.Placement:
                ChangeScore(scorePerPlacement);
                Debug.Log("Placement Score: " + scorePerPlacement);
                SoundManager.Instance.PlaySoundFX(SoundFXTypes.Score, out _);
                break;
            case ScoreTypes.Combo:
                if (contactedAmount <= 1) return;
                int score = scorePerCombo * (contactedAmount - 1);
                Debug.Log("Combo Score: " + score);
                ChangeScore(score);
                SoundManager.Instance.PlaySoundFX(SoundFXTypes.Score, out _);
                break;
            case ScoreTypes.Bomb:
                if (contactedAmount <= 2) return;
                int bombScore = scorePerBomb * contactedAmount;
                Debug.Log("Bomb Score: " + bombScore);
                ChangeScore(bombScore);
                ChangeGameTimer(bombTimeBonus);
                SoundManager.Instance.PlaySoundFX(SoundFXTypes.Score, out _);
                SoundManager.Instance.PlaySoundFX(SoundFXTypes.BonusTime, out _);
                SoundManager.Instance.PlaySoundFX(SoundFXTypes.BombAnnounce, out _);
                SoundManager.Instance.PlaySoundFX(SoundFXTypes.BombExplode, out _);
                break;
            case ScoreTypes.FitMe:
                ChangeScore(scorePerFitMe);
                ChangeGameTimer(gameTimer);
                SoundManager.Instance.PlaySoundFX(SoundFXTypes.ScoreFitMe, out _);
                SoundManager.Instance.PlaySoundFX(SoundFXTypes.BonusTimeFitMe, out _);
                SoundManager.Instance.PlaySoundFX(SoundFXTypes.FitMeAnnounce, out _);
                SoundManager.Instance.PlaySoundFX(SoundFXTypes.FitMeExplode, out _);
                break;
        }
    }

    /// <summary>
    /// Update the score text
    /// </summary>
    private void UpdateScoreText(bool bump = true)
    {
        //Bump animation
        if (bump)
        {
            scoreText.transform.DOScale(1.2f, 0.1f).OnComplete(() =>
            {
                scoreText.transform.DOScale(1f, 0.1f);
            });
        }
        scoreText.text = "Score: " + _score;
    }
    
    /// <summary>
    /// Update the count off timer
    /// </summary>
    private void UpdateCountOff()
    {
        if (GameStarted || IsPaused) return;
        _countOffTimer += Time.deltaTime;
        if (_countOffTimer < countOffTime) return;
        _gameStarted = true;
        _countOffTimer = 0;
        SoundManager.Instance.PlayBGM(BGMTypes.Game, out _bgmAudioSource);
    }

    /// <summary>
    /// Update the game timer
    /// </summary>
    private void UpdateGameTimer()
    {
        if (!GameStarted || IsGameOver || IsPaused) return;
        _currentGameTimer -= Time.deltaTime;
        timerSlider.value = _currentGameTimer / gameTimer;
        Color color = Color.Lerp(endColor, startColor, _currentGameTimer / gameTimer);
        timerFill.color = color;
        if (_currentGameTimer <= 10 && !_countDownPlayed)
        {
            SoundManager.Instance.PlaySoundFX(SoundFXTypes.TenSecondsLeft, out _);
            _countDownPlayed = true;
        }
        if (_currentGameTimer <= 0)
        {
            GameOver();
        }
    }

    public void ChangeGameTimer(float value, bool bump = true)
    {
        float newTimer = _currentGameTimer + value;
        _currentGameTimer = Mathf.Clamp(newTimer, 0, gameTimer);
        if (bump)
        {
            timerSlider.transform.DOScale(1.2f, 0.1f).OnComplete(() =>
            {
                timerSlider.transform.DOScale(1f, 0.1f);
            });
        }
        if (_currentGameTimer > 10)
        {
            _countDownPlayed = false;
        }
    }
    
    public void PauseGame()
    {
        _isPaused = true;
        pausePanel.SetActive(true);
        SoundManager.Instance.PlaySoundFX(SoundFXTypes.Pause, out _);
        SoundManager.Instance.PauseSound(_bgmAudioSource);
    }
    
    public void ResumeGame()
    {
        _isPaused = false;
        pausePanel.SetActive(false);
        SoundManager.Instance.ResumeSound(_bgmAudioSource);
    }
    
    public void GameOver()
    {
        _isGameOver = true;
        _currentGameTimer = 0;
        gameOverPanel.SetActive(true);
        gameOverScoreText.text = "Score: " + _score;
        SoundManager.Instance.PlaySoundFX(SoundFXTypes.TimeOut, out _);
        SoundManager.Instance.StopSound(_bgmAudioSource);
    }
    
    public void BackToMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void Retry()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
