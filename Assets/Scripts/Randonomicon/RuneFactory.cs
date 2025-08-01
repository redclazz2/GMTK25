using UnityEngine;

public static class RuneFactory
{
    public static IRuneState Create(RuneStateData data)
    {
        switch (data.runeType)
        {
            case RuneType.ArcaneCircle:
                return new RuneArcaneCircle(data);
            case RuneType.Fireball:
                return new RuneFireball(data);
            case RuneType.ChainLightning:
                return new RuneChainLightning(data);
            default:
                Debug.LogError($"RuneFactory: tipo no reconocido {data.runeType}");
                return null;
        }
    }
}
