using PurrNet;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : NetworkBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 6f;
    [SerializeField] private float acceleration = 0.1f;

    [Header("Aiming Settings")]
    [Tooltip("When enabled, the player rotates toward the mouse cursor on the ground plane instead of movement direction.")]
    [SerializeField] private bool aimTowardMouse = true;
    [SerializeField] private float rotationSpeed = 25f;

    [Header("Gravity")]
    [SerializeField] private float gravity = -9.81f;

    [Header("Input Action Reference")]
    [SerializeField] private InputActionReference moveActionReference;

    private CharacterController controller;
    private Camera mainCamera;
    private Vector2 currentInput;
    private Vector2 smoothedInput;
    private Vector2 inputVelocity;
    private float verticalVelocity;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    protected override void OnSpawned()
    {
        base.OnSpawned();

        if (isOwner)
        {
            mainCamera = Camera.main;
            if (moveActionReference != null)
            {
                moveActionReference.action.Enable();
            }
        }
    }

    protected override void OnDespawned()
    {
        if (isOwner && moveActionReference != null)
        {
            moveActionReference.action.Disable();
        }
        base.OnDespawned();
    }

    public void SetMoveSpeed(float newSpeed)
    {
        moveSpeed = newSpeed;
    }

    private void Update()
    {
        if (!isOwner) return;

        GatherInput();
        Vector3 moveDirection = HandleMovement();
        HandleRotation(moveDirection);
    }

    private void GatherInput()
    {
        currentInput = (moveActionReference != null && moveActionReference.action != null)
            ? moveActionReference.action.ReadValue<Vector2>()
            : Vector2.zero;

        smoothedInput = Vector2.SmoothDamp(smoothedInput, currentInput, ref inputVelocity, acceleration);
    }

    private Vector3 HandleMovement()
    {
        Vector3 moveDirection = new Vector3(smoothedInput.x, 0f, smoothedInput.y).normalized;
        Vector3 motion = moveDirection * (moveSpeed * smoothedInput.magnitude);

        if (controller.isGrounded)
            verticalVelocity = -2.0f;
        else
            verticalVelocity += gravity * Time.deltaTime;

        motion.y = verticalVelocity;
        controller.Move(motion * Time.deltaTime);

        return moveDirection;
    }

    private void HandleRotation(Vector3 moveDirection)
    {
        Vector3 targetLookDirection = Vector3.zero;

        if (aimTowardMouse)
        {
            targetLookDirection = GetMouseAimDirection();
        }
        else if (moveDirection.sqrMagnitude > 0.001f)
        {
            targetLookDirection = moveDirection;
        }

        if (targetLookDirection.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(targetLookDirection, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }

    private Vector3 GetMouseAimDirection()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
            if (mainCamera == null) return Vector3.zero;
        }

        if (Mouse.current == null) return Vector3.zero;

        Vector2 mouseScreenPos = Mouse.current.position.ReadValue();
        Ray ray = mainCamera.ScreenPointToRay(mouseScreenPos);

        // Intersect a horizontal plane at the player's current Y height (works even over pits/edges)
        Plane aimPlane = new Plane(Vector3.up, new Vector3(0f, transform.position.y, 0f));

        if (aimPlane.Raycast(ray, out float enterDistance))
        {
            Vector3 hitPoint = ray.GetPoint(enterDistance);
            Vector3 direction = hitPoint - transform.position;
            direction.y = 0f;

            // Small deadzone prevents jitter if the cursor sits directly on the player's center
            if (direction.sqrMagnitude > 0.04f)
            {
                return direction.normalized;
            }
        }

        return Vector3.zero;
    }
}