using UnityEngine;

public class TransformInterpolator : MonoBehaviour
{
    [SerializeField] public bool useThresholds = false;
    [SerializeField] private float m_postionThreshold = 0.001f;
    [SerializeField] private float m_rotationThreshold = 0.1f;
    [SerializeField] private float m_scaleThreshold = 0.001f;
    
    private Interpolator<TransformData> m_interpolator;
    private InterpolatedTransform m_interpolated;

    private void Awake()
    {
        m_interpolated = new InterpolatedTransform(transform);
        m_interpolator = new Interpolator<TransformData>(m_interpolated, useThresholds);
    }

    private void Start()
    {
        m_interpolator.Start();
    }

    public void SetThresholds(bool useThresholds, float positionThreshold, float rotationThreshold, float scaleThreshold)
    {
        m_interpolator.UseThreshold = useThresholds;
        m_interpolated.SetThresholds(positionThreshold, rotationThreshold, scaleThreshold);

        this.useThresholds = useThresholds;
        m_postionThreshold = positionThreshold;
        m_rotationThreshold = rotationThreshold;
        m_scaleThreshold = scaleThreshold;
    }

    private void FixedUpdate()
    {
        m_interpolator.UseThreshold = useThresholds;
        m_interpolated.SetThresholds(m_postionThreshold, m_rotationThreshold, m_scaleThreshold);
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