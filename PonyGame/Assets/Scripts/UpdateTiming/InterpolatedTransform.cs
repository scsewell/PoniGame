using UnityEngine;

public class InterpolatedTransform : IInterpolated<TransformData>
{
    private Transform m_transform;
    private float m_positionThreshold = 0;
    private float m_rotationThreshold = 0;
    private float m_scaleThreshold = 0;

    public InterpolatedTransform(Transform transform)
    {
        m_transform = transform;
    }

    public void SetThresholds(float position, float rotation, float scale)
    {
        m_positionThreshold = position;
        m_rotationThreshold = rotation;
        m_scaleThreshold = scale;
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