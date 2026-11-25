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
    [Tooltip("When enabled, aims via mouse cursor or right analog stick depending on which was used last.")]
    [SerializeField] private bool twinStickAiming = true;
    [SerializeField] private float rotationSpeed = 25f;
    [SerializeField] private float stickAimDeadzone = 0.2f;

    [Header("Gravity")]
    [SerializeField] private float gravity = -9.81f;

    [Header("Input Action References")]
    [SerializeField] private InputActionReference moveActionReference;
    [Tooltip("Optional: Bind to Right Stick / Look (Vector2). If unassigned, falls back to Gamepad.current.rightStick.")]
    [SerializeField] private InputActionReference lookActionReference;

    private CharacterController controller;
    private Camera mainCamera;
    private Vector2 currentInput;
    private Vector2 smoothedInput;
    private Vector2 inputVelocity;
    private float verticalVelocity;

    private bool usingGamepadAim;
    private Vector3 lastGamepadAimDirection;

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
            if (moveActionReference != null) moveActionReference.action.Enable();
            if (lookActionReference != null) lookActionReference.action.Enable();
        }
    }

    protected override void OnDespawned()
    {
        if (isOwner)
        {
            if (moveActionReference != null) moveActionReference.action.Disable();
            if (lookActionReference != null) lookActionReference.action.Disable();
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

        if (twinStickAiming)
        {
            targetLookDirection = GetTwinStickAimDirection(moveDirection);
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

    private Vector3 GetTwinStickAimDirection(Vector3 moveDirection)
    {
        Vector2 stickInput = ReadRightStickInput();

        // 1. If the player tilts the right stick past the deadzone, switch to gamepad aiming
        if (stickInput.sqrMagnitude >= stickAimDeadzone * stickAimDeadzone)
        {
            usingGamepadAim = true;
            lastGamepadAimDirection = new Vector3(stickInput.x, 0f, stickInput.y).normalized;
            return lastGamepadAimDirection;
        }

        // 2. If the mouse physically moved this frame, switch back to mouse aiming
        if (Mouse.current != null && Mouse.current.delta.ReadValue().sqrMagnitude > 1f)
        {
            usingGamepadAim = false;
        }

        // 3. Apply active device's aim
        if (usingGamepadAim)
        {
            // Keep facing last aimed direction when stick returns to center (or face move direction if never aimed yet)
            return lastGamepadAimDirection.sqrMagnitude > 0.001f ? lastGamepadAimDirection : moveDirection;
        }

        return GetMouseAimDirection();
    }

    private Vector2 ReadRightStickInput()
    {
        // Prefer the assigned InputActionReference if wired up in the Inspector
        if (lookActionReference != null && lookActionReference.action != null)
        {
            // Ignore Mouse Delta if your Look action shares both Mouse Delta and Right Stick
            if (lookActionReference.action.activeControl?.device is Gamepad)
            {
                return lookActionReference.action.ReadValue<Vector2>();
            }
        }

        // Direct fallback to active Gamepad right stick so it works out of the box
        if (Gamepad.current != null)
        {
            return Gamepad.current.rightStick.ReadValue();
        }

        return Vector2.zero;
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

        Plane aimPlane = new Plane(Vector3.up, new Vector3(0f, transform.position.y, 0f));

        if (aimPlane.Raycast(ray, out float enterDistance))
        {
            Vector3 hitPoint = ray.GetPoint(enterDistance);
            Vector3 direction = hitPoint - transform.position;
            direction.y = 0f;

            if (direction.sqrMagnitude > 0.04f)
            {
                return direction.normalized;
            }
        }

        return Vector3.zero;
    }
}