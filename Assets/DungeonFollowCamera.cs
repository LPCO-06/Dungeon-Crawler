using PurrNet;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CultStyleCamera : MonoBehaviour
{
    [Header("Cult of the Lamb Framing")]
    [Tooltip("Telephoto offset: pulled back and zoomed in with a narrow FOV for a 2.5D diorama feel.")]
    [SerializeField] private Vector3 offset = new Vector3(0f, 11.5f, -15.5f);
    [Tooltip("35 degrees lets 2D billboard sprites stand tall while keeping floor combat readable.")]
    [SerializeField] private float pitchAngle = 35f;
    [Tooltip("Narrow FOV (28-32) removes wide-angle edge distortion so 2D sprites sit naturally in 3D.")]
    [SerializeField] private float fieldOfView = 30f;

    [Header("Smooth Follow & Look-Ahead")]
    [Tooltip("Lower values = tighter lock, higher values = floatier follow.")]
    [SerializeField] private float followSmoothTime = 0.14f;
    [Tooltip("Subtly shifts the camera ahead of where the player is moving.")]
    [SerializeField] private float lookAheadDistance = 1.5f;
    [SerializeField] private float lookAheadSmoothTime = 0.25f;

    private Camera cam;
    private Transform localPlayerTarget;
    private Vector3 lastPlayerPos;
    private Vector3 currentVelocity;
    private Vector3 currentLookAhead;
    private Vector3 lookAheadVelocity;

    private void Awake()
    {
        cam = GetComponent<Camera>();
        ApplyLensSettings();
    }

    private void OnValidate()
    {
        // Live-updates FOV and pitch angle in the Scene/Game view while tweaking in the Inspector
        if (cam == null) cam = GetComponent<Camera>();
        if (cam != null) cam.fieldOfView = fieldOfView;
        transform.rotation = Quaternion.Euler(pitchAngle, 0f, 0f);
    }

    private void ApplyLensSettings()
    {
        cam.orthographic = false;
        cam.fieldOfView = fieldOfView;
        transform.rotation = Quaternion.Euler(pitchAngle, 0f, 0f);
    }

    private void LateUpdate()
    {
        if (localPlayerTarget == null)
        {
            FindLocalPlayer();
            return;
        }

        // Calculate player movement delta for subtle look-ahead
        Vector3 frameDelta = (localPlayerTarget.position - lastPlayerPos) / Mathf.Max(Time.deltaTime, 0.0001f);
        lastPlayerPos = localPlayerTarget.position;

        Vector3 targetLookAhead = Vector3.ClampMagnitude(new Vector3(frameDelta.x, 0f, frameDelta.z) * 0.25f, lookAheadDistance);
        currentLookAhead = Vector3.SmoothDamp(currentLookAhead, targetLookAhead, ref lookAheadVelocity, lookAheadSmoothTime);

        Vector3 desiredPosition = localPlayerTarget.position + offset + currentLookAhead;
        transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref currentVelocity, followSmoothTime);
        transform.rotation = Quaternion.Euler(pitchAngle, 0f, 0f);
    }

    private void FindLocalPlayer()
    {
        PlayerController[] players = FindObjectsByType<PlayerController>(FindObjectsSortMode.None);
        foreach (PlayerController player in players)
        {
            if (player.isOwner)
            {
                localPlayerTarget = player.transform;
                lastPlayerPos = localPlayerTarget.position;
                transform.position = localPlayerTarget.position + offset;
                break;
            }
        }
    }
}