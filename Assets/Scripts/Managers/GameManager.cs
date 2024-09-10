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
    [SerializeField] private Slider timerSlider;
    [SerializeField] private Image timerFill;
    [SerializeField] private Color startColor = Color.green;
    [SerializeField] private Color endColor = Color.red;
    [SerializeField] private float bombTimeBonus = 10f;
    
    [Header("Count Off Settings")]
    [SerializeField] private float countOffTime = 3f;
    [SerializeField] private GameObject countOffPanel;
    [SerializeField] private TMP_Text countOffText;

    [Header("Reroll Settings")] 
    [SerializeField] private Button reRollButton;
    [SerializeField] private TMP_Text reRollText;
    [SerializeField] private int maxReRoll = 2;
    [SerializeField] private int reRollScoreThreshold = 5000;

    [Header("Pause Settings")]
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private Slider volumeSlider;
    
    [Header("Game Over Settings")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TMP_Text gameOverText;
    
    [Header("Score Settings")]
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private int scorePerPlacement = 100;
    [SerializeField] private int scorePerCombo = 100;
    [SerializeField] private int scorePerBomb = 200;
    [SerializeField] private int scorePerFitMe = 10000;

    private bool _sceneActivated;
    private int _currentReRoll;
    private int _previousReRollScore;
    private float _currentGameTimer;
    private float _countOffTimer;
    private bool _isGameOver;
    private bool _isPaused;
    private bool _gameStarted;
    private int _score;
    private bool _countDownPlayed;
    private AudioSource _bgmAudioSource;
    private Coroutine _leaderboardCoroutine;
    public bool IsGameOver => _isGameOver;
    public bool GameStarted => _gameStarted;
    public bool IsPaused => _isPaused;
    public int CurrentReRoll => _currentReRoll;
    // Start is called before the first frame update

    private void Awake()
    {
        _instance = this;
    }

    void Start()
    {
        _currentGameTimer = gameTimer;
        gameOverPanel.SetActive(false);
        gameOverPanel.transform.localScale = Vector3.zero;
        pausePanel.SetActive(false);
        countOffPanel.SetActive(true);
        UpdateScoreText(false);
        UpdateReRollText(false);
        reRollButton.interactable = false;
        volumeSlider.value = SoundManager.Instance.MasterVolume;
        volumeSlider.onValueChanged.AddListener((_) =>
        {
            SoundManager.Instance.ChangeMixerVolume(volumeSlider.value);
        });
        volumeSlider.gameObject.SetActive(false);
        if (LoadSceneManager.Instance.FirstSceneLoaded == SceneManager.GetActiveScene() || LoadSceneManager.Instance.Retry)
        {
            ActivateScene();
            LoadSceneManager.Instance.Retry = false;
        }
    }
    
    public void ActivateScene()
    {
        if (_sceneActivated) return;
        _sceneActivated = true;
        SoundManager.Instance.PlaySoundFX(SoundFXTypes.CountOff, out _);
    }

    // Update is called once per frame
    void Update()
    {
        if (!_sceneActivated) return;
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
                //SoundManager.Instance.PlaySoundFX(SoundFXTypes.Score, out _);
                break;
            case ScoreTypes.Combo:
                if (contactedAmount <= 1) return;
                int score = scorePerCombo * (contactedAmount - 1);
                Debug.Log("Combo Score: " + score);
                ChangeScore(score);
                //SoundManager.Instance.PlaySoundFX(SoundFXTypes.Score, out _);
                break;
            case ScoreTypes.Bomb:
                if (contactedAmount <= 2) return;
                int bombScore = scorePerBomb * contactedAmount;
                Debug.Log("Bomb Score: " + bombScore);
                ChangeScore(bombScore);
                ChangeGameTimer(bombTimeBonus);
                //SoundManager.Instance.PlaySoundFX(SoundFXTypes.Score, out _);
                //SoundManager.Instance.PlaySoundFX(SoundFXTypes.BonusTime, out _);
                //SoundManager.Instance.PlaySoundFX(SoundFXTypes.BombAnnounce, out _);
                SoundManager.Instance.PlaySoundFX(SoundFXTypes.BombExplode, out _);
                SoundManager.Instance.PlaySoundFX(SoundFXTypes.Congrats, out _);
                break;
            case ScoreTypes.FitMe:
                ChangeScore(scorePerFitMe);
                ChangeGameTimer(gameTimer);
                //SoundManager.Instance.PlaySoundFX(SoundFXTypes.ScoreFitMe, out _);
                //SoundManager.Instance.PlaySoundFX(SoundFXTypes.BonusTimeFitMe, out _);
                //SoundManager.Instance.PlaySoundFX(SoundFXTypes.FitMeAnnounce, out _);
                SoundManager.Instance.PlaySoundFX(SoundFXTypes.FitMeExplode, out _);
                break;
        }
        if (_score - _previousReRollScore < reRollScoreThreshold) return;
        int reRoll = Mathf.FloorToInt((_score - _previousReRollScore) / (float)reRollScoreThreshold);
        if (ChangeReRoll(reRoll)) SoundManager.Instance.PlaySoundFX(SoundFXTypes.ReRollGain, out _);
        _previousReRollScore += reRollScoreThreshold;
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

        scoreText.text = _score.ToString("N0");
    }
    
    /// <summary>
    /// Update the count off timer
    /// </summary>
    private void UpdateCountOff()
    {
        if (GameStarted || IsPaused) return;
        _countOffTimer += Time.deltaTime;
        int countOff = Mathf.CeilToInt(countOffTime - _countOffTimer) - 1;
        if (countOff == 0)
        {
            countOffText.text = "GO!";
        }
        else
        {
            countOffText.text = countOff.ToString();
        }
        if (_countOffTimer < countOffTime) return;
        _gameStarted = true;
        _countOffTimer = 0;
        countOffPanel.SetActive(false);
        RandomBlock.Instance.SpawnAtStart();
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
        switch (_currentGameTimer)
        {
            case > 10 when _countDownPlayed:
                SoundManager.Instance.StopSound(_bgmAudioSource);
                SoundManager.Instance.PlayBGM(BGMTypes.Game, out _bgmAudioSource);
                _countDownPlayed = false;
                break;
            case <= 10 when !_countDownPlayed:
                SoundManager.Instance.StopSound(_bgmAudioSource);
                SoundManager.Instance.PlayBGM(BGMTypes.TenSecondsLeft, out _bgmAudioSource);
                _countDownPlayed = true;
                break;
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
    }
    
    public bool ChangeReRoll(int value)
    {
        int before = _currentReRoll;
        _currentReRoll += value;
        _currentReRoll = Mathf.Clamp(_currentReRoll, 0, maxReRoll);
        reRollButton.interactable = _currentReRoll > 0;
        if (_currentReRoll == before) return false;
        UpdateReRollText();
        return true;
    }
    
    private void UpdateReRollText(bool bump = true)
    {
        reRollText.text = $"{_currentReRoll}/{maxReRoll}";
        if (bump)
        {
            reRollText.transform.DOScale(1.2f, 0.1f).OnComplete(() =>
            {
                reRollText.transform.DOScale(1f, 0.1f);
            });
        }
    }

    public void ReRoll()
    {
        if (_currentReRoll <= 0) return;
        if (ChangeReRoll(-1)) SoundManager.Instance.PlaySoundFX(SoundFXTypes.ReRollLose, out _);
        RandomBlock.Instance.ReRoll();
    }
    
    public void PauseGame()
    {
        if (IsGameOver || !GameStarted) return;
        _isPaused = true;
        pausePanel.SetActive(true);
        SoundManager.Instance.PlaySoundFX(SoundFXTypes.Pause, out _);
        SoundManager.Instance.PauseSound(_bgmAudioSource);
    }
    
    public void ResumeGame()
    {
        if (IsGameOver || !GameStarted) return;
        _isPaused = false;
        pausePanel.SetActive(false);
        SoundManager.Instance.ResumeSound(_bgmAudioSource);
    }

    public void GameOver(bool fail = false)
    {
        if (_leaderboardCoroutine != null) return;
        _isGameOver = true;
        _currentGameTimer = 0;
        SoundManager.Instance.StopSound(_bgmAudioSource);
        gameOverText.text = fail ? "Failed!" : "Time's Up!";
        gameOverPanel.SetActive(true);
        gameOverPanel.transform.DOScale(1f, 0.5f).SetEase(Ease.OutBounce);
        StartCoroutine(GameOverCoroutine(fail));
    }

    private IEnumerator GameOverCoroutine(bool fail = false)
    {
        AudioSource temp;
        if (fail) SoundManager.Instance.PlaySoundFX(SoundFXTypes.Fail, out temp);
        else SoundManager.Instance.PlaySoundFX(SoundFXTypes.TimeOut, out temp);
        yield return new WaitForSeconds(temp.clip.length);
        LoadSceneManager.Instance.Score = _score;
        _leaderboardCoroutine = StartCoroutine(LoadLeaderboard());
    }
    private IEnumerator LoadLeaderboard()
    {
        AsyncOperation loadSceneAsync = SceneManager.LoadSceneAsync(SceneNames.Leaderboard.ToString(), LoadSceneMode.Additive);
        while (!loadSceneAsync.isDone)
        {
            yield return null;
        }
    }
    public void BackToMenu()
    {
        if (SceneManager.sceneCount > 1) return;
        SoundManager.Instance.StopSound(_bgmAudioSource);
        SceneManager.LoadScene(SceneNames.MainMenu.ToString());
    }

    public void Retry()
    {
        if (SceneManager.sceneCount > 1) return;
        LoadSceneManager.Instance.Retry = true;
        SoundManager.Instance.StopSound(_bgmAudioSource);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    
    public void ToggleVolumeSlider()
    {
        volumeSlider.gameObject.SetActive(!volumeSlider.gameObject.activeSelf);
    }
}
