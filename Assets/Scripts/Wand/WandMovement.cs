using UnityEngine;

public class WandMovement2D : MonoBehaviour
{
    [Header("Orbit Settings")]
    public float orbitRadius = 2f;
    public float orbitSpeed = 90f;
    public bool clockwise = true;

    private Transform parentTransform;
    private float currentAngle;

    void Start()
    {
        parentTransform = transform.parent;
        if (parentTransform == null)
        {
            enabled = false;
            return;
        }

        currentAngle = Random.Range(0f, 360f);
        UpdatePositionAndRotation();
    }

    void Update()
    {
        float dirSign = clockwise ? -1f : 1f;
        currentAngle += orbitSpeed * dirSign * Time.deltaTime;
        currentAngle %= 360f;
        UpdatePositionAndRotation();
    }

    private void UpdatePositionAndRotation()
    {
        // 1) Position
        float rad = currentAngle * Mathf.Deg2Rad;
        float x = Mathf.Cos(rad) * orbitRadius;
        float y = Mathf.Sin(rad) * orbitRadius;
        transform.localPosition = new Vector3(x, y, 0f);

        // 2) Rotation (point away from center)
        //    dirVec points from parent to wand in local space
        Vector2 dirVec = new Vector2(x, y).normalized;
        float angle = Mathf.Atan2(dirVec.y, dirVec.x) * Mathf.Rad2Deg;
        // If your wand sprite by default points 'up' (along +Y), subtract 90°
        transform.localRotation = Quaternion.Euler(0f, 0f, angle - 90f);
    }
}