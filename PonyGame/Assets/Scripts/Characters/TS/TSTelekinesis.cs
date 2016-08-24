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


    private Rigidbody m_tkTarget;
    private float m_distance;


    private void FixedUpdate()
    {
        if (Controls.JustDown(GameButton.Telekinesis))
        {
            m_tkTarget = (m_tkTarget == null) ? FindTKTarget() : null;
            if (m_tkTarget != null)
            {
                m_distance = Vector3.Distance(m_tkTarget.transform.position, transform.position);
            }
        }

        if (m_tkTarget != null && Vector3.Distance(transform.position, m_tkTarget.transform.position) > m_loseRange)
        {
            m_tkTarget = null;
        }

        if (m_tkTarget != null)
        {
            Transform cam = Camera.main.transform;
            float camToPlayerDistance = Vector3.Dot(cam.forward, (transform.position - cam.position));
            Vector3 targetPos = cam.position + (camToPlayerDistance + m_distance) * cam.forward;
            Vector3 velocity = m_velocityScale * (targetPos - m_tkTarget.transform.position);
            Vector3 bobVelocity = m_bobStrength * Mathf.Sin(Time.time * m_bobFrequency) * Vector3.up;
            Vector3 targetVelocity = Vector3.ClampMagnitude(velocity + bobVelocity, m_maxVelocity) + ((m_gravityOffset / m_velocitySmoothing) * -Physics.gravity);
            m_tkTarget.velocity = Vector3.Lerp(m_tkTarget.velocity, targetVelocity, m_velocitySmoothing * Time.deltaTime);
            m_tkTarget.angularVelocity = (1 - m_angVelocityDamp) * m_tkTarget.angularVelocity;
        }
    }

    private Rigidbody FindTKTarget()
    {
        Transform cam = Camera.main.transform;
        Rigidbody mostSuitable = null;
        float bestSuitability = m_minGrabCenterness;
        foreach (GameObject go in GameObject.FindGameObjectsWithTag("Telekinesis"))
        {
            Rigidbody body = go.transform.root.GetComponent<Rigidbody>();
            if (body == null)
            {
                continue;
            }
            Vector3 dir = (go.transform.position - cam.position).normalized;
            float suitability = Vector3.Dot(cam.forward, dir);
            float distance = Vector3.Distance(go.transform.position, transform.position);
            if (suitability > bestSuitability && distance < m_maxGrabRange && Physics.Linecast(cam.position, go.transform.position, m_lineOfSightBlocking))
            {
                mostSuitable = body;
                bestSuitability = suitability;
            }
        }
        return mostSuitable;
    }
}
