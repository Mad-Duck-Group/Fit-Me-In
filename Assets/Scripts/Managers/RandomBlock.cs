using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;
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
    public struct SpawnPoint
    {
        [SerializeField] private Transform transform;
        public Transform Transform => transform;
        private bool _isFree;
        public bool IsFree { get => _isFree; set => _isFree = value; }
        private Block _currentBlock;
        public Block CurrentBlock
        {
            get => _currentBlock;
            set => _currentBlock = value;
        }
    }

    [SerializeField] private float objectScale = 0.5f;
    [SerializeField] List<GameObject> randomObjects;
    [SerializeField] GameObject[] topten;
    [SerializeField] GameObject[] jelly;
    [SerializeField] GameObject[] pan;
    [SerializeField] GameObject[] sankaya;
    [FormerlySerializedAs("spawnPositions")] [SerializeField] SpawnPoint[] spawnPoints;
    public SpawnPoint[] SpawnPoints => spawnPoints;

    void Awake()
    {
        _instance = this;
    } 
    
    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < spawnPoints.Length; i++)
        {
            FreeSpawnPoint(i);
        }
        SpawnRandomBlock();
    }
    
    public void FreeSpawnPoint(int index)
    {
        spawnPoints[index].IsFree = true;
        spawnPoints[index].CurrentBlock = null;
    }

    public void RandomType()
    {
        GameObject toptenObj = topten[Random.Range(0, topten.Length)];
        GameObject jellyObj = jelly[Random.Range(0, jelly.Length)];
        GameObject panObj = pan[Random.Range(0, pan.Length)];
        GameObject sankayaObj = sankaya[Random.Range(0, sankaya.Length)];
        
        randomObjects = new List<GameObject>() {toptenObj, jellyObj, panObj, sankayaObj};
        
        
    }

    public void SpawnRandomBlock()
    {
        RandomType();
        for (int i = 0; i < spawnPoints.Length; i++)
        {
            if (!spawnPoints[i].IsFree)
            {
                continue;
            }
            Transform spawnTransform = spawnPoints[i].Transform;
            int randomIndex = Random.Range(0, randomObjects.Count);
            Block spawn = Instantiate(randomObjects[randomIndex], spawnTransform.position, Quaternion.identity).GetComponent<Block>();
            randomObjects.RemoveAt(randomIndex);
            spawn.SpawnIndex = i;
            spawn.transform.localScale = new Vector3(objectScale, objectScale, 1f);
            spawnPoints[i].IsFree = false;
            spawnPoints[i].CurrentBlock = spawn;
        }
    }
    
    public void DestroyBlock(bool destroyAll = false)
    {
        for (int i = 0; i < spawnPoints.Length; i++)
        {
            if (destroyAll)
            {
                Destroy(spawnPoints[i].CurrentBlock.gameObject);
                FreeSpawnPoint(i);
            }
            else if (!spawnPoints[i].IsFree)
            {
                Destroy(spawnPoints[i].CurrentBlock.gameObject);
                FreeSpawnPoint(i);
            }
        }
        if (!destroyAll)
        {
            SpawnRandomBlock();
        }
    }
    
    public void ReRoll()
    {
        DestroyBlock(true);
        SpawnRandomBlock();
    }
    

    public void GameOverCheck()
    {
        List<Block> blockToCheck = spawnPoints.Select(spawnPoint => spawnPoint.CurrentBlock).ToList();
        if (!GridManager.Instance.CheckAvailableBlock(blockToCheck, out _))
        {
            GameManager.Instance.GameOver();
        }
    }
}
