using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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
    [SerializeField] private float gameTimer = 60f;
    [SerializeField] private Slider timerSlider;
    [SerializeField] private Image timerFill;
    [SerializeField] private Color startColor = Color.green;
    [SerializeField] private Color endColor = Color.red;
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TMP_Text gameOverScoreText;

    private float _currentGameTimer;
    private bool _isGameOver;
    private int _score;
    
    public bool IsGameOver => _isGameOver;
    // Start is called before the first frame update

    private void Awake()
    {
        _instance = this;
    }

    void Start()
    {
        _currentGameTimer = gameTimer;
        gameOverPanel.SetActive(false);
        UpdateScoreText();
    }

    // Update is called once per frame
    void Update()
    {
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

    /// <summary>
    /// Update the score text
    /// </summary>
    private void UpdateScoreText()
    {
        scoreText.text = "Score: " + _score;
    }

    /// <summary>
    /// Update the game timer
    /// </summary>
    private void UpdateGameTimer()
    {
        if (_currentGameTimer <= 0) return;
        _currentGameTimer -= Time.deltaTime;
        timerSlider.value = _currentGameTimer / gameTimer;
        Color color = Color.Lerp(endColor, startColor, _currentGameTimer / gameTimer);
        timerFill.color = color;
        if (_currentGameTimer <= 0)
        {
            _isGameOver = true;
            ShowGameOverPanel();
            _currentGameTimer = 0;
        }
    }
    
    private void ShowGameOverPanel()
    {
        gameOverPanel.SetActive(true);
        gameOverScoreText.text = "Score: " + _score;
    }
    
    public void BackToMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
