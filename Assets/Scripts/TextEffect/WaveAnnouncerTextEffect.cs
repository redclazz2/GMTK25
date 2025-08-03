using TMPro;
using UnityEngine;
using System.Collections;

public class WaveAnnouncerTextEffect : MonoBehaviour
{
    [Header("Text Settings")]
    public TMP_Text textComponent;

    [Header("Animation Settings")]
    [SerializeField] private float characterRevealDelay = 0.08f;
    [SerializeField] private float fadeInDuration = 0.4f;
    [SerializeField] private float popInScale = 1.3f;
    [SerializeField] private float wobbleDuration = 1.2f;
    [SerializeField] private float wobbleStrength = 15f;
    [SerializeField] private float wobbleSpeed = 8f;
    [SerializeField] private float rotationWobble = 5f;
    [SerializeField] private float moveToTopDuration = 1f;
    [SerializeField] private AnimationCurve moveCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    private string originalText;
    private Vector3 originalPosition;
    private bool effectStarted = false;
    private bool textHidden = false;

    void Start()
    {
        if (textComponent == null)
            textComponent = GetComponent<TMP_Text>();

        originalText = textComponent.text;
        originalPosition = transform.position;

        // Immediately set alpha to 0 using the text component's alpha
        Color textColor = textComponent.color;
        textColor.a = 0f;
        textComponent.color = textColor;

        // Start the hiding process
        StartCoroutine(EnsureTextHidden());
    }

    private IEnumerator EnsureTextHidden()
    {
        // Wait a frame for TMPro to initialize
        yield return null;

        // Force mesh update and hide all characters
        yield return StartCoroutine(HideTextCompletely());
        textHidden = true;

        // Wait a bit more then start the effect
        yield return new WaitForSeconds(0.1f);
        StartTextEffect();
    }

    private IEnumerator HideTextCompletely()
    {
        // Force mesh update multiple times to ensure it's ready
        textComponent.ForceMeshUpdate();
        yield return null;
        textComponent.ForceMeshUpdate();

        TMP_TextInfo textInfo = textComponent.textInfo;

        // Hide all characters at vertex level
        for (int i = 0; i < textInfo.characterCount; i++)
        {
            SetCharacterAlpha(i, 0f);
        }
        textComponent.UpdateVertexData();

        // Also set the main text color alpha to 0 as backup
        Color textColor = textComponent.color;
        textColor.a = 0f;
        textComponent.color = textColor;
    }

    public void StartTextEffect()
    {
        if (!effectStarted && textHidden)
        {
            effectStarted = true;

            // Reset the main text color alpha to 1 so individual character alphas work
            Color textColor = textComponent.color;
            textColor.a = 1f;
            textComponent.color = textColor;

            StartCoroutine(PlayTextEffect());
        }
    }

    private IEnumerator PlayTextEffect()
    {
        // Phase 1: Reveal characters one by one (they wobble as soon as they're visible)
        StartCoroutine(WobbleText()); // Start wobbling immediately
        yield return StartCoroutine(RevealCharacters());

        // Phase 2: Continue wobbling for remaining time
        yield return new WaitForSeconds(wobbleDuration - (textComponent.textInfo.characterCount * characterRevealDelay));

        // Phase 3: Move to top center
        yield return StartCoroutine(MoveToTopCenter());
    }

    private IEnumerator RevealCharacters()
    {
        textComponent.ForceMeshUpdate();
        TMP_TextInfo textInfo = textComponent.textInfo;

        // Keep all characters hidden initially (don't reset them here)
        // The characters are already hidden from the start

        // Reveal characters one by one
        for (int i = 0; i < textInfo.characterCount; i++)
        {
            if (textInfo.characterInfo[i].isVisible)
            {
                StartCoroutine(FadeInCharacter(i));
                yield return new WaitForSeconds(characterRevealDelay);
            }
        }

        // Wait for the last character to fully fade in
        yield return new WaitForSeconds(fadeInDuration);
    }

    private IEnumerator FadeInCharacter(int characterIndex)
    {
        float elapsedTime = 0f;

        // Get character info for scale manipulation
        textComponent.ForceMeshUpdate();
        TMP_TextInfo textInfo = textComponent.textInfo;

        if (characterIndex >= textInfo.characterCount || !textInfo.characterInfo[characterIndex].isVisible)
            yield break;

        int materialIndex = textInfo.characterInfo[characterIndex].materialReferenceIndex;
        int vertexIndex = textInfo.characterInfo[characterIndex].vertexIndex;

        // Store original vertex positions for this character
        Vector3[] originalVertices = new Vector3[4];
        for (int i = 0; i < 4; i++)
        {
            originalVertices[i] = textInfo.meshInfo[materialIndex].vertices[vertexIndex + i];
        }

        // Calculate character center
        Vector3 characterCenter = (originalVertices[0] + originalVertices[2]) * 0.5f;

        while (elapsedTime < fadeInDuration)
        {
            float t = elapsedTime / fadeInDuration;

            // Dramatic entrance: fade + pop scale effect
            float alpha = Mathf.Lerp(0f, 1f, t);
            float scale = Mathf.Lerp(0.3f, popInScale, Mathf.SmoothStep(0f, 0.7f, t));
            if (t > 0.7f)
            {
                scale = Mathf.Lerp(popInScale, 1f, (t - 0.7f) / 0.3f);
            }

            // Apply scale transformation
            for (int i = 0; i < 4; i++)
            {
                Vector3 direction = originalVertices[i] - characterCenter;
                textInfo.meshInfo[materialIndex].vertices[vertexIndex + i] = characterCenter + direction * scale;
            }

            SetCharacterAlpha(characterIndex, alpha);
            textComponent.UpdateVertexData();

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Reset to original scale
        for (int i = 0; i < 4; i++)
        {
            textInfo.meshInfo[materialIndex].vertices[vertexIndex + i] = originalVertices[i];
        }
        SetCharacterAlpha(characterIndex, 1f);
        textComponent.UpdateVertexData();
    }

    private IEnumerator WobbleText()
    {
        float elapsedTime = 0f;
        textComponent.ForceMeshUpdate();
        TMP_TextInfo textInfo = textComponent.textInfo;

        // Store original vertex positions
        Vector3[][] originalVertices = new Vector3[textInfo.meshInfo.Length][];
        for (int i = 0; i < textInfo.meshInfo.Length; i++)
        {
            originalVertices[i] = new Vector3[textInfo.meshInfo[i].vertices.Length];
            textInfo.meshInfo[i].vertices.CopyTo(originalVertices[i], 0);
        }

        while (elapsedTime < wobbleDuration)
        {
            for (int i = 0; i < textInfo.characterCount; i++)
            {
                if (!textInfo.characterInfo[i].isVisible) continue;

                int matIndex = textInfo.characterInfo[i].materialReferenceIndex;
                int vertexIndex = textInfo.characterInfo[i].vertexIndex;

                float time = Time.time + i * 0.05f;
                float wobbleY = Mathf.Sin((time * wobbleSpeed) + (i * 0.3f)) * wobbleStrength;
                float wobbleX = Mathf.Cos((time * wobbleSpeed * 1.2f) + (i * 0.5f)) * wobbleStrength * 0.4f;
                float rot = Mathf.Sin((time * wobbleSpeed * 0.9f) + i) * rotationWobble * Mathf.Deg2Rad;

                Vector3 offset = new Vector3(wobbleX, wobbleY, 0);

                Vector3 center = Vector3.zero;
                for (int j = 0; j < 4; j++)
                    center += originalVertices[matIndex][vertexIndex + j];
                center /= 4f;

                for (int j = 0; j < 4; j++)
                {
                    Vector3 orig = originalVertices[matIndex][vertexIndex + j];
                    Vector3 local = orig - center;

                    float cos = Mathf.Cos(rot);
                    float sin = Mathf.Sin(rot);

                    Vector3 rotated = new Vector3(
                        local.x * cos - local.y * sin,
                        local.x * sin + local.y * cos,
                        local.z
                    );

                    textInfo.meshInfo[matIndex].vertices[vertexIndex + j] = center + rotated + offset;
                }
            }

            textComponent.UpdateVertexData();
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Restore original vertices
        for (int i = 0; i < textInfo.meshInfo.Length; i++)
        {
            originalVertices[i].CopyTo(textInfo.meshInfo[i].vertices, 0);
        }
        textComponent.UpdateVertexData();
    }


    private IEnumerator MoveToTopCenter()
    {
        Camera cam = Camera.main;
        if (cam == null) cam = FindFirstObjectByType<Camera>();

        Vector3 targetPosition;
        if (cam != null)
        {
            if (textComponent.canvas != null && textComponent.canvas.renderMode == RenderMode.ScreenSpaceOverlay)
            {
                targetPosition = new Vector3(Screen.width * 0.5f, Screen.height * 0.9f, 0);
            }
            else
            {
                Vector3 topCenter = cam.ScreenToWorldPoint(new Vector3(Screen.width * 0.5f, Screen.height * 0.9f, cam.nearClipPlane + 1f));
                targetPosition = topCenter;
            }
        }
        else
        {
            targetPosition = originalPosition + Vector3.up * 5f;
        }

        Vector3 startPosition = transform.position;
        Vector3 startScale = transform.localScale;
        Vector3 targetScale = startScale * 0.7f;
        float elapsedTime = 0f;

        while (elapsedTime < moveToTopDuration)
        {
            float t = elapsedTime / moveToTopDuration;
            float curveT = moveCurve.Evaluate(t);

            if (t > 0.8f)
            {
                float overshootT = (t - 0.8f) / 0.2f;
                Vector3 overshootOffset = Vector3.up * Mathf.Sin(overshootT * Mathf.PI) * 20f;
                transform.position = Vector3.Lerp(startPosition, targetPosition, curveT) + overshootOffset;
            }
            else
            {
                transform.position = Vector3.Lerp(startPosition, targetPosition, curveT);
            }

            transform.localScale = Vector3.Lerp(startScale, targetScale, t);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.position = targetPosition;
        transform.localScale = targetScale;

        // Wait 1 second before fading out
        yield return new WaitForSeconds(1f);

        yield return StartCoroutine(FadeOutText(0.5f));
    }

    private IEnumerator FadeOutText(float duration)
    {
        float elapsedTime = 0f;
        textComponent.ForceMeshUpdate();
        TMP_TextInfo textInfo = textComponent.textInfo;

        while (elapsedTime < duration)
        {
            float t = 1f - (elapsedTime / duration);
            for (int i = 0; i < textInfo.characterCount; i++)
            {
                if (textInfo.characterInfo[i].isVisible)
                    SetCharacterAlpha(i, t);
            }

            textComponent.UpdateVertexData();
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Fully invisible
        for (int i = 0; i < textInfo.characterCount; i++)
        {
            SetCharacterAlpha(i, 0f);
        }
        textComponent.UpdateVertexData();


        Destroy(gameObject);
    }

    private void SetCharacterAlpha(int characterIndex, float alpha)
    {
        TMP_TextInfo textInfo = textComponent.textInfo;

        if (characterIndex >= textInfo.characterCount || !textInfo.characterInfo[characterIndex].isVisible)
            return;

        int materialIndex = textInfo.characterInfo[characterIndex].materialReferenceIndex;
        int vertexIndex = textInfo.characterInfo[characterIndex].vertexIndex;

        Color32[] colors = textInfo.meshInfo[materialIndex].colors32;
        byte a = (byte)(alpha * 255);

        for (int i = 0; i < 4; i++)
        {
            colors[vertexIndex + i].a = a;
        }
    }


    // Public method to restart the effect
    public void RestartEffect()
    {
        StopAllCoroutines();
        transform.position = originalPosition;
        transform.localScale = Vector3.one;
        effectStarted = false;
        textHidden = false;

        // Reset text to original state and hide it
        textComponent.text = originalText;
        StartCoroutine(EnsureTextHidden());
    }
}