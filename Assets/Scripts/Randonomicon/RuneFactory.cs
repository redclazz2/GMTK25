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
            case RuneType.TossACoin:
                return new RuneTossACoin(data);
            // Survival
            case RuneType.Berserk:
                return new RuneBerserk(data);
            case RuneType.Goliat:
                return new RuneGoliat(data);
            // Size
            case RuneType.GiantSlayer:
                return new RuneGiantSlayer(data);
            // Crowd Control
            case RuneType.TargetPractice:
                return new RuneTargetPractice(data);
            case RuneType.Transmute:
                return new RuneTransmute(data);
            default:
                Debug.LogError($"RuneFactory: tipo no reconocido {data.runeType}");
                return null;
        }
    }
}
