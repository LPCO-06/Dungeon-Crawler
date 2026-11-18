using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class BillboardSprite : MonoBehaviour
{
    public enum BillboardMode
    {
        FullCameraAngle, // Tilts back to match the camera's pitch (prevents perspective foreshortening)
        YAxisOnly        // Stands strictly vertical, only rotating around the Y axis
    }

    [Header("Billboard Orientation")]
    [SerializeField] private BillboardMode mode = BillboardMode.FullCameraAngle;

    [Header("Horizontal Flip")]
    [Tooltip("Automatically flips the sprite left/right based on the parent root's facing direction.")]
    [SerializeField] private bool flipBasedOnParentFacing = true;
    [Tooltip("Check this if your default sprite art faces Left instead of Right.")]
    [SerializeField] private bool spriteFacesLeftByDefault = false;

    private SpriteRenderer spriteRenderer;
    private Transform mainCamTransform;
    private Transform rootTransform;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        rootTransform = transform.parent != null ? transform.parent : transform;
    }

    private void Start()
    {
        if (Camera.main != null)
        {
            mainCamTransform = Camera.main.transform;
        }
    }

    private void LateUpdate()
    {
        if (mainCamTransform == null)
        {
            if (Camera.main != null) mainCamTransform = Camera.main.transform;
            else return;
        }

        // 1. Keep the sprite facing the camera without inheriting the root's 360-degree Y spin
        if (mode == BillboardMode.FullCameraAngle)
        {
            transform.rotation = mainCamTransform.rotation;
        }
        else
        {
            Vector3 lookDir = transform.position - mainCamTransform.position;
            lookDir.y = 0f;
            if (lookDir.sqrMagnitude > 0.001f)
            {
                transform.rotation = Quaternion.LookRotation(lookDir);
            }
        }

        // 2. Flip left/right smoothly based on which horizontal direction the parent root is facing
        if (flipBasedOnParentFacing && rootTransform != null)
        {
            float horizontalFacing = Vector3.Dot(rootTransform.forward, mainCamTransform.right);

            // Deadzone prevents rapid flickering when moving straight up/down along the Z-axis
            if (Mathf.Abs(horizontalFacing) > 0.05f)
            {
                bool facingLeft = horizontalFacing < 0f;
                spriteRenderer.flipX = spriteFacesLeftByDefault ? !facingLeft : facingLeft;
            }
        }
    }
}