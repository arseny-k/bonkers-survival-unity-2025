using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Player Settings")]
    public float walkSpeed = 6f;
    public float sprintSpeed = 12f;
    public float jumpHeight = 1.5f;
    public float gravityValue = -20f;

    [Tooltip("Насколько быстро тело игрока поворачивается за камерой")]
    public float rotationSmoothness = 15f;

    [Header("Input System")]
    public InputActionAsset inputAsset;

    // Actions
    public InputAction MoveAction { get; private set; }
    public InputAction JumpAction { get; private set; }
    public InputAction SprintAction { get; private set; }

    // Components
    public CharacterController Controller { get; private set; }
    public Transform CameraTransform { get; private set; }

    // State Machine
    private PlayerBaseState _currentState;
    public PlayerBaseState GroundedState;
    public PlayerBaseState AirState;

    [Header("Debug Info")]
    public Vector3 VerticalVelocity;

    private void Awake()
    {
        Controller = GetComponent<CharacterController>();
        CameraTransform = Camera.main.transform;

        var playerMap = inputAsset.FindActionMap("Player");
        MoveAction = playerMap.FindAction("Move");
        JumpAction = playerMap.FindAction("Jump");
        SprintAction = playerMap.FindAction("Sprint");

        GroundedState = new GroundedState(this);
        AirState = new AirState(this);
    }

    private void OnEnable() => inputAsset.FindActionMap("Player").Enable();
    private void OnDisable() => inputAsset.FindActionMap("Player").Disable();

    private void Start() => ChangeState(GroundedState);
    private void Update() => _currentState.Update();

    public void ChangeState(PlayerBaseState newState)
    {
        _currentState?.Exit();
        _currentState = newState;
        _currentState.Enter();
    }

    public Vector3 GetMovement()
    {
        RotateBodyToCamera();

        Vector2 input = MoveAction.ReadValue<Vector2>();
        if (input == Vector2.zero) return Vector3.zero;

        // transform.forward всегда совпадает с направлением взгляда камеры (по горизонту)
        // Поэтому W - это transform.forward, D - transform.right
        Vector3 moveDir = (transform.forward * input.y + transform.right * input.x).normalized;

        float speed = SprintAction.IsPressed() ? sprintSpeed : walkSpeed;
        return speed * Time.deltaTime * moveDir;
    }

    private void RotateBodyToCamera()
    {
        Vector3 camFwd = CameraTransform.forward;
        camFwd.y = 0; // Игнорируем наклон вверх/вниз
        camFwd.Normalize();

        if (camFwd != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(camFwd);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                rotationSmoothness * Time.deltaTime
            );
        }
    }

    private void OnDrawGizmos()
    {

        if (Controller == null) return;

        Vector3 startPos = transform.position + Vector3.up;

        if (Controller.velocity.magnitude > 0.1f)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawRay(startPos, Controller.velocity);

            Gizmos.DrawSphere(startPos + Controller.velocity, 0.1f);
        }

        Gizmos.color = Color.blue;
        Gizmos.DrawRay(startPos, transform.forward * 1.5f);
    }

}

