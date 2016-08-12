using UnityEngine;

public class InterpolatedTransform : IInterpolated<TransformData>
{
    private Transform m_transform;

    public InterpolatedTransform(Transform transform)
    {
        m_transform = transform;
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
}