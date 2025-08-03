using System.Collections.Generic;
using UnityEngine;

public class RuneManager : MonoBehaviour
{
    public static RuneManager Instance { get; private set; }

    [Header("Fury (Damage Runes)")]
    public List<RuneStateData> furyRunes = new();

    [Header("Zephyr (Mobility Runes)")]
    public List<RuneStateData> zephyrRunes = new();

    [Header("Chaos (Gameplay Runes)")]
    public List<RuneStateData> chaosRunes = new();

    [Header("Bulwark (Survivability Runes)")]
    public List<RuneStateData> bulwarkRunes = new();

    [Header("Colossus (Size Runes)")]
    public List<RuneStateData> colossusRunes = new();

    [Header("Chains (Crowd Control Runes)")]
    public List<RuneStateData> chainsRunes = new();

    [Header("Chaos Extra Slots")]
    public List<RuneStateData> chaosExtraRunes = new();

    [Header("Settings")]
    public bool runeChaos = false;

    // Current active runes (for UI reading)
    [HideInInspector] public RuneStateData currentFuryRune;
    [HideInInspector] public RuneStateData currentZephyrRune;
    [HideInInspector] public RuneStateData currentChaosRune;
    [HideInInspector] public RuneStateData currentBulwarkRune;
    [HideInInspector] public RuneStateData currentColossusRune;
    [HideInInspector] public RuneStateData currentChainsRune;
    [HideInInspector] public RuneStateData currentChaosExtra1;
    [HideInInspector] public RuneStateData currentChaosExtra2;

    private GameObject playerObject;
    private RuneComponent playerRuneComponent;

    void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        FindPlayer();
    }

    private void FindPlayer()
    {
        playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            playerRuneComponent = playerObject.GetComponent<RuneComponent>();
            if (playerRuneComponent == null)
            {
                playerRuneComponent = playerObject.AddComponent<RuneComponent>();
            }
        }
        else
        {
            Debug.LogError("RuneManager: No GameObject with tag 'Player' found!");
        }
    }

    public static void RollNewRunes()
    {
        if (Instance == null)
        {
            Debug.LogError("RuneManager: Instance is null!");
            return;
        }

        Instance.RollNewRunesInternal();
    }

    private void RollNewRunesInternal()
    {
        // Ensure we have player reference
        if (playerObject == null || playerRuneComponent == null)
        {
            FindPlayer();
            if (playerObject == null || playerRuneComponent == null)
            {
                Debug.LogError("RuneManager: Player not found or missing RuneComponent!");
                return;
            }
        }

        // Clear all current runes
        playerRuneComponent.ClearAll();

        // Reset current rune references
        currentFuryRune = null;
        currentZephyrRune = null;
        currentChaosRune = null;
        currentBulwarkRune = null;
        currentColossusRune = null;
        currentChainsRune = null;
        currentChaosExtra1 = null;
        currentChaosExtra2 = null;

        // Roll and add new runes for each category
        RollAndAddRune(furyRunes, ref currentFuryRune, "Fury");
        RollAndAddRune(zephyrRunes, ref currentZephyrRune, "Zephyr");
        RollAndAddRune(chaosRunes, ref currentChaosRune, "Chaos");
        RollAndAddRune(bulwarkRunes, ref currentBulwarkRune, "Bulwark");
        RollAndAddRune(colossusRunes, ref currentColossusRune, "Colossus");
        RollAndAddRune(chainsRunes, ref currentChainsRune, "Chains");

        // Roll chaos extra slots if runeChaos is enabled
        if (runeChaos)
        {
            RollAndAddRune(chaosExtraRunes, ref currentChaosExtra1, "Chaos Extra 1");
            RollAndAddRune(chaosExtraRunes, ref currentChaosExtra2, "Chaos Extra 2");
        }

        Debug.Log("RuneManager: New runes rolled successfully!");
    }

    private void RollAndAddRune(List<RuneStateData> runeList, ref RuneStateData currentRune, string categoryName)
    {
        // Skip if list is empty
        if (runeList.Count == 0)
        {
            Debug.Log($"RuneManager: Skipping {categoryName} - list is empty");
            return;
        }

        // Roll random rune from the list
        RuneStateData rolledRune = runeList[Random.Range(0, runeList.Count)];
        currentRune = rolledRune;

        // Create and add the rune state
        IRuneState runeState = RuneFactory.Create(rolledRune);
        if (runeState != null)
        {
            playerRuneComponent.AddState(runeState);
            Debug.Log($"RuneManager: Added {categoryName} rune: {rolledRune.runeName}");
        }
        else
        {
            Debug.LogError($"RuneManager: Failed to create rune state for {rolledRune.runeName}");
        }
    }

    // Public getters for UI access
    public static RuneStateData GetCurrentFuryRune() => Instance?.currentFuryRune;
    public static RuneStateData GetCurrentZephyrRune() => Instance?.currentZephyrRune;
    public static RuneStateData GetCurrentChaosRune() => Instance?.currentChaosRune;
    public static RuneStateData GetCurrentBulwarkRune() => Instance?.currentBulwarkRune;
    public static RuneStateData GetCurrentColossusRune() => Instance?.currentColossusRune;
    public static RuneStateData GetCurrentChainsRune() => Instance?.currentChainsRune;
    public static RuneStateData GetCurrentChaosExtra1() => Instance?.currentChaosExtra1;
    public static RuneStateData GetCurrentChaosExtra2() => Instance?.currentChaosExtra2;

    // Get all current runes as array (useful for UI)
    public static RuneStateData[] GetAllCurrentRunes()
    {
        if (Instance == null) return new RuneStateData[0];

        List<RuneStateData> runes = new();

        if (Instance.currentFuryRune != null) runes.Add(Instance.currentFuryRune);
        if (Instance.currentZephyrRune != null) runes.Add(Instance.currentZephyrRune);
        if (Instance.currentChaosRune != null) runes.Add(Instance.currentChaosRune);
        if (Instance.currentBulwarkRune != null) runes.Add(Instance.currentBulwarkRune);
        if (Instance.currentColossusRune != null) runes.Add(Instance.currentColossusRune);
        if (Instance.currentChainsRune != null) runes.Add(Instance.currentChainsRune);

        if (Instance.runeChaos)
        {
            if (Instance.currentChaosExtra1 != null) runes.Add(Instance.currentChaosExtra1);
            if (Instance.currentChaosExtra2 != null) runes.Add(Instance.currentChaosExtra2);
        }

        return runes.ToArray();
    }

    public static void SetRuneChaos(bool enabled)
    {
        if (Instance != null)
        {
            Instance.runeChaos = enabled;
        }
    }

    public static bool GetRuneChaos()
    {
        return Instance?.runeChaos ?? false;
    }
}