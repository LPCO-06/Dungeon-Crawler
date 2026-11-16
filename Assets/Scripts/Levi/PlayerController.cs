using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 6f;
    [SerializeField] private float acceleration = 0.1f;
    [SerializeField] private float rotationSpeed = 15f;

    [Header("Gravity")]
    [SerializeField] private float gravity = -9.81f;

    [Header("Input Action Reference")]
    [SerializeField] private InputActionReference moveActionReference;

    private CharacterController controller;
    private Vector2 currentInput;
    private Vector2 smoothedInput;
    private Vector2 inputVelocity;
    private float verticalVelocity;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    private void OnEnable()
    {
        if (moveActionReference != null) moveActionReference.action.Enable();
    }

    private void OnDisable()
    {
        if (moveActionReference != null) moveActionReference.action.Disable();
    }

    /// <summary>
    /// Overrides movement speed dynamically when equipping a class.
    /// </summary>
    public void SetMoveSpeed(float newSpeed)
    {
        moveSpeed = newSpeed;
    }

    private void Update()
    {
        GatherInput();
        HandleMovement();
    }

    private void GatherInput()
    {
        currentInput = (moveActionReference != null && moveActionReference.action != null)
            ? moveActionReference.action.ReadValue<Vector2>()
            : Vector2.zero;

        smoothedInput = Vector2.SmoothDamp(smoothedInput, currentInput, ref inputVelocity, acceleration);
    }

    private void HandleMovement()
    {
        Vector3 moveDirection = new Vector3(smoothedInput.x, 0f, smoothedInput.y).normalized;
        Vector3 motion = moveDirection * (moveSpeed * smoothedInput.magnitude);

        if (controller.isGrounded)
            verticalVelocity = -2.0f;
        else
            verticalVelocity += gravity * Time.deltaTime;

        motion.y = verticalVelocity;
        controller.Move(motion * Time.deltaTime);

        if (moveDirection.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }
}