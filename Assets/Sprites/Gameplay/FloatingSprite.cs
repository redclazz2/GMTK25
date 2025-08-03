using UnityEngine;

public class FloatingSprite : MonoBehaviour
{
    [SerializeField] private float amplitude = 0.1f; // Small value for world units
    [SerializeField] private float frequency = 1f;

    private Vector3 startPos;

    void Start()
    {
        startPos = transform.position;
    }

    void Update()
    {
        float offsetY = Mathf.Sin(Time.time * frequency) * amplitude;
        transform.position = startPos + new Vector3(0f, offsetY, 0f);
    }
}
