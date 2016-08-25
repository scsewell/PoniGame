using UnityEngine;
using System.Collections;

public class TKObject : MonoBehaviour
{
    [SerializeField]
    private GameObject m_particlePrefab;

    [SerializeField]
    [Range(0, 10)]
    private float m_emissionMultiplier = 1;

    [SerializeField]
    [Range(0, 1)]
    private float m_transitionTime = 0.5f;

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

    private bool m_active = false;
    public bool Active
    {
        get { return m_active; }
        set { m_active = value; }
    }
    

    private void Start()
    {
        m_renderer = GetComponent<MeshRenderer>();

        GameObject particles = Instantiate(m_particlePrefab, transform, false) as GameObject;
        ParticleSystem particleSystem = particles.GetComponent<ParticleSystem>();
        ParticleSystem.ShapeModule shape = particleSystem.shape;
        shape.meshRenderer = m_renderer;
        m_emission = particleSystem.emission;
        m_baseEmissionRate = m_emission.rate.constant;

        m_color.a = 0;
        m_emissionFraction = 0;
    }

    private void Update()
    {
        m_renderer.enabled = (m_color.a > 0);

        float maxDelta = (1 / m_transitionTime) * Time.deltaTime;
        m_color.a = Mathf.MoveTowards(m_color.a, m_active ? 1 : 0, maxDelta);
        m_emissionFraction = Mathf.MoveTowards(m_emissionFraction, m_active ? 1 : 0, maxDelta);
        
        if (m_renderer.enabled)
        {
            m_renderer.material.SetColor("_Color", m_color);
        }
        m_emission.rate = m_emissionFraction * m_baseEmissionRate * m_emissionMultiplier;
    }

    public void SetColor(Color color)
    {
        m_color = new Color(color.r, color.g, color.b, m_color.a);
    }
}
