using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class Light2DFade : MonoBehaviour
{
    public Light2D targetLight;
    public float targetIntensity = 1f;
    public float fadeDuration = 1f;

    /// <summary>
    /// Starts fading the light intensity from 0 to targetIntensity.
    /// </summary>
    public void StartFade()
    {
        if (targetLight != null)
        {
            StopAllCoroutines(); // optional, avoids overlapping fades
            StartCoroutine(FadeIn());
        }
    }

    /// <summary>
    /// Instantly sets the light intensity to 0.
    /// </summary>
    public void ResetLight()
    {
        if (targetLight != null)
        {
            StopAllCoroutines();
            targetLight.intensity = 0f;
        }
    }

    private IEnumerator FadeIn()
    {
        float startIntensity = 0f;
        float time = 0f;

        targetLight.intensity = 0f;

        while (time < fadeDuration)
        {
            float t = time / fadeDuration;
            targetLight.intensity = Mathf.Lerp(startIntensity, targetIntensity, t);
            time += Time.deltaTime;
            yield return null;
        }

        targetLight.intensity = targetIntensity;
    }
}
