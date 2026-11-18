using PurrNet;
using UnityEngine;

public class PlayerAimIndicator : NetworkBehaviour
{
    [Header("Ring Dimensions")]
    [SerializeField] private float ringRadius = 0.7f;
    [SerializeField] private float lineWidth = 0.06f;
    [SerializeField] private float groundOffset = 0.03f;
    [SerializeField] private int circleSegments = 48;

    [Header("Directional Pointer")]
    [SerializeField] private float arrowDistance = 0.9f;
    [SerializeField] private float arrowSize = 0.22f;

    [Header("Colors & Co-op Visibility")]
    [SerializeField] private Color localPlayerColor = new Color(0.2f, 0.95f, 1f, 0.85f);
    [SerializeField] private bool showTeammateRing = true;
    [SerializeField] private Color teammateColor = new Color(1f, 1f, 1f, 0.35f);

    private GameObject indicatorRoot;
    private LineRenderer ringRenderer;
    private LineRenderer arrowRenderer;

    protected override void OnSpawned()
    {
        base.OnSpawned();

        if (!isOwner && !showTeammateRing) return;

        BuildIndicator();
    }

    protected override void OnDespawned()
    {
        if (indicatorRoot != null)
        {
            Destroy(indicatorRoot);
        }
        base.OnDespawned();
    }

    private void BuildIndicator()
    {
        indicatorRoot = new GameObject("AimIndicator");
        indicatorRoot.transform.SetParent(transform, false);
        indicatorRoot.transform.localPosition = new Vector3(0f, groundOffset, 0f);
        indicatorRoot.transform.localRotation = Quaternion.identity;

        Color activeColor = isOwner ? localPlayerColor : teammateColor;
        Material lineMat = new Material(Shader.Find("Sprites/Default"));

        GameObject ringObj = new GameObject("GroundRing");
        ringObj.transform.SetParent(indicatorRoot.transform, false);
        ringRenderer = ringObj.AddComponent<LineRenderer>();
        ConfigureLineRenderer(ringRenderer, lineMat, activeColor, true);

        ringRenderer.positionCount = circleSegments;
        for (int i = 0; i < circleSegments; i++)
        {
            float angle = (i / (float)circleSegments) * Mathf.PI * 2f;
            float x = Mathf.Sin(angle) * ringRadius;
            float z = Mathf.Cos(angle) * ringRadius;
            ringRenderer.SetPosition(i, new Vector3(x, z, 0f));
        }

        if (isOwner)
        {
            GameObject arrowObj = new GameObject("AimArrow");
            arrowObj.transform.SetParent(indicatorRoot.transform, false);
            arrowRenderer = arrowObj.AddComponent<LineRenderer>();
            ConfigureLineRenderer(arrowRenderer, lineMat, activeColor, true);

            arrowRenderer.positionCount = 3;
            Vector3 tip = new Vector3(0f, arrowDistance + arrowSize, 0f);
            Vector3 leftBase = new Vector3(-arrowSize * 0.75f, arrowDistance, 0f);
            Vector3 rightBase = new Vector3(arrowSize * 0.75f, arrowDistance, 0f);

            arrowRenderer.SetPosition(0, tip);
            arrowRenderer.SetPosition(1, leftBase);
            arrowRenderer.SetPosition(2, rightBase);
        }
    }

    private void ConfigureLineRenderer(LineRenderer lr, Material mat, Color color, bool loop)
    {
        lr.useWorldSpace = false;
        lr.loop = loop;
        lr.alignment = LineAlignment.TransformZ;
        lr.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        lr.material = mat;
        lr.startColor = color;
        lr.endColor = color;
        lr.startWidth = lineWidth;
        lr.endWidth = lineWidth;
        lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        lr.receiveShadows = false;
    }
}