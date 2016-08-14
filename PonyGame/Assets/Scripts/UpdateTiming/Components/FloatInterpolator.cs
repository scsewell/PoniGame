using UnityEngine;

public class FloatInterpolator : MonoBehaviour
{
    private Interpolator<float> m_interpolator;
    private InterpolatedFloat m_interpolated;
    private bool m_initialized = false;

    public Interpolator<float> Initialize(InterpolatedFloat interpolated, bool useThreshold = false)
    {
        m_interpolator = new Interpolator<float>(interpolated, useThreshold);
        m_interpolated = interpolated;
        m_initialized = true;
        return m_interpolator;
    }

    public void SetThreshold(float threshold)
    {
        m_interpolated.SetThreshold(threshold);
    }

    private void Start()
    {
        if (m_interpolator == null)
        {
            Debug.LogError("float interpolator is null on " + transform.name);
        }
        if (!m_initialized)
        {
            Debug.LogError("float interpolator was not initialized on " + transform.name);
        }
        m_interpolator.Start();
    }

    private void FixedUpdate()
    {
        m_interpolator.FixedUpdate();
    }

    private void Update()
    {
        m_interpolator.Update();
    }
}