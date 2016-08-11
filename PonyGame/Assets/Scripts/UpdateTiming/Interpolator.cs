using UnityEngine;

public class Interpolator<T>
{
    private IInterpolated<T> m_interpolated;
    private T[] m_latestValues;
    private int m_newestValueIndex;

    private bool m_firstFixedLoop = true;
    private bool m_firstUpdateLoop = true;

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
        if (!m_firstFixedLoop)
        {
            StoreCurrentValue();
            m_firstFixedLoop = false;
        }
        m_interpolated.AffectOriginal(m_latestValues[m_newestValueIndex]);

        m_firstUpdateLoop = true;
    }

    public void StoreCurrentValue()
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
        if (m_firstUpdateLoop)
        {
            StoreCurrentValue();
            m_firstUpdateLoop = false;
        }
        T newer = m_latestValues[m_newestValueIndex];
        T older = m_latestValues[GetOlderValueIndex()];
        m_interpolated.AffectOriginal(m_interpolated.GetInterpolatedValue(older, newer, InterpolationController.InterpolationFactor));

        m_firstFixedLoop = true;
    }

    private int GetOlderValueIndex()
    {
        return (m_newestValueIndex + 1) % 2;
    }
}
