using UnityEngine;

public class InterpolatedTransform : IInterpolated<TransformData>
{
    private Transform m_transform;

    private float m_positionThreshold;
    private float m_rotationThreshold;
    private float m_scaleThreshold;

    public InterpolatedTransform(Transform transform, float positionThreshold = 0, float rotationThreshold = 0, float scaleThreshold = 0)
    {
        m_transform = transform;
        SetThresholds(positionThreshold, rotationThreshold, scaleThreshold);
    }

    public void SetThresholds(float positionThreshold, float rotationThreshold, float scaleThreshold)
    {
        m_positionThreshold = positionThreshold;
        m_rotationThreshold = rotationThreshold;
        m_scaleThreshold = scaleThreshold;
    }

    public TransformData GetInterpolatedValue(TransformData older, TransformData newer, float interpolationFactor)
    {
        return TransformData.Interpolate(older, newer, interpolationFactor);
    }

    public TransformData ReadOriginal()
    {
        return new TransformData(m_transform);
    }

    public void AffectOriginal(TransformData transformData)
    {
        TransformData.Apply(transformData, m_transform);
    }

    public bool AreDifferent(TransformData first, TransformData second)
    {
        return  Vector3.Distance(first.position, second.position) > m_positionThreshold ||
                Quaternion.Angle(first.rotation, second.rotation) > m_rotationThreshold ||
                Vector3.Distance(first.scale, second.scale) > m_scaleThreshold;
    }
}