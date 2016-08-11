using UnityEngine;

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

    public void ForgetPreviousValues()
    {
        m_interpolator.ForgetPreviousValues();
    }

    private void Update()
    {
        m_interpolator.Update();
    }
}