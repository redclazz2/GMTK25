using UnityEngine;

public class RandomJumpMover : MonoBehaviour
{
    public float moveSpeed = 3f;
    public float jumpHeight = 0.15f;
    public float jumpFrequency = 8f;
    public float tiltAmount = 10f;
    public float directionChangeInterval = 1.2f;
    public AudioClip sheepSound;

    private Vector2 moveDirection;
    private readonly float lifeTime = 4.5f;
    private float timer;
    private float directionTimer;
    private float nextSoundTime;
    private SpriteRenderer spriteRenderer;
    private new AudioSource audio;
    private Vector3 startPosition;
    private Quaternion originalRotation;

    private void Start()
    {
        moveDirection = GetRandomDiagonalDirection();
        timer = lifeTime;
        directionTimer = directionChangeInterval;
        spriteRenderer = GetComponent<SpriteRenderer>();
        audio = GetComponent<AudioSource>();
        startPosition = transform.position;
        originalRotation = transform.rotation;

        UpdateSpriteFlip();

        // First sound happens quickly
        ScheduleNextSound(initial: true);
    }

    private void Update()
    {
        transform.position += (Vector3)(moveSpeed * Time.deltaTime * moveDirection);

        float wave = Mathf.Sin(Time.time * jumpFrequency);
        float yOffset = Mathf.Abs(wave) * jumpHeight;
        float tilt = wave * tiltAmount;

        transform.position = new Vector3(transform.position.x, startPosition.y + yOffset, transform.position.z);
        transform.rotation = Quaternion.Euler(0f, 0f, tilt);

        directionTimer -= Time.deltaTime;
        if (directionTimer <= 0f)
        {
            directionTimer = directionChangeInterval;
            moveDirection = GetRandomDiagonalDirection();
            UpdateSpriteFlip();
        }

        if (audio != null && sheepSound != null && timer <= lifeTime && timer <= nextSoundTime)
        {
            audio.PlayOneShot(sheepSound, 0.6f);
            ScheduleNextSound(initial: false);
        }

        // Fade out sprite starting at 3.5 seconds remaining
        if (spriteRenderer != null)
        {
            float fadeStartTime = 1f; // fade starts when timer < 1 second (4.5 - 3.5)
            float alpha = timer < fadeStartTime ? Mathf.Clamp01(timer / fadeStartTime) : 1f;
            Color color = spriteRenderer.color;
            color.a = alpha;
            spriteRenderer.color = color;
        }

        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            Destroy(gameObject);
        }
    }

    private void UpdateSpriteFlip()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.flipX = moveDirection.x > 0f;
        }
    }

    private void ScheduleNextSound(bool initial)
    {
        float delay = initial ? Random.Range(0.05f, 0.2f) : Random.Range(0.6f, 1.2f);
        nextSoundTime = timer - delay;
    }

    private Vector2 GetRandomDiagonalDirection()
    {
        Vector2[] diagonalDirections = new Vector2[]
        {
            (Vector2.up + Vector2.right).normalized,
            (Vector2.up + Vector2.left).normalized,
            (Vector2.down + Vector2.right).normalized,
            (Vector2.down + Vector2.left).normalized
        };

        return diagonalDirections[Random.Range(0, diagonalDirections.Length)];
    }
}
