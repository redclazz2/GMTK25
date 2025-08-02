using UnityEngine;

public class RuneBerserk : PassiveRuneStateBase
{
    private StatsComponent stats;
    private float currentBerserkBonus = 0f; // Solo el bonus actual de Berserk
    private float lastHealthPercentage = -1f; // Para detectar cambios

    public RuneBerserk(RuneStateData runeStateData) : base(runeStateData)
    {
    }

    public override void Enter(GameObject owner)
    {
        base.Enter(owner);
        stats = StatsComponent.Get(owner);
        currentBerserkBonus = 0f; // Inicializamos sin bonus
        UpdateLifeStealBonus(); // Aplicamos el cálculo inicial
    }

    public override void Tick(float dt)
    {
        base.Tick(dt);

        // Solo recalculamos si la salud cambió significativamente
        float currentHealthPercentage = stats.currentStats.health / stats.currentStats.maxHealth;

        // Usamos un pequeño umbral para evitar recálculos innecesarios
        if (Mathf.Abs(currentHealthPercentage - lastHealthPercentage) > 0.01f)
        {
            UpdateLifeStealBonus();
            lastHealthPercentage = currentHealthPercentage;
        }
    }

    private void UpdateLifeStealBonus()
    {
        if (stats.currentStats.maxHealth <= 0) return;

        float healthPercentage = stats.currentStats.health / stats.currentStats.maxHealth;
        float missingHealthPercentage = 1f - healthPercentage;

        // Calculamos el nuevo bonus de Berserk
        float newBerserkBonus;
        if (missingHealthPercentage <= 0.01f) // Vida completa
        {
            newBerserkBonus = 0f;
        }
        else
        {
            newBerserkBonus = missingHealthPercentage * 4f;
        }

        // Solo aplicamos el cambio si es diferente al actual
        if (Mathf.Abs(newBerserkBonus - currentBerserkBonus) > 0.001f)
        {
            // Removemos el bonus anterior
            stats.currentStats.lifeSteal -= currentBerserkBonus;

            // Aplicamos el nuevo bonus
            stats.currentStats.lifeSteal += newBerserkBonus;

            // Actualizamos nuestro registro
            currentBerserkBonus = newBerserkBonus;
        }
    }

    public override void Exit()
    {
        // Removemos solo nuestro bonus antes de salir
        if (stats != null)
        {
            stats.currentStats.lifeSteal -= currentBerserkBonus;
        }

        base.Exit(); // Esto removerá los statDelta del ScriptableObject si los hay
    }
}