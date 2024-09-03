using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

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
    private int i;

    void Awake()
    {
        _instance = this;
    } 
    
    // Start is called before the first frame update
    void Start()
    {
        for (i = 0; i < spawnPositions.Length; i++)
        {
            if (isFree == true)
            {
                SpawnRandomBlock();
                if (i >= 3)
                {
                    isFree = false;
                }
            }
        }
        
    }

    private void Update()
    {
        Debug.Log("FreeSpawnPoint: " + _freeSpawnPoint);
        Debug.Log("isFree: " + isFree);
    }

    public void SpawnRandomBlock()
    {
        Transform spawnPosition = spawnPositions[i];
        int randomIndex = Random.Range(0, randomObjects.Length);
        Instantiate(randomObjects[randomIndex], spawnPosition.position, Quaternion.identity);
        _freeSpawnPoint++;
    }
}
