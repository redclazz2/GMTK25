using System.Threading.Tasks;
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

    private float ambientTimer = 0f;
    private float batchTimer = 0f;
    private float waveTimer = 0f;
    private float intermissionTimer = 0f;

    private bool inWave = false;
    private int currentWave = 0;
    private SpawnZone currentZone;

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
                SpawnBatch();
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
        Debug.Log("Wave ended");
    }

    async void SpawnBatch()
    {
        for (int i = 0; i < enemiesPerBatch; i++)
        {
            SpawnEnemyFrom(waveEnemies);
            await Task.Delay(1200);
        }
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
}
