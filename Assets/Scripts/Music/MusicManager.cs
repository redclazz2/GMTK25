using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance { get; private set; }

    // Active looping AudioSources, indexed by key
    private readonly Dictionary<string, AudioSource> _loopSources = new();

    // All coroutines used for fading, so we can stop them if needed
    private readonly Dictionary<string, Coroutine> _fadeCoroutines = new();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>Play a clip in loop, or restart if already playing.</summary>
    public void PlayLoop(string key, AudioClip clip, float volume = 1f)
    {
        if (_loopSources.TryGetValue(key, out var src))
        {
            // Already exists: restart
            src.clip = clip;
            src.volume = volume;
            if (!src.isPlaying) src.Play();
        }
        else
        {
            var go = new GameObject($"Loop_{key}");
            go.transform.SetParent(transform);
            var audio = go.AddComponent<AudioSource>();
            audio.clip = clip;
            audio.loop = true;
            audio.volume = volume;
            audio.Play();
            _loopSources[key] = audio;
        }
    }

    /// <summary>Play a one-shot SFX at manager’s AudioSource.</summary>
    public void PlayOneShot(AudioClip clip, float volume = 1f)
    {
        // We can use a temporary AudioSource
        var go = new GameObject("OneShot");
        go.transform.SetParent(transform);
        var audio = go.AddComponent<AudioSource>();
        audio.clip = clip;
        audio.volume = volume;
        audio.Play();
        StartCoroutine(DestroyWhenFinished(go, clip.length));
    }

    /// <summary>Stop and destroy the loop track with this key immediately.</summary>
    public void StopLoop(string key)
    {
        if (_loopSources.TryGetValue(key, out var src))
        {
            src.Stop();
            Destroy(src.gameObject);
            _loopSources.Remove(key);
            StopFadeCoroutine(key);
        }
    }

    /// <summary>Fade out a loop track over duration seconds, then stop it.</summary>
    public void FadeOutLoop(string key, float duration)
    {
        if (_loopSources.TryGetValue(key, out var src))
        {
            StopFadeCoroutine(key);
            _fadeCoroutines[key] = StartCoroutine(FadeOutAndStop(key, src, duration));
        }
    }

    /// <summary>Fade out all loop tracks over duration, then stop them.</summary>
    public void FadeOutAll(float duration)
    {
        foreach (var kv in new Dictionary<string, AudioSource>(_loopSources))
        {
            FadeOutLoop(kv.Key, duration);
        }
    }

    /// <summary>Stop and clear all loop tracks immediately.</summary>
    public void ClearAll()
    {
        foreach (var kv in _loopSources)
        {
            if (kv.Value != null) Destroy(kv.Value.gameObject);
        }
        _loopSources.Clear();

        foreach (var co in _fadeCoroutines.Values)
            if (co != null) StopCoroutine(co);
        _fadeCoroutines.Clear();
    }

    // Helpers

    private IEnumerator DestroyWhenFinished(GameObject go, float delay)
    {
        yield return new WaitForSeconds(delay);
        Destroy(go);
    }

    private IEnumerator FadeOutAndStop(string key, AudioSource src, float duration)
    {
        float startVol = src.volume;
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            src.volume = Mathf.Lerp(startVol, 0f, t / duration);
            yield return null;
        }
        StopLoop(key);
    }

    private void StopFadeCoroutine(string key)
    {
        if (_fadeCoroutines.TryGetValue(key, out var co))
        {
            if (co != null) StopCoroutine(co);
            _fadeCoroutines.Remove(key);
        }
    }
}