using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    [Header("Target")]
    public Transform playerTarget;
    public Vector3 offset = new Vector3(0, 2, -5);

    [Header("Settings")]
    public float sensitivity = 15f;
    public float minPitch = -30f;
    public float maxPitch = 60f;

    [Header("Input")]
    public InputActionAsset inputAsset;

    private InputAction _lookAction;
    private float _yaw;   // лево-право
    private float _pitch; // вверх-вниз

    private void Awake()
    {
        var map = inputAsset.FindActionMap("Player");
        _lookAction = map.FindAction("Look");
    }

    private void OnEnable() => _lookAction.Enable();
    private void OnDisable() => _lookAction.Disable();

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void LateUpdate()
    {
        if (playerTarget == null) return;

        Vector2 delta = _lookAction.ReadValue<Vector2>();

        _yaw += delta.x * sensitivity * Time.deltaTime;
        _pitch -= delta.y * sensitivity * Time.deltaTime; // Минус чтобы не было инверсии

        _pitch = Mathf.Clamp(_pitch, minPitch, maxPitch);

        Quaternion rotation = Quaternion.Euler(_pitch, _yaw, 0);

        transform.SetPositionAndRotation(playerTarget.position + rotation * offset, rotation);
    }
}