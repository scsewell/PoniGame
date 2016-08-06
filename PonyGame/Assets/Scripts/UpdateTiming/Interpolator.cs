using UnityEngine;

public class Interpolator<T> : IInterpolator
{
    private IInterpolated<T> m_interpolated;
    private T[] m_latestValues;
    private int m_newestValueIndex;

    public Interpolator(IInterpolated<T> interpolated)
    {
        m_interpolated = interpolated;
    }

    public void Start()
    {
        m_latestValues = new T[2];

        ForgetPreviousValues();
    }

    public void FixedUpdate()
    {
        m_interpolated.AffectOriginal(m_latestValues[m_newestValueIndex]);
    }

    public void LateFixedUpdate()
    {
        m_newestValueIndex = GetOlderValueIndex();
        m_latestValues[m_newestValueIndex] = m_interpolated.ReadOriginal();
    }

    public void ForgetPreviousValues()
    {
        T t = m_interpolated.ReadOriginal();
        m_latestValues[0] = t;
        m_latestValues[1] = t;
    }

    public void Update()
    {
        T newer = m_latestValues[m_newestValueIndex];
        T older = m_latestValues[GetOlderValueIndex()];
        m_interpolated.AffectOriginal(m_interpolated.GetInterpolatedValue(older, newer, InterpolationController.InterpolationFactor));
    }

    private int GetOlderValueIndex()
    {
        return (m_newestValueIndex + 1) % 2;
    }
}
