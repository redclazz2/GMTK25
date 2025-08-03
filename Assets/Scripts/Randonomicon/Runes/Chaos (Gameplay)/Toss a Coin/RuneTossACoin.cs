using UnityEngine;
using System.Collections.Generic;

public class RuneTossACoin : ActiveRuneStateBase
{
    private readonly float effectRadius = 8f; // Radio visible para el jugador
    private readonly float duplicateOffset = 1f; // Distancia de spawn de duplicados

    public RuneTossACoin(RuneStateData runeStateData) : base(runeStateData, 15f) // Cooldown largo por ser poderosa
    {
    }

    protected override bool CanTrigger()
    {
        // Solo se activa si hay enemigos en el radio
        Vector3 playerPos = owner.transform.position;
        Collider2D[] enemies = Physics2D.OverlapCircleAll(playerPos, effectRadius);

        foreach (var enemy in enemies)
        {
            if (enemy.CompareTag("Enemy"))
            {
                return true;
            }
        }
        return false;
    }

    protected override void OnTrigger()
    {
        Vector3 playerPos = owner.transform.position;

        // Encontramos todos los enemigos en el radio
        List<GameObject> enemiesInRange = new();
        Collider2D[] colliders = Physics2D.OverlapCircleAll(playerPos, effectRadius);

        foreach (var collider in colliders)
        {
            if (collider.CompareTag("Enemy"))
            {
                enemiesInRange.Add(collider.gameObject);
            }
        }

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