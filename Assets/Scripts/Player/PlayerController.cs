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

    [Header("Combat Settings")]
    public float attackRange = 1.5f;
    public int attackDamage = 10;
    public float attackDuration = 0.5f;
    public float attackLag = 0.2f;
    public LayerMask enemyLayers;
    public Transform attackPoint;

    [Tooltip("Насколько быстро тело игрока поворачивается за камерой")]
    public float rotationSmoothness = 15f;

    [Header("Input System")]
    public InputActionAsset inputAsset;

    [Header("Visuals")]
    public Animator animator;

    // Actions
    public InputAction MoveAction { get; private set; }
    public InputAction JumpAction { get; private set; }
    public InputAction SprintAction { get; private set; }
    public InputAction AttackAction { get; private set; }

    // Components
    public CharacterController Controller { get; private set; }
    public Transform CameraTransform { get; private set; }

    // State Machine
    private PlayerBaseState _currentState;
    public PlayerBaseState GroundedState;
    public PlayerBaseState AirState;
    public PlayerBaseState AttackState;

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
        AttackAction = playerMap.FindAction("Attack");

        GroundedState = new GroundedState(this);
        AirState = new AirState(this);
        AttackState = new AttackState(this);
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

    public void UpdateAnimationState()
    {
        if (animator == null) return;

        Vector3 horizontalVelocity = new Vector3(Controller.velocity.x, 0, Controller.velocity.z);
        float speed = horizontalVelocity.magnitude;

        // Плавная интерполяция (Damp) значения Speed к реальной скорости
        animator.SetFloat("Speed", speed, 0.1f, Time.deltaTime);
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

        if (attackPoint == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }

}

