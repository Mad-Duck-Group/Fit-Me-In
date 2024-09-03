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
    
    [Serializable]
    private struct SpawnPoint
    {
        [SerializeField] private Transform transform;
        public Transform Transform => transform;
        private bool _isFree;
        public bool IsFree { get => _isFree; set => _isFree = value; }

    }
    
    [SerializeField] GameObject[] randomObjects;
    [SerializeField] SpawnPoint[] spawnPositions;

    void Awake()
    {
        _instance = this;
    } 
    
    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < spawnPositions.Length; i++)
        {
            FreeSpawnPoint(i);
        }
        SpawnRandomBlock();
    }
    
    public void FreeSpawnPoint(int index)
    {
        spawnPositions[index].IsFree = true;
    }

    public void SpawnRandomBlock()
    {
        for (int i = 0; i < spawnPositions.Length; i++)
        {
            if (!spawnPositions[i].IsFree)
            {
                continue;
            }
            Transform spawnTransform = spawnPositions[i].Transform;
            int randomIndex = Random.Range(0, randomObjects.Length);
            Block spawn = Instantiate(randomObjects[randomIndex], spawnTransform.position, Quaternion.identity).GetComponent<Block>();
            spawn.SpawnIndex = i;
            spawnPositions[i].IsFree = false;
        }
    }
}
