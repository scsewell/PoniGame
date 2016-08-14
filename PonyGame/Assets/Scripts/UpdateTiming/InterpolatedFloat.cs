using UnityEngine;
using System;

public class InterpolatedFloat : IInterpolated<float>
{
    private Func<float> m_originalReader;
    private Action<float> m_originalAffector;

    private Transform m_transform;
    private float m_threshold = 0;

    public InterpolatedFloat(Func<float> originalReader, Action<float> originalAffector)
    {
        m_originalReader = originalReader;
        m_originalAffector = originalAffector;
    }

    public void SetThreshold(float threshold)
    {
        m_threshold = threshold;
    }

    public float GetInterpolatedValue(float older, float newer, float interpolationFactor)
    {
        return Mathf.Lerp(older, newer, interpolationFactor);
    }

    public float ReadOriginal()
    {
        return m_originalReader();
    }

    public void AffectOriginal(float value)
    {
        m_originalAffector(value);
    }

    public bool AreDifferent(float first, float second)
    {
        return Mathf.Abs(first - second) > m_threshold;
    }
}