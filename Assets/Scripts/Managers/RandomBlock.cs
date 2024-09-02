using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomBlock : MonoBehaviour
{
    private static RandomBlock _instance;
        
    public static RandomBlock Instance
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
    
    [SerializeField] GameObject[] randomObjects;
    [SerializeField] Transform[] spawnPositions;
    private int _freeSpawnPoint;
    public int FreeSpawnPoint { get => _freeSpawnPoint; set => _freeSpawnPoint = value; }
    bool isFree = true;

    void Awake()
    {
        _instance = this;
    } 
    
    // Start is called before the first frame update
    void Update()
    {
        if (_freeSpawnPoint >= 3)
            return;
        SpawnRandomBlock();
        
        
    }

    public void SpawnRandomBlock()
    {
        for (int i = 0; i < spawnPositions.Length; i++)
        {
            Transform spawnPosition = spawnPositions[i];
            int randomIndex = Random.Range(0, randomObjects.Length);
            Instantiate(randomObjects[randomIndex], spawnPosition.position, Quaternion.identity);
            _freeSpawnPoint++;
        }
    }
}
