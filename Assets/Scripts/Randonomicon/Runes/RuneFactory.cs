using UnityEngine;

public static class RuneFactory
{
    public static IRuneState Create(RuneStateData data)
    {
        switch (data.runeType)
        {
            default:
                Debug.LogError($"RuneFactory: tipo no reconocido {data.runeType}");
                return null;
        }
    }
}
