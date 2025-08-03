using UnityEngine;
public class FloatingUI : MonoBehaviour
{
    [SerializeField] private float amplitude = 10f;
    [SerializeField] private float frequency = 1f; 

    private RectTransform rectTransform;
    private Vector2 startPos;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        startPos = rectTransform.anchoredPosition;
    }

    void Update()
    {
        float offsetY = Mathf.Sin(Time.time * frequency) * amplitude;
        rectTransform.anchoredPosition = startPos + new Vector2(0f, offsetY);
    }
}
