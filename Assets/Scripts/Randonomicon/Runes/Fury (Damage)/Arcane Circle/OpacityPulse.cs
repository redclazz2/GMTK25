using UnityEngine;
using UnityEngine.Rendering.Universal;

[RequireComponent(typeof(SpriteRenderer))]
public class OpacityAndLightPulse : MonoBehaviour
{
    [Header("Pulse Settings")]
    [Tooltip("Time (in seconds) to go from max to min and back to max.")]
    public float interval = 1f;

    [Tooltip("Minimum sprite alpha (0 = transparent, 1 = opaque).")]
    [Range(0f, 1f)]
    public float minAlpha = 0.2f;

    [Tooltip("Maximum sprite alpha (0 = transparent, 1 = opaque).")]
    [Range(0f, 1f)]
    public float maxAlpha = 1f;

    [Header("Light2D Settings")]
    [Tooltip("Reference to the 2D Light whose intensity will pulse.")]
    public Light2D light2d;

    [Tooltip("Minimum light intensity.")]
    public float minIntensity = 0.5f;

    [Tooltip("Maximum light intensity.")]
    public float maxIntensity = 2f;

    private SpriteRenderer _sprite;
    private float _halfInterval;

    void Awake()
    {
        _sprite = GetComponent<SpriteRenderer>();
        if (light2d == null)
            Debug.LogWarning("OpacityAndLightPulse: no Light2D assigned", this);

        _halfInterval = interval / 2f;
    }

    void Update()
    {
        if (interval <= 0f) return;

        float t = Mathf.PingPong(Time.time / _halfInterval, 1f);

        // Lerp sprite alpha
        float alpha = Mathf.Lerp(minAlpha, maxAlpha, t);
        Color c = _sprite.color;
        c.a = alpha;
        _sprite.color = c;

        // Lerp light intensity, if assigned
        if (light2d != null)
        {
            light2d.intensity = Mathf.Lerp(minIntensity, maxIntensity, t);
        }
    }
}
