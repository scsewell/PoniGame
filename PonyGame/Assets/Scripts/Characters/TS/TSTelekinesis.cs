using UnityEngine;
using System.Collections;
using System.Linq;

public class TSTelekinesis : MonoBehaviour
{
    [SerializeField]
    private LayerMask m_lineOfSightBlocking;

    [SerializeField]
    [Range(0, 1)]
    private float m_minGrabCenterness = 0.98f;

    [SerializeField]
    [Range(0, 20)]
    private float m_maxGrabRange = 10f;

    [SerializeField]
    [Range(0, 20)]
    private float m_loseRange = 11f;

    [SerializeField]
    [Range(0, 2)]
    private float m_minDistance = 1f;

    [SerializeField]
    [Range(0, 20)]
    private float m_velocityScale = 4f;

    [SerializeField]
    [Range(0, 20)]
    private float m_maxVelocity = 6f;

    [SerializeField]
    [Range(0, 16)]
    private float m_velocitySmoothing = 4f;

    [SerializeField]
    [Range(0, 10)]
    private float m_bobFrequency = 1.0f;

    [SerializeField]
    [Range(0, 5)]
    private float m_bobStrength = 1.0f;

    [SerializeField]
    [Range(0, 2)]
    private float m_gravityOffset = 1f;

    [SerializeField]
    [Range(0, 1)]
    private float m_angVelocityDamp = 0.05f;

    [SerializeField]
    [Range(0, 40)]
    private float m_throwVelocity = 15f;

    [SerializeField]
    [Range(0, 1)]
    private float m_throwMassFactor = 1.0f;

    private TSMagic m_magic;

    private TKObject m_tkTarget;
    private float m_distance;

    private void Start()
    {
        m_magic = GetComponent<TSMagic>();
    }

    private void FixedUpdate()
    {
        if (Controls.JustDown(GameButton.TK) && (m_magic.IsUsingMagic != (m_tkTarget == null)))
        {
            TKObject newTarget = (m_tkTarget == null) ? FindTKTarget() : null;
            if (newTarget != null)
            {
                m_tkTarget = newTarget;
                m_tkTarget.IsGrabbed = true;
                m_tkTarget.SetColor(m_magic.MagicColor);
                m_magic.IsUsingMagic = true;
                m_distance = Mathf.Max(Vector3.Distance(newTarget.transform.position, transform.position), m_minDistance);
            }
            else
            {
                StopTK();
            }
        }
        
        if (m_tkTarget != null && Controls.JustDown(GameButton.Primary))
        {
            float speed = m_throwVelocity * ((m_throwMassFactor / m_tkTarget.Rigidbody.mass) + (1 - m_throwMassFactor));
            m_tkTarget.Rigidbody.velocity = speed * Camera.main.transform.forward;
            StopTK();
        }

        if (m_tkTarget != null && (Vector3.Distance(transform.position, m_tkTarget.transform.position) > m_loseRange || !m_magic.CanUseMagic))
        {
            StopTK();
        }

        if (m_tkTarget != null)
        {
            Transform cam = Camera.main.transform;
            float camToPlayerDistance = Vector3.Dot(cam.forward, (transform.position - cam.position));
            Vector3 targetPos = cam.position + (camToPlayerDistance + m_distance) * cam.forward;
            Vector3 spherePos = m_distance * (targetPos - transform.position).normalized + transform.position;
            Vector3 velocity = m_velocityScale * (spherePos - m_tkTarget.transform.position);
            Vector3 bobVelocity = m_bobStrength * Mathf.Sin(Time.time * m_bobFrequency) * Vector3.up;
            Vector3 targetVelocity = Vector3.ClampMagnitude(velocity + bobVelocity, m_maxVelocity) + ((m_gravityOffset / m_velocitySmoothing) * -Physics.gravity);
            m_tkTarget.Rigidbody.velocity = Vector3.Lerp(m_tkTarget.Rigidbody.velocity, targetVelocity, m_velocitySmoothing * Time.deltaTime);
            m_tkTarget.Rigidbody.angularVelocity = (1 - m_angVelocityDamp) * m_tkTarget.Rigidbody.angularVelocity;
        }
    }

    private TKObject FindTKTarget()
    {
        Transform cam = Camera.main.transform;
        TKObject mostSuitable = null;
        float bestSuitability = m_minGrabCenterness;
        foreach (GameObject go in GameObject.FindGameObjectsWithTag("Telekinesis"))
        {
            TKObject tkObject = go.GetComponent<TKObject>();
            Vector3 disp = (go.transform.position - cam.position);
            float suitability = Vector3.Dot(cam.forward, disp.normalized);
            float distance = Vector3.Distance(go.transform.position, transform.position);
            if (suitability > bestSuitability && distance < m_maxGrabRange)
            {
                RaycastHit[] hits = Physics.RaycastAll(cam.position, disp, disp.magnitude, m_lineOfSightBlocking);
                if (!hits.Any(hit => hit.collider.attachedRigidbody != tkObject.Rigidbody))
                {
                    mostSuitable = tkObject;
                    bestSuitability = suitability;
                }
            }
        }
        return mostSuitable;
    }

    private void StopTK()
    {
        if (m_tkTarget != null)
        {
            m_tkTarget.IsGrabbed = false;
            m_tkTarget = null;
            m_magic.IsUsingMagic = false;
        }
    }
}
