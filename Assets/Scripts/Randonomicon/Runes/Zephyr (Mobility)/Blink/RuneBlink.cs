using UnityEngine;
using System.Linq;

public class RuneBlink : ActiveRuneStateBase
{
    private readonly float detectionRadius = 8f;
    private readonly float regenerationDuration = 3f;

    private float regenerationTimer = 0f;
    private float currentRegenerationBonus = 0f;
    private bool hasActiveRegeneration = false;

    public RuneBlink(RuneStateData runeStateData) : base(runeStateData, 8f) // 8 segundos de cooldown
    {
    }

    public override void Tick(float dt)
    {
        base.Tick(dt);

        // Manejar el temporizador de regeneración
        if (hasActiveRegeneration)
        {
            regenerationTimer -= dt;

            if (regenerationTimer <= 0f)
            {
                RemoveRegenerationBonus();
            }
        }
    }

    protected override bool CanTrigger()
    {
        Vector3 playerPos = owner.transform.position;
        Collider2D[] enemies = Physics2D.OverlapCircleAll(playerPos, detectionRadius);

        // Verificar que hay al menos un enemigo en rango
        for (int i = 0; i < enemies.Length; i++)
        {
            if (enemies[i].CompareTag("Enemy"))
            {
                return true;
            }
        }

        return false;
    }

    protected override void OnTrigger()
    {
        Transform playerTransform = owner.transform;
        Vector3 playerPos = playerTransform.position;

        // Encontrar todos los enemigos en rango
        GameObject[] allEnemies = GameObject.FindGameObjectsWithTag("Enemy");
        var enemiesInRange = allEnemies
            .Where(enemy => Vector2.Distance(playerPos, enemy.transform.position) <= detectionRadius)
            .ToArray();

        if (enemiesInRange.Length == 0)
            return;

        // Seleccionar un enemigo aleatorio
        GameObject targetEnemy = enemiesInRange[Random.Range(0, enemiesInRange.Length)];
        Transform enemyTransform = targetEnemy.transform;

        // Guardar las posiciones originales
        Vector3 originalPlayerPos = playerTransform.position;
        Vector3 originalEnemyPos = enemyTransform.position;

        // Realizar el intercambio de posiciones
        playerTransform.position = originalEnemyPos;
        enemyTransform.position = originalPlayerPos;

        // Aplicar la regeneración temporal al jugador
        ApplyTemporaryRegeneration();

        Debug.Log($"Position swapped with {targetEnemy.name}! Temporary regeneration applied.");
    }

    private void ApplyTemporaryRegeneration()
    {
        // Si ya hay regeneración activa, removerla primero
        if (hasActiveRegeneration)
        {
            RemoveRegenerationBonus();
        }

        StatsComponent statsComponent = StatsComponent.Get(owner);

        // La regeneración es el 5% de la vida máxima actual
        currentRegenerationBonus = statsComponent.currentStats.maxHealth * 0.05f;

        // Aplicar la regeneración temporalmente
        statsComponent.ApplyModifiers(new StatsBundle
        {
            regeneration = currentRegenerationBonus
        });

        // Iniciar el temporizador
        regenerationTimer = regenerationDuration;
        hasActiveRegeneration = true;

        Debug.Log($"Temporary regeneration applied: +{currentRegenerationBonus} for {regenerationDuration} seconds");
    }

    private void RemoveRegenerationBonus()
    {
        if (!hasActiveRegeneration)
            return;

        StatsComponent statsComponent = StatsComponent.Get(owner);
        if (statsComponent != null)
        {
            statsComponent.ApplyModifiers(new StatsBundle
            {
                regeneration = -currentRegenerationBonus
            });

            Debug.Log($"Temporary regeneration removed: -{currentRegenerationBonus}");
        }

        hasActiveRegeneration = false;
        currentRegenerationBonus = 0f;
        regenerationTimer = 0f;
    }

    public override void Exit()
    {
        // Asegurar que se remueva la regeneración al salir de la runa
        if (hasActiveRegeneration)
        {
            RemoveRegenerationBonus();
        }

        base.Exit();
    }
}