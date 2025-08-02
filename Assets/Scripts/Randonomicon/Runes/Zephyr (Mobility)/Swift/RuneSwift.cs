using UnityEngine;

public class RuneSwift : PassiveRuneStateBase
{
    private float lastCooldownReduction = 0f;
    private float moveSpeedBonus = 0f;

    public RuneSwift(RuneStateData runeStateData) : base(runeStateData)
    {
    }

    public override void Enter(GameObject owner)
    {
        this.owner = owner;
        
        // Primero aplicar el 15% de velocidad de movimiento adicional
        StatsComponent statsComponent = StatsComponent.Get(owner);
        float baseMoveSpeed = statsComponent.currentStats.moveSpeed;
        moveSpeedBonus = baseMoveSpeed * 0.15f;
        
        // Aplicar el bono de velocidad y los modificadores base de la runa
        StatsBundle initialModifiers = _stateData.statDelta;
        initialModifiers.moveSpeed += moveSpeedBonus;
        statsComponent.ApplyModifiers(initialModifiers);
        
        // Luego aplicar la reducción de enfriamiento basada en la velocidad total
        UpdateCooldownReduction();
    }

    public override void Tick(float dt)
    {
        base.Tick(dt);
        
        // Verificar si necesitamos actualizar la reducción de enfriamiento
        StatsComponent statsComponent = StatsComponent.Get(owner);
        float currentMoveSpeed = statsComponent.currentStats.moveSpeed;
        float expectedCooldownReduction = currentMoveSpeed;
        
        if (!Mathf.Approximately(expectedCooldownReduction, lastCooldownReduction))
        {
            UpdateCooldownReduction();
        }
    }

    private void UpdateCooldownReduction()
    {
        StatsComponent statsComponent = StatsComponent.Get(owner);
        
        // Revertir la reducción anterior si existía
        if (lastCooldownReduction > 0f)
        {
            statsComponent.ApplyModifiers(new StatsBundle 
            { 
                cooldownReduction = -lastCooldownReduction 
            });
        }
        
        // Aplicar la nueva reducción basada en la velocidad total actual
        float currentMoveSpeed = statsComponent.currentStats.moveSpeed;
        if (currentMoveSpeed > 0f)
        {
            statsComponent.ApplyModifiers(new StatsBundle 
            { 
                cooldownReduction = currentMoveSpeed * 0.5f
            });
            lastCooldownReduction = currentMoveSpeed * 0.5f;
        }
        else
        {
            lastCooldownReduction = 0f;
        }
    }

    public override void Exit()
    {
        StatsComponent statsComponent = StatsComponent.Get(owner);
        
        // Revertir la reducción de enfriamiento
        if (lastCooldownReduction > 0f)
        {
            statsComponent.ApplyModifiers(new StatsBundle 
            { 
                cooldownReduction = -lastCooldownReduction 
            });
        }
        
        // Revertir los modificadores base incluyendo el bono de velocidad
        StatsBundle exitModifiers = _stateData.statDelta;
        exitModifiers.moveSpeed += moveSpeedBonus;
        statsComponent.ApplyModifiers(-exitModifiers);
    }
}