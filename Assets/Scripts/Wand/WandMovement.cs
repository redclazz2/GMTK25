using UnityEngine;

public class WandMovement : MonoBehaviour
{
    [Header("Orbit Settings")]
    public float orbitRadius = 2f;
    public float orbitSpeed = 90f; // degrees per second

    [Header("Optional Settings")]
    public bool clockwise = true;
    public Vector3 orbitAxis = Vector3.forward; // axis to orbit around (for 3D)

    private Transform parentTransform;
    private float currentAngle = 0f;

    void Start()
    {
        // Get parent transform
        parentTransform = transform.parent;

        if (parentTransform == null)
        {
            Debug.LogWarning("WandMovement: No parent found! This script requires a parent object.");
            enabled = false;
            return;
        }

        // Pick random starting angle
        currentAngle = Random.Range(0f, 360f);

        // Set initial position at orbit radius
        Vector3 initialPosition = CalculateOrbitPosition();
        transform.localPosition = initialPosition;
    }

    void Update()
    {
        if (parentTransform == null) return;

        // Rotate around parent
        float rotationDirection = clockwise ? -1f : 1f;
        currentAngle += orbitSpeed * rotationDirection * Time.deltaTime;

        // Keep angle in 0-360 range
        if (currentAngle >= 360f) currentAngle -= 360f;
        if (currentAngle < 0f) currentAngle += 360f;

        // Calculate new position
        Vector3 newLocalPosition = CalculateOrbitPosition();
        transform.localPosition = newLocalPosition;
    }

    Vector3 CalculateOrbitPosition()
    {
        // Convert angle to radians
        float angleInRadians = currentAngle * Mathf.Deg2Rad;

        // For 2D orbiting (in XY plane)
        if (orbitAxis == Vector3.forward || orbitAxis == Vector3.back)
        {
            float x = Mathf.Cos(angleInRadians) * orbitRadius;
            float y = Mathf.Sin(angleInRadians) * orbitRadius;
            return new Vector3(x, y, 0f);
        }
        // For 3D orbiting around Y axis
        else if (orbitAxis == Vector3.up || orbitAxis == Vector3.down)
        {
            float x = Mathf.Cos(angleInRadians) * orbitRadius;
            float z = Mathf.Sin(angleInRadians) * orbitRadius;
            return new Vector3(x, 0f, z);
        }
        // For 3D orbiting around X axis
        else if (orbitAxis == Vector3.right || orbitAxis == Vector3.left)
        {
            float y = Mathf.Cos(angleInRadians) * orbitRadius;
            float z = Mathf.Sin(angleInRadians) * orbitRadius;
            return new Vector3(0f, y, z);
        }

        // Default to XY plane
        float defaultX = Mathf.Cos(angleInRadians) * orbitRadius;
        float defaultY = Mathf.Sin(angleInRadians) * orbitRadius;
        return new Vector3(defaultX, defaultY, 0f);
    }

    // Public methods to control the wand
    public void SetOrbitRadius(float newRadius)
    {
        orbitRadius = newRadius;
        // Immediately update position with new radius
        Vector3 newPosition = CalculateOrbitPosition();
        transform.localPosition = newPosition;
    }

    public void SetOrbitSpeed(float newSpeed)
    {
        orbitSpeed = newSpeed;
    }

    public void SetClockwise(bool isClockwise)
    {
        clockwise = isClockwise;
    }

    public void ResetToNewRadius(float newRadius)
    {
        orbitRadius = newRadius;
        currentAngle = Random.Range(0f, 360f);
        Vector3 newPosition = CalculateOrbitPosition();
        transform.localPosition = newPosition;
    }

    // Get current orbit info
    public float GetCurrentAngle()
    {
        return currentAngle;
    }

    public float GetCurrentRadius()
    {
        return orbitRadius;
    }
}