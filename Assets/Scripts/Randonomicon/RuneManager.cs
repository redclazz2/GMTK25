using System.Collections.Generic;
using UnityEngine;

public class RuneManager : MonoBehaviour
{
    public static RuneManager Instance { get; private set; }

    [Header("Slot 1 Runes")]
    public List<RuneStateData> slot1Runes = new();

    [Header("Slots 2 & 3 Runes")]
    public List<RuneStateData> slot2And3Runes = new();

    // Current active runes (for UI reading)
    [HideInInspector] public RuneStateData currentSlot1Rune;
    [HideInInspector] public RuneStateData currentSlot2Rune;
    [HideInInspector] public RuneStateData currentSlot3Rune;

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
        currentSlot1Rune = null;
        currentSlot2Rune = null;
        currentSlot3Rune = null;

        // Create a list to track used runes to ensure uniqueness
        List<RuneStateData> usedRunes = new();

        // Roll slot 1 rune
        RollAndAddRune(slot1Runes, ref currentSlot1Rune, "Slot 1", usedRunes);

        // Roll slots 2 and 3 runes (using the same list but ensuring uniqueness)
        RollAndAddRune(slot2And3Runes, ref currentSlot2Rune, "Slot 2", usedRunes);
        RollAndAddRune(slot2And3Runes, ref currentSlot3Rune, "Slot 3", usedRunes);

        Debug.Log("RuneManager: New runes rolled successfully!");
    }

    private void RollAndAddRune(List<RuneStateData> runeList, ref RuneStateData currentRune, string slotName, List<RuneStateData> usedRunes)
    {
        // Skip if list is empty
        if (runeList.Count == 0)
        {
            Debug.Log($"RuneManager: Skipping {slotName} - list is empty");
            return;
        }

        // Create a list of available runes (excluding already used ones)
        List<RuneStateData> availableRunes = new();
        foreach (var rune in runeList)
        {
            if (!usedRunes.Contains(rune))
            {
                availableRunes.Add(rune);
            }
        }

        // If no available runes, skip this slot
        if (availableRunes.Count == 0)
        {
            Debug.Log($"RuneManager: Skipping {slotName} - no unique runes available");
            return;
        }

        // Roll random rune from available runes
        RuneStateData rolledRune = availableRunes[Random.Range(0, availableRunes.Count)];
        currentRune = rolledRune;
        usedRunes.Add(rolledRune); // Mark as used

        // Create and add the rune state
        IRuneState runeState = RuneFactory.Create(rolledRune);
        if (runeState != null)
        {
            playerRuneComponent.AddState(runeState);
            Debug.Log($"RuneManager: Added {slotName} rune: {rolledRune.runeName}");
        }
        else
        {
            Debug.LogError($"RuneManager: Failed to create rune state for {rolledRune.runeName}");
        }
    }

    // Public getters for UI access
    public static RuneStateData GetCurrentSlot1Rune() => Instance?.currentSlot1Rune;
    public static RuneStateData GetCurrentSlot2Rune() => Instance?.currentSlot2Rune;
    public static RuneStateData GetCurrentSlot3Rune() => Instance?.currentSlot3Rune;

    // Get all current runes as array (useful for UI)
    public static RuneStateData[] GetAllCurrentRunes()
    {
        if (Instance == null) return new RuneStateData[0];

        List<RuneStateData> runes = new();

        if (Instance.currentSlot1Rune != null) runes.Add(Instance.currentSlot1Rune);
        if (Instance.currentSlot2Rune != null) runes.Add(Instance.currentSlot2Rune);
        if (Instance.currentSlot3Rune != null) runes.Add(Instance.currentSlot3Rune);

        return runes.ToArray();
    }

    // Get current runes by slot index (0, 1, 2)
    public static RuneStateData GetRuneBySlot(int slotIndex)
    {
        if (Instance == null) return null;

        return slotIndex switch
        {
            0 => Instance.currentSlot1Rune,
            1 => Instance.currentSlot2Rune,
            2 => Instance.currentSlot3Rune,
            _ => null
        };
    }
}