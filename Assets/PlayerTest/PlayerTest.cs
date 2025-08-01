using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerTest : MonoBehaviour
{
    private StatsComponent statsComponent;
    private RuneComponent runes;
    private InputSystem_Actions controls;
    private Vector2 moveInput;
    public RuneStateData stateToTest;

    private void Awake()
    {
        controls = new InputSystem_Actions();
    }

    private void Start()
    {
        statsComponent = GetComponent<StatsComponent>();
        runes = GetComponent<RuneComponent>();
        runes.AddState(RuneFactory.Create(stateToTest));
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
        // Usamos directamente la estadística actual
        float speed = statsComponent.currentStats.moveSpeed;
        Vector3 movement = new Vector3(moveInput.x, moveInput.y, 0f);
        transform.position += movement * speed * Time.deltaTime;
    }
}