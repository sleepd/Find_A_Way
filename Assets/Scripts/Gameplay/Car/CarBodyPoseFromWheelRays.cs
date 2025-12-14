using UnityEngine;

/// <summary>
/// Samples ground under each wheel and aligns the car root to the fitted plane.
/// Intended for Timeline/Sequence usage (not tied to physics or suspension).
/// </summary>
public class CarBodyPoseFromWheelRays : MonoBehaviour
{
    [Header("References")]
    public Transform carRoot;
    [Tooltip("Visual/model transform to align. If null, carRoot will be aligned.")]
    public Transform bodyTransform;
    [Tooltip("Expected order: FrontLeft, FrontRight, RearLeft, RearRight.")]
    public Transform[] wheels;

    [Header("Sampling")]
    public float rayLength = 2f;
    public float wheelRadius = 0.35f;
    public LayerMask groundMask = ~0;
    [Tooltip("Cache ray origins in carRoot space so wheel rotation does not move the emission point.")]
    public bool cacheRayOrigins = true;
    [Tooltip("Vertical offset from wheel center plane to carRoot (along fitted up). Use negative if your root is at ground contact height.")]
    public float rootHeightOffsetFromWheelCenter = 0f;
    [Tooltip("Extra height adjust on top of root offset (along fitted up). Use this if the car still floats or clips.")]
    public float heightOffset = 0f;

    [Header("Smoothing")]
    public float positionLerpSpeed = 12f;
    public float rotationLerpSpeed = 12f;

    [Header("Debug")]
    public bool drawGizmos = false;

    private Vector3[] wheelCentersCache;
    private Vector3[] contactPointsCache;
    private Vector3[] normalsCache;
    private Vector3[] originOffsets;

    void LateUpdate()
    {
        if (carRoot == null || wheels == null || wheels.Length < 4)
        {
            return;
        }

        EnsureCaches();

        var centroid = Vector3.zero;
        int validCount = 0;

        for (int i = 0; i < wheels.Length; i++)
        {
            var wheel = wheels[i];
            if (wheel == null)
            {
                continue;
            }

            Vector3 baseOrigin = cacheRayOrigins && originOffsets != null
                ? carRoot.TransformPoint(originOffsets[i])
                : wheel.position;

            Vector3 origin = baseOrigin + Vector3.up * (rayLength * 0.5f);
            Vector3 direction = Vector3.down;

            RaycastHit hit;
            Vector3 contactPoint;
            Vector3 contactNormal;

            if (Physics.Raycast(origin, direction, out hit, rayLength, groundMask, QueryTriggerInteraction.Ignore))
            {
                contactPoint = hit.point;
                contactNormal = hit.normal;
            }
            else
            {
                contactPoint = wheel.position - Vector3.up * (rayLength * 0.5f);
                contactNormal = Vector3.up;
            }

            Vector3 wheelCenter = contactPoint + contactNormal.normalized * wheelRadius;

            contactPointsCache[i] = contactPoint;
            normalsCache[i] = contactNormal;
            wheelCentersCache[i] = wheelCenter;

            centroid += wheelCenter;
            validCount++;
        }

        if (validCount < 4)
        {
            return;
        }

        centroid /= validCount;

        // Diagonal cross method (stable, order-dependent).
        Vector3 diag1 = wheelCentersCache[1] - wheelCentersCache[2]; // FR - RL
        Vector3 diag2 = wheelCentersCache[0] - wheelCentersCache[3]; // FL - RR
        Vector3 up = Vector3.Cross(diag1, diag2);
        if (up.sqrMagnitude < 1e-6f)
        {
            return;
        }
        up.Normalize();

        Vector3 projectedForward = Vector3.ProjectOnPlane(carRoot.forward, up);
        if (projectedForward.sqrMagnitude < 1e-6f)
        {
            projectedForward = Vector3.Cross(up, carRoot.right);
        }

        Quaternion targetRotation = Quaternion.LookRotation(projectedForward, up);
        Vector3 targetPosition = centroid + up * (rootHeightOffsetFromWheelCenter + heightOffset);

        float positionLerpFactor = positionLerpSpeed > 0f ? 1f - Mathf.Exp(-positionLerpSpeed * Time.deltaTime) : 1f;
        float rotationLerpFactor = rotationLerpSpeed > 0f ? 1f - Mathf.Exp(-rotationLerpSpeed * Time.deltaTime) : 1f;

        var target = bodyTransform != null ? bodyTransform : carRoot;

        target.position = Vector3.Lerp(target.position, targetPosition, positionLerpFactor);
        target.rotation = Quaternion.Slerp(target.rotation, targetRotation, rotationLerpFactor);
    }

    private void EnsureCaches()
    {
        int count = wheels.Length;

        if (wheelCentersCache == null || wheelCentersCache.Length != count)
        {
            wheelCentersCache = new Vector3[count];
        }

        if (contactPointsCache == null || contactPointsCache.Length != count)
        {
            contactPointsCache = new Vector3[count];
        }

        if (normalsCache == null || normalsCache.Length != count)
        {
            normalsCache = new Vector3[count];
        }

        if (originOffsets == null || originOffsets.Length != count)
        {
            originOffsets = new Vector3[count];
            CacheOrigins();
        }
    }

    void OnEnable()
    {
        CacheOrigins();
    }

    void OnValidate()
    {
        CacheOrigins();
    }

    private void CacheOrigins()
    {
        if (!cacheRayOrigins || carRoot == null || wheels == null)
        {
            return;
        }

        if (originOffsets == null || originOffsets.Length != wheels.Length)
        {
            originOffsets = new Vector3[wheels.Length];
        }

        for (int i = 0; i < wheels.Length; i++)
        {
            if (wheels[i] == null)
            {
                continue;
            }

            originOffsets[i] = carRoot.InverseTransformPoint(wheels[i].position);
        }
    }

    void OnDrawGizmosSelected()
    {
        if (!drawGizmos || wheels == null || wheelCentersCache == null)
        {
            return;
        }

        if (wheelCentersCache.Length == 0)
        {
            return;
        }

        Gizmos.color = Color.cyan;
        for (int i = 0; i < wheels.Length; i++)
        {
            if (wheels[i] == null)
            {
                continue;
            }

            Gizmos.DrawLine(wheels[i].position, contactPointsCache[i]);
            Gizmos.DrawSphere(contactPointsCache[i], 0.02f);

            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(contactPointsCache[i], contactPointsCache[i] + normalsCache[i] * 0.2f);
            Gizmos.color = Color.cyan;

            Gizmos.DrawSphere(wheelCentersCache[i], 0.02f);
        }

        Gizmos.color = Color.green;
        Gizmos.DrawLine(wheelCentersCache[0], wheelCentersCache[wheelCentersCache.Length - 1]);
        for (int i = 0; i < wheelCentersCache.Length - 1; i++)
        {
            Gizmos.DrawLine(wheelCentersCache[i], wheelCentersCache[i + 1]);
        }
    }
}
