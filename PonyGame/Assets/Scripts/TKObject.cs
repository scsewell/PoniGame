using UnityEngine;
using System.Collections;

public class TKObject : MonoBehaviour
{
    private const float TRANSITION_TIME = 0.4f;
    
    [SerializeField]
    private GameObject m_particlePrefab;
    
    [SerializeField]
    private Rigidbody m_rigidbody;
    public Rigidbody Rigidbody
    {
        get { return m_rigidbody; }
    }

    private MeshRenderer m_renderer;
    private ParticleSystem.EmissionModule m_emission;
    private float m_baseEmissionRate;
    private Color m_color;
    private float m_emissionFraction;

    private bool m_isGrabbed = false;
    public bool IsGrabbed
    {
        get { return m_isGrabbed; }
        set { m_isGrabbed = value; }
    }
    
    private void Start()
    {
        m_renderer = GetComponent<MeshRenderer>();
        m_renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

        GameObject particles = Instantiate(m_particlePrefab, transform, false) as GameObject;
        ParticleSystem particleSystem = particles.GetComponent<ParticleSystem>();
        ParticleSystem.ShapeModule shape = particleSystem.shape;
        shape.meshRenderer = m_renderer;
        m_emission = particleSystem.emission;

        float scale = m_renderer.bounds.extents.magnitude;
        m_baseEmissionRate = m_emission.rate.constant * scale;
        m_renderer.material.SetFloat("_Displacement", m_renderer.material.GetFloat("_Displacement") * scale);
        m_renderer.material.SetFloat("_Amplitude", m_renderer.material.GetFloat("_Amplitude") * scale);
        m_renderer.material.SetFloat("_SpatialFrequency", m_renderer.material.GetFloat("_SpatialFrequency") / scale);

        m_color.a = 0;
        m_emissionFraction = 0;
    }

    private void Update()
    {
        m_renderer.enabled = (m_color.a > 0);

        float maxDelta = (1 / TRANSITION_TIME) * Time.deltaTime;
        m_color.a = Mathf.MoveTowards(m_color.a, m_isGrabbed ? 1 : 0, maxDelta);
        m_emissionFraction = Mathf.MoveTowards(m_emissionFraction, m_isGrabbed ? 1 : 0, maxDelta);
        
        if (m_renderer.enabled)
        {
            m_renderer.material.SetColor("_Color", m_color);
        }
        m_emission.rate = m_emissionFraction * m_baseEmissionRate;
    }

    public void SetColor(Color color)
    {
        m_color = new Color(color.r, color.g, color.b, m_color.a);
    }
}
