using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RuneUIManager : MonoBehaviour
{
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI descriptionText;
    public Image iconImage;
    public CanvasGroup canvasGroup;

    [SerializeField] private float fadeDuration = 0.5f;

    private static RuneUIManager instance;
    private Coroutine displayCoroutine;

    private void Awake()
    {
        instance = this;
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
        }
    }

    public static void StartDisplayingRunes(List<RuneStateData> runes, float offsetTime)
    {
        if (instance == null)
        {
            Debug.LogWarning("RuneUIManager instance not found.");
            return;
        }

        if (instance.displayCoroutine != null)
        {
            instance.StopCoroutine(instance.displayCoroutine);
        }

        instance.displayCoroutine = instance.StartCoroutine(instance.DisplayRunes(runes, offsetTime));
    }

    private IEnumerator DisplayRunes(List<RuneStateData> runes, float offsetTime)
    {
        foreach (var rune in runes)
        {
            // Set UI data
            if (iconImage != null) iconImage.sprite = rune.icon;
            if (titleText != null) titleText.text = rune.name;
            if (descriptionText != null) descriptionText.text = rune.description;

            // Fade In
            yield return StartCoroutine(FadeCanvasGroup(0f, 1f, fadeDuration));

            // Stay visible for offsetTime seconds
            yield return new WaitForSeconds(offsetTime);

            // Fade Out
            yield return StartCoroutine(FadeCanvasGroup(1f, 0f, fadeDuration));
        }

        // Optionally clear after all done
        if (iconImage != null) iconImage.sprite = null;
        if (titleText != null) titleText.text = "";
        if (descriptionText != null) descriptionText.text = "";
    }

    private IEnumerator FadeCanvasGroup(float from, float to, float duration)
    {
        float timer = 0f;
        while (timer < duration)
        {
            float t = timer / duration;
            if (canvasGroup != null)
                canvasGroup.alpha = Mathf.Lerp(from, to, t);

            timer += Time.deltaTime;
            yield return null;
        }

        if (canvasGroup != null)
            canvasGroup.alpha = to;
    }
}
