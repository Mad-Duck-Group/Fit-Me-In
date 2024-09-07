using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum SceneNames
{
    MainMenu,
    Game,
    Leaderboard
}
public class LoadSceneManager : MonoBehaviour
{
    private static LoadSceneManager _instance;
    public static LoadSceneManager Instance
    {
        get
        {
            if (_instance == null)
            {
                Debug.LogError("Load Scene Manager is null");
            }
            return _instance;
        }
    }

    private Scene _firstSceneLoaded;
    public Scene FirstSceneLoaded => _firstSceneLoaded;
    private bool _retry;
    public bool Retry {get => _retry; set => _retry = value; }
    private int _score;
    public int Score { get => _score; set => _score = value; }
    
    private void Awake()
    {
        List<LoadSceneManager> loadSceneManagers = FindObjectsOfType<LoadSceneManager>().ToList();
        if (loadSceneManagers.Count > 1)
        {
            foreach (LoadSceneManager loadSceneManager in loadSceneManagers)
            {
                if (loadSceneManager != _instance)
                {
                    Destroy(loadSceneManager.gameObject);
                }
            }
        }
        else
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        _firstSceneLoaded = SceneManager.GetActiveScene();
    }
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
