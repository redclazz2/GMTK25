using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewRuneStateData", menuName = "Randonomicon/Rune State Data")]
public class RuneStateData : ScriptableObject
{
    [Header("Identidad")]
    public string runeName;
    public Sprite icon;
    [TextArea(2, 5)] public string description;

    [Header("Identificador de la runa")]
    public RuneType runeType;

    [Header("Modificadores de stats")]
    public StatsBundle statDelta;

    [Header("Prefabs necesarios")]
    public List<PrefabEntry> prefabEntries;

    [Header("Audio Clips")]
    public List<AudioEntry> audioEntries;

    [System.Serializable]
    public class PrefabEntry
    {
        public string key;
        public GameObject prefab;
    }

    [System.Serializable]
    public class AudioEntry
    {
        public string key;
        public AudioClip audioClip;
    }

    private Dictionary<string, GameObject> _prefabDict;
    private Dictionary<string, AudioClip> _audioDict;

    public GameObject GetPrefab(string key)
    {
        if (_prefabDict == null)
        {
            _prefabDict = new Dictionary<string, GameObject>();
            foreach (var e in prefabEntries)
                if (!_prefabDict.ContainsKey(e.key))
                    _prefabDict.Add(e.key, e.prefab);
        }
        _prefabDict.TryGetValue(key, out var p);
        return p;
    }

    public AudioClip GetAudioClip(string key)
    {
        if (_audioDict == null)
        {
            _audioDict = new Dictionary<string, AudioClip>();
            foreach (var e in audioEntries)
                if (!_audioDict.ContainsKey(e.key))
                    _audioDict.Add(e.key, e.audioClip);
        }
        _audioDict.TryGetValue(key, out var clip);
        return clip;
    }
}