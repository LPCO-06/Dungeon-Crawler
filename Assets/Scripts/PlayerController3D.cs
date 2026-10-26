using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController3D : MonoBehaviour
{
    [Header("Class Configuration")]
    public PlayerClassData classData;

    [HideInInspector] public float currentSpeed;
    [HideInInspector] public float currentHealth;

    private CharacterController controller;
    private Vector3 currentVelocity;
    private Vector3 smoothVelocity;

    void Start()
    {
        controller = GetComponent<CharacterController>();

        if (classData != null)
        {
            currentSpeed = classData.moveSpeed;
            currentHealth = classData.maxHealth;
        }
        else
        {
            currentSpeed = 10f;
            currentHealth = 20f;
        }
    }

    void Update()
    {
        if (Keyboard.current == null) return;

        float moveX = 0f;
        float moveZ = 0f;

        if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) moveZ = 1f;
        if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) moveZ = -1f;
        if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) moveX = -1f;
        if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) moveX = 1f;

        Vector3 targetDirection = new Vector3(moveX, 0, moveZ).normalized;

        if (targetDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 15f * Time.deltaTime);
        }

        Vector3 targetVelocity = targetDirection * currentSpeed;
        currentVelocity = Vector3.SmoothDamp(currentVelocity, targetVelocity, ref smoothVelocity, 0.05f);

        controller.Move(currentVelocity * Time.deltaTime);
    }
}