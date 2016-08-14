using UnityEngine;

public class Interpolator<T>
{
    private IInterpolated<T> m_interpolated;

    private T[] m_latestValues;
    private int m_newestValueIndex;

    private bool m_firstFixedLoop = true;
    private bool m_firstUpdateLoop = true;
    private bool m_interpolate = true;
    private T m_lastVisual;

    private bool m_useThreshold = false;
    public bool UseThreshold
    {
        set { m_useThreshold = value; }
    }

    public Interpolator(IInterpolated<T> interpolated, bool useThreshold = false)
    {
        m_interpolated = interpolated;
        m_useThreshold = useThreshold;
    }

    public void Start()
    {
        m_latestValues = new T[2];

        ForgetPreviousValues();
    }

    public void FixedUpdate()
    {
        if (m_firstFixedLoop)
        {
            if (m_useThreshold)
            {
                m_lastVisual = m_interpolated.ReadOriginal();
            }
            if (m_interpolate)
            {
                m_interpolated.AffectOriginal(m_latestValues[m_newestValueIndex]);
            }
            m_firstFixedLoop = false;
        }
        else
        {
            StoreCurrentValue();
        }

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
            m_interpolate = !m_useThreshold || m_interpolated.AreDifferent(m_lastVisual, m_latestValues[m_newestValueIndex]);
            m_firstUpdateLoop = false;
        }
        if (m_interpolate)
        {
            T newer = m_latestValues[m_newestValueIndex];
            T older = m_latestValues[GetOlderValueIndex()];
            m_interpolated.AffectOriginal(m_interpolated.GetInterpolatedValue(older, newer, InterpolationController.InterpolationFactor));
        }

        m_firstFixedLoop = true;
    }

    private int GetOlderValueIndex()
    {
        return (m_newestValueIndex + 1) % 2;
    }
}
