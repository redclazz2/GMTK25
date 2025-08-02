using UnityEngine;

public static class RuneFactory
{
    public static IRuneState Create(RuneStateData data)
    {
        switch (data.runeType)
        {
            // Dmg
            case RuneType.ArcaneCircle:
                return new RuneArcaneCircle(data);
            case RuneType.Fireball:
                return new RuneFireball(data);
            case RuneType.ChainLightning:
                return new RuneChainLightning(data);
            // Mov
            case RuneType.FastFairy:
                return new RuneFairy(data);
            case RuneType.Swift:
                return new RuneSwift(data);
            case RuneType.Blink:
                return new RuneBlink(data);
            // Gameplay
            case RuneType.GoldenWand:
                return new RuneGoldenWand(data);
            // Survival
            case RuneType.Berserk:
                return new RuneBerserk(data);
            // Size
            case RuneType.GiantSlayer:
                return new RuneGiantSlayer(data);
            default:
                Debug.LogError($"RuneFactory: tipo no reconocido {data.runeType}");
                return null;
        }
    }
}
