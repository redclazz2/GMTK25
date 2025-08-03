using UnityEngine;
using System.Collections.Generic;

public class RuneTossACoin : ActiveRuneStateBase
{
    private readonly float duplicateOffset = 1f; // Distancia de spawn de duplicados
    private Camera playerCamera;

    public RuneTossACoin(RuneStateData runeStateData) : base(runeStateData, 15f) // Cooldown largo por ser poderosa
    {
    }

    public override void Enter(GameObject owner)
    {
        base.Enter(owner);
        // Obtener la cámara principal
        playerCamera = Camera.main;
        if (playerCamera == null)
            playerCamera = GameObject.FindFirstObjectByType<Camera>();
    }

    public override void Tick(float dt)
    {
        // Original tick logic for cooldown
        base.Tick(dt);
    }

    protected override bool CanTrigger()
    {
        // Solo se activa si hay enemigos visibles en pantalla
        List<GameObject> visibleEnemies = GetVisibleEnemies();
        return visibleEnemies.Count > 0;
    }

    protected override void OnTrigger()
    {
        ExecuteEffect();
    }

    private void ExecuteEffect()
    {
        // Obtener enemigos visibles en el momento de la ejecución
        List<GameObject> enemiesInRange = GetVisibleEnemies();

        if (enemiesInRange.Count == 0) return;

        // 50% de probabilidad para cada resultado
        float randomChance = Random.Range(0f, 1f);

        if (randomChance < 0.5f)
        {
            // ELIMINAR todos los enemigos
            EliminateEnemies(enemiesInRange);
        }
        else
        {
            // DUPLICAR todos los enemigos
            DuplicateEnemies(enemiesInRange);
        }
    }

    private List<GameObject> GetVisibleEnemies()
    {
        List<GameObject> visibleEnemies = new();

        if (playerCamera == null) return visibleEnemies;

        // Obtener todos los enemigos con tag "Enemy"
        GameObject[] allEnemies = GameObject.FindGameObjectsWithTag("Enemy");

        foreach (GameObject enemy in allEnemies)
        {
            if (enemy != null && IsEnemyVisible(enemy))
            {
                visibleEnemies.Add(enemy);
            }
        }

        return visibleEnemies;
    }

    private bool IsEnemyVisible(GameObject enemy)
    {
        if (playerCamera == null || enemy == null) return false;

        // Convertir posición del enemigo a coordenadas de viewport
        Vector3 viewportPoint = playerCamera.WorldToViewportPoint(enemy.transform.position);

        // Verificar si está dentro de los límites de la pantalla
        // viewport coordinates: (0,0) = bottom-left, (1,1) = top-right
        return viewportPoint.x >= 0 && viewportPoint.x <= 1 &&
               viewportPoint.y >= 0 && viewportPoint.y <= 1 &&
               viewportPoint.z > 0; // z > 0 means it's in front of the camera
    }

    private void EliminateEnemies(List<GameObject> enemies)
    {
        int eliminatedCount = 0;
        foreach (GameObject enemy in enemies)
        {
            if (enemy != null)
            {
                GameObject.Destroy(enemy);
                eliminatedCount++;
            }
        }
        Debug.Log($"[ChaosGambit] ELIMINATION! Destroyed {eliminatedCount} enemies");
    }

    private void DuplicateEnemies(List<GameObject> enemies)
    {
        int duplicatedCount = 0;
        foreach (GameObject enemy in enemies)
        {
            if (enemy != null)
            {
                // Calculamos posición para el duplicado (cerca del original)
                Vector2 randomOffset = Random.insideUnitCircle.normalized * duplicateOffset;
                Vector3 duplicatePos = enemy.transform.position + (Vector3)randomOffset;

                // Creamos el duplicado
                GameObject duplicate = GameObject.Instantiate(enemy, duplicatePos, enemy.transform.rotation);
                duplicatedCount++;
            }
        }
        Debug.Log($"[ChaosGambit] DUPLICATION! Created {duplicatedCount} enemy duplicates");
    }
}