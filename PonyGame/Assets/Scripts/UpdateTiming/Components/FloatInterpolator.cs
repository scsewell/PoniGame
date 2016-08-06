using UnityEngine;

[RequireComponent(typeof(InterpolatorUpdater))]

class FloatInterpolator : MonoBehaviour
{
    private Interpolator<float> m_interpolator;
    private bool m_initialized = false;

    public Interpolator<float> Initialize(Interpolator<float> interpolator)
    {
        m_interpolator = interpolator;
        m_initialized = true;
        return m_interpolator;
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

    public void LateFixedUpdate()
    {
        m_interpolator.LateFixedUpdate();
    }

    private void Update()
    {
        m_interpolator.Update();
    }
}