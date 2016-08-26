using UnityEngine;
using System.Collections;
using System.Linq;

public class TSMagic : MonoBehaviour
{
    [SerializeField]
    private Color m_magicColor = Color.white;
    public Color MagicColor
    {
        get { return m_magicColor; }
    }

    [SerializeField]
    private MeshRenderer m_hornMagic;

    [SerializeField]
    private GameObject m_particlePrefab;

    [SerializeField]
    [Range(0,1)]
    private float m_transitionTime = 0.3f;

    private ParticleSystem.EmissionModule m_hornEmission;
    private float m_baseEmissionRate;
    private Color m_hornMagicColor;
    private float m_emissionFraction;

    private bool m_canUseMagic = true;
    public bool CanUseMagic
    {
        get { return m_canUseMagic; }
        set { m_canUseMagic = value; }
    }

    private bool m_isUsingMagic = false;
    public bool IsUsingMagic
    {
        get { return m_isUsingMagic; }
        set { m_isUsingMagic = value; }
    }

    private void Start()
    {
        m_hornMagic.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

        GameObject particles = Instantiate(m_particlePrefab, m_hornMagic.transform, false) as GameObject;
        ParticleSystem particleSystem = particles.GetComponent<ParticleSystem>();
        ParticleSystem.ShapeModule shape = particleSystem.shape;
        shape.meshRenderer = m_hornMagic;
        m_hornEmission = particleSystem.emission;

        float scale = m_hornMagic.bounds.extents.magnitude;
        m_baseEmissionRate = m_hornEmission.rate.constant * scale;
        m_hornMagic.material.SetFloat("_Displacement", m_hornMagic.material.GetFloat("_Displacement") * scale);
        m_hornMagic.material.SetFloat("_Amplitude", m_hornMagic.material.GetFloat("_Amplitude") * scale);
        m_hornMagic.material.SetFloat("_SpatialFrequency", m_hornMagic.material.GetFloat("_SpatialFrequency") / scale);

        m_hornMagicColor = m_magicColor;
        m_hornMagicColor.a = 0;
        m_emissionFraction = 0;
    }

    public void FixedUpdate()
    {

	}

    public void UpdateVisuals()
    {
        m_hornMagic.enabled = (m_hornMagicColor.a > 0);

        float maxDelta = (1 / m_transitionTime) * Time.deltaTime;
        m_hornMagicColor.a = Mathf.MoveTowards(m_hornMagicColor.a, m_isUsingMagic ? 1 : 0, maxDelta);
        m_emissionFraction = Mathf.MoveTowards(m_emissionFraction, m_isUsingMagic ? 1 : 0, maxDelta);

        if (m_hornMagic.enabled)
        {
            m_hornMagic.material.SetColor("_Color", m_hornMagicColor);
        }
        m_hornEmission.rate = m_emissionFraction * m_baseEmissionRate;
    }
}
