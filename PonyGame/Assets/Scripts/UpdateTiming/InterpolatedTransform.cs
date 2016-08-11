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
        return new TransformData(
            Vector3.Lerp(older.position, newer.position, interpolationFactor),
            Quaternion.Slerp(older.rotation, newer.rotation, interpolationFactor),
            Vector3.Lerp(older.scale, newer.scale, interpolationFactor)
        );
    }

    public TransformData ReadOriginal()
    {
        return new TransformData(m_transform.localPosition, m_transform.localRotation, m_transform.localScale);
    }

    public void AffectOriginal(TransformData transformData)
    {
        m_transform.localPosition = transformData.position;
        m_transform.localRotation = transformData.rotation;
        m_transform.localScale = transformData.scale;
    }
}