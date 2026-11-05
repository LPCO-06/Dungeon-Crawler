using UnityEngine;
using UnityEngine.InputSystem; // Required for New Input System
using Unity.Netcode;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : NetworkBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 8.5f;
    [SerializeField] private float acceleration = 50f;
    [SerializeField] private float deceleration = 40f;
    [SerializeField] private float rotationSpeed = 25f;
    [SerializeField] private float gravity = -20f;

    private CharacterController controller;
    private Vector3 currentVelocity;
    private Vector3 verticalVelocity;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    private void Update()
    {
        // Allow input if we own this object, or if testing locally without a network manager running
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening && !IsOwner)
            return;

        HandleMovement();
        HandleGravity();

        Vector3 totalMovement = (currentVelocity + verticalVelocity) * Time.deltaTime;
        controller.Move(totalMovement);
    }

    private void HandleMovement()
    {
        float inputX = 0f;
        float inputZ = 0f;

        // Read Keyboard Input (WASD / Arrows)
        if (Keyboard.current != null)
        {
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) inputX -= 1f;
            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) inputX += 1f;
            if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) inputZ -= 1f;
            if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) inputZ += 1f;
        }

        // Read Gamepad Left Stick Input (if connected)
        if (Gamepad.current != null)
        {
            Vector2 stick = Gamepad.current.leftStick.ReadValue();
            if (stick.sqrMagnitude > 0.01f)
            {
                inputX = stick.x;
                inputZ = stick.y;
            }
        }

        Vector3 targetDirection = new Vector3(inputX, 0f, inputZ).normalized;
        Vector3 targetVelocity = targetDirection * moveSpeed;

        float rate = (targetDirection.magnitude > 0.01f) ? acceleration : deceleration;
        currentVelocity = Vector3.MoveTowards(currentVelocity, targetVelocity, rate * Time.deltaTime);

        // Rotate toward movement direction if moving
        if (targetDirection.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }

    private void HandleGravity()
    {
        if (controller.isGrounded && verticalVelocity.y < 0)
        {
            verticalVelocity.y = -2f;
        }

        verticalVelocity.y += gravity * Time.deltaTime;
    }

    public void ApplyAttackLunge(Vector3 lungeVector)
    {
        currentVelocity += lungeVector;
    }
}