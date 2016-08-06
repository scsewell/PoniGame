using UnityEngine;

[RequireComponent(typeof(InterpolatorUpdater))]

class TransformInterpolator : MonoBehaviour
{
    private Interpolator<TransformData> m_interpolator;
    
    private void Start()
    {
        m_interpolator = new Interpolator<TransformData>(new InterpolatedTransform(transform));
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

    public void ForgetPreviousValues()
    {
        m_interpolator.ForgetPreviousValues();
    }

    private void Update()
    {
        m_interpolator.Update();
    }
}