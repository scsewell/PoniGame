using UnityEngine;

class TransformInterpolator : MonoBehaviour
{
    [SerializeField]
    private bool m_useThresholds = false;
    public bool UseThresholds
    {
        set
        {
            m_useThresholds = value;
            m_interpolator.UseThreshold = m_useThresholds;
        }
    }

    [SerializeField]
    private float m_postionThreshold = 0.001f;

    [SerializeField]
    private float m_rotationThreshold = 0.1f;

    [SerializeField]
    private float m_scaleThreshold = 0.001f;


    private Interpolator<TransformData> m_interpolator;
    private InterpolatedTransform m_interpolated;
    
    private void Awake()
    {
        m_interpolated = new InterpolatedTransform(transform);
        m_interpolator = new Interpolator<TransformData>(m_interpolated, m_useThresholds);
    }

    private void Start()
    {
        m_interpolator.Start();
        SetThresholds(m_postionThreshold, m_rotationThreshold, m_scaleThreshold);
    }

    public void SetThresholds(float position, float rotation, float scale)
    {
        m_postionThreshold = position;
        m_rotationThreshold = rotation;
        m_scaleThreshold = scale;
        m_interpolated.SetThresholds(position, rotation, scale);
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