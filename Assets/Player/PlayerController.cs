using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    private StatsComponent statsComponent;
    private RuneComponent runes;
    private InputSystem_Actions controls;
    private Vector2 moveInput;
    public RuneStateData stateToTest;
    public AudioClip backgroundMusic;

    public Animator animator;
    public SpriteRenderer spriteRenderer;
    public string walkAnimation = "Walk";
    public string idleAnimation = "Idle";

    private void Awake()
    {
        controls = new InputSystem_Actions();
        MusicManager.Instance.PlayLoop("bg1", backgroundMusic, 1);
    }

    private void Start()
    {
        statsComponent = GetComponent<StatsComponent>();
        runes = GetComponent<RuneComponent>();
        runes.AddState(RuneFactory.Create(stateToTest));

        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer == null)
            {
                Debug.LogWarning("SpriteRenderer not assigned or found on GameObject.");
            }
        }
    }

    private void OnEnable()
    {
        controls.Player.Enable();
        controls.Player.Move.performed += OnMove;
        controls.Player.Move.canceled += OnMove;
    }

    private void OnDisable()
    {
        controls.Player.Move.performed -= OnMove;
        controls.Player.Move.canceled -= OnMove;
        controls.Player.Disable();
    }

    private void OnMove(InputAction.CallbackContext ctx)
    {
        moveInput = ctx.ReadValue<Vector2>();
    }

    private void Update()
    {
        float speed = statsComponent.currentStats.moveSpeed;
        Vector3 movement = new(moveInput.x, moveInput.y, 0f);
        transform.position += speed * Time.deltaTime * movement;

        if (animator != null)
        {
            if (movement.sqrMagnitude > 0.001f)
            {
                animator.Play(walkAnimation);
            }
            else
            {
                animator.Play(idleAnimation);
            }
        }

        // Flip sprite based on horizontal movement
        if (spriteRenderer != null && Mathf.Abs(movement.x) > 0.01f)
        {
            spriteRenderer.flipX = movement.x < 0f;
        }
    }
}
