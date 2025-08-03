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

    [SerializeField] private float fadeDuration = 0.5f;

    private static RuneUIManager instance;
    private Coroutine displayCoroutine;

    private void Awake()
    {
        instance = this;
        SetAlpha(titleText, 0f);
        SetAlpha(descriptionText, 0f);
        SetAlpha(iconImage, 0f);
    }

    public static void StartDisplayingRunes(RuneStateData[] runes, float offsetTime)
    {
        if (instance == null)
        {
            Debug.LogWarning("RuneUIManager instance not found in the scene.");
            return;
        }

        if (instance.displayCoroutine != null)
        {
            instance.StopCoroutine(instance.displayCoroutine);
        }

        instance.displayCoroutine = instance.StartCoroutine(instance.DisplayRunes(runes, offsetTime));
    }

    private IEnumerator DisplayRunes(RuneStateData[] runes, float offsetTime)
    {
        foreach (var rune in runes)
        {
            // Apply rune data
            titleText.text = rune.runeName;
            descriptionText.text = rune.description;
            iconImage.sprite = rune.icon;

            // Simultaneous fade in
            yield return StartCoroutine(FadeAll(0f, 1f));

            // Display duration
            yield return new WaitForSeconds(offsetTime);

            // Simultaneous fade out
            yield return StartCoroutine(FadeAll(1f, 0f));
        }

        // Optional cleanup
        titleText.text = "";
        descriptionText.text = "";
        iconImage.sprite = null;
    }

    private IEnumerator FadeAll(float from, float to)
    {
        float timer = 0f;
        Color tColor = titleText.color;
        Color dColor = descriptionText.color;
        Color iColor = iconImage.color;

        while (timer < fadeDuration)
        {
            float t = timer / fadeDuration;
            float alpha = Mathf.Lerp(from, to, t);

            tColor.a = alpha;
            dColor.a = alpha;
            iColor.a = alpha;

            titleText.color = tColor;
            descriptionText.color = dColor;
            iconImage.color = iColor;

            timer += Time.deltaTime;
            yield return null;
        }

        tColor.a = to;
        dColor.a = to;
        iColor.a = to;

        titleText.color = tColor;
        descriptionText.color = dColor;
        iconImage.color = iColor;
    }

    private void SetAlpha(Graphic element, float alpha)
    {
        Color c = element.color;
        c.a = alpha;
        element.color = c;
    }
}
