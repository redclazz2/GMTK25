using System.Collections;
using UnityEngine;

public enum SpawnMode
{
    BetweenWaves,
    WaveActive
}

public class WaveManager : MonoBehaviour
{
    public SpawnZone[] spawnZones;
    public GameObject[] ambientEnemies;
    public GameObject[] waveEnemies;
    
    // Wave configuration
    public float waveDuration = 20f;
    public float intermissionDuration = 10f;
    public float ambientInterval = 6f;
    public float batchSpawnInterval = 5f;
    public int enemiesPerBatch = 4;
    public float enemySpawnDelay = 1.2f; // Delay between individual enemies in a batch
    
    private float ambientTimer = 0f;
    private float batchTimer = 0f;
    private float waveTimer = 0f;
    private float intermissionTimer = 0f;
    private bool inWave = false;
    private int currentWave = 0;
    private SpawnZone currentZone;
    
    private Coroutine spawnBatchCoroutine; // Track the coroutine
    
    void Start()
    {
        PickNewSpawnZone();
    }
    
    void Update()
    {
        if (inWave)
        {
            waveTimer += Time.deltaTime;
            batchTimer += Time.deltaTime;
            
            if (batchTimer >= batchSpawnInterval)
            {
                // Stop any existing batch spawn before starting a new one
                if (spawnBatchCoroutine != null)
                {
                    StopCoroutine(spawnBatchCoroutine);
                }
                
                spawnBatchCoroutine = StartCoroutine(SpawnBatchCoroutine());
                batchTimer = 0f;
                PickNewSpawnZone();
            }
            
            if (waveTimer >= waveDuration)
            {
                EndWave();
            }
        }
        else
        {
            ambientTimer += Time.deltaTime;
            intermissionTimer += Time.deltaTime;
            
            if (ambientTimer >= ambientInterval)
            {
                SpawnEnemyFrom(ambientEnemies);
                ambientTimer = 0f;
            }
            
            if (intermissionTimer >= intermissionDuration)
            {
                StartWave();
            }
        }
    }
    
    void StartWave()
    {
        currentWave++;
        inWave = true;
        waveTimer = 0f;
        batchTimer = 0f;
        PickNewSpawnZone();
        Debug.Log("Wave " + currentWave + " starting at " + currentZone.name);
    }
    
    void EndWave()
    {
        inWave = false;
        intermissionTimer = 0f;
        ambientTimer = 0f;
        
        if (spawnBatchCoroutine != null)
        {
            StopCoroutine(spawnBatchCoroutine);
            spawnBatchCoroutine = null;
        }
        
        Debug.Log("Wave ended");
    }
    
    IEnumerator SpawnBatchCoroutine()
    {
        for (int i = 0; i < enemiesPerBatch; i++)
        {
            SpawnEnemyFrom(waveEnemies);
            
            if (i < enemiesPerBatch - 1)
            {
                yield return new WaitForSeconds(enemySpawnDelay);
            }
        }
        
        spawnBatchCoroutine = null;
    }
    
    void PickNewSpawnZone()
    {
        currentZone = spawnZones[Random.Range(0, spawnZones.Length)];
    }
    
    void SpawnEnemyFrom(GameObject[] list)
    {
        if (list.Length == 0 || currentZone == null || currentZone.spawnPoints.Length == 0) return;
        
        var prefab = list[Random.Range(0, list.Length)];
        var spawnPoint = currentZone.spawnPoints[Random.Range(0, currentZone.spawnPoints.Length)];
        
        float radius = 3f;
        float minDistance = 1f;
        float distance = Random.Range(minDistance, radius);
        Vector2 offset = Random.insideUnitCircle.normalized * distance;
        Vector3 spawnPos = spawnPoint.position + new Vector3(offset.x, offset.y, 0);
        
        Instantiate(prefab, spawnPos, Quaternion.identity);
    }
    
    void OnDisable()
    {
        if (spawnBatchCoroutine != null)
        {
            StopCoroutine(spawnBatchCoroutine);
            spawnBatchCoroutine = null;
        }
    }
}