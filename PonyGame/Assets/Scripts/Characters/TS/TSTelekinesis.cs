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
    [Range(0, 40)]
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
    private float m_maxThrowVelocity = 20f;

    [SerializeField]
    [Range(0, 1)]
    private float m_massFactor = 1.0f;

    [SerializeField]
    [Range(0, 5)]
    private float m_distanceSensitivity = 1.0f;

    [SerializeField]
    [Range(0, 50)]
    private float m_rotateSpeed = 30.0f;

    [SerializeField]
    [Range(0, 1)]
    private float m_reorientTapTime = 0.35f;

    private TSMagic m_magic;
    private float m_distance;
    private float m_originalMaxAngVel;
    private float m_lastOrientTime;
    private bool m_reorienting = false;
    private bool m_throwing = false;

    private TKObject m_tkTarget;
    public TKObject TKTarget
    {
        get { return m_tkTarget; }
    }

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
                m_tkTarget.CollisionNotifier.OnCollision += OnTargetCollision;
                m_originalMaxAngVel = m_tkTarget.Rigidbody.maxAngularVelocity;
                m_tkTarget.Rigidbody.maxAngularVelocity = 40;
                m_distance = Mathf.Max(Vector3.Distance(newTarget.transform.position, transform.position), m_minDistance);

                m_magic.IsUsingMagic = true;
            }
            else
            {
                StopTK();
            }
        }

        if (m_tkTarget != null && (Vector3.Distance(transform.position, m_tkTarget.transform.position) > m_loseRange || !m_magic.CanUseMagic))
        {
            StopTK();
        }

        if (m_tkTarget != null)
        {
            m_distance = Mathf.Clamp(m_distance + (m_distanceSensitivity * Controls.AverageValue(GameAxis.TKDistance)), m_minDistance, m_maxGrabRange);

            if (Controls.JustDown(GameButton.Primary))
            {
                m_throwing = true;
            }

            if (Controls.JustDown(GameButton.Secondary))
            {
                if (m_reorienting)
                {
                    m_reorienting = false;
                }
                else if (Time.time - m_lastOrientTime < m_reorientTapTime)
                {
                    m_reorienting = true;
                }
                else
                {
                    m_lastOrientTime = Time.time;
                }
            }
            
            if (!m_reorienting && Controls.IsDown(GameButton.Secondary))
            {
                float rotateX = Controls.AverageValue(GameAxis.TKRotateX);
                float rotateY = Controls.AverageValue(GameAxis.TKRotateY);
                m_tkTarget.Rigidbody.angularVelocity = Camera.main.transform.rotation * (m_rotateSpeed * Vector3.ClampMagnitude(new Vector3(rotateY, -rotateX, 0), 1) / GetMassFactor());
            }
            else if (m_reorienting)
            {
                if (Quaternion.Angle(Camera.main.transform.rotation, m_tkTarget.Rigidbody.transform.rotation) > 2)
                {
                    Quaternion difference = Camera.main.transform.rotation * Quaternion.Inverse(m_tkTarget.Rigidbody.transform.rotation);
                    float angle;
                    Vector3 axis;
                    difference.ToAngleAxis(out angle, out axis);
                    angle = Utils.TransformAngle(angle);
                    float maxMagnitude = m_rotateSpeed / GetMassFactor();
                    m_tkTarget.Rigidbody.angularVelocity = Mathf.Clamp(angle, -maxMagnitude, maxMagnitude) * axis;
                }
                else
                {
                    m_tkTarget.Rigidbody.angularVelocity = Vector3.zero;
                    m_reorienting = false;
                }
            }
            else
            {
                m_tkTarget.Rigidbody.angularVelocity = (1 - m_angVelocityDamp) * m_tkTarget.Rigidbody.angularVelocity;
            }
        }

        if (m_tkTarget != null)
        {
            Transform cam = Camera.main.transform;
            Vector3 targetVelocity;
            if (!m_throwing)
            {
                float camToPlayerDistance = Vector3.Dot(cam.forward, (transform.position - cam.position));
                Vector3 targetPos = cam.position + (camToPlayerDistance + m_distance) * cam.forward;
                Vector3 spherePos = m_distance * (targetPos - transform.position).normalized + transform.position;
                Vector3 velocity = m_velocityScale * (spherePos - m_tkTarget.transform.position);
                Vector3 bobVelocity = m_bobStrength * Mathf.Sin(Time.time * m_bobFrequency) * Vector3.up;
                targetVelocity = Vector3.ClampMagnitude(velocity + bobVelocity, m_maxVelocity / GetMassFactor());
            }
            else
            {
                Vector3 dir = cam.forward;
                if (GameController.CameraRig.LockTarget != null)
                {
                    dir = (GameController.CameraRig.LockTarget.position - m_tkTarget.transform.position).normalized;
                }
                targetVelocity = dir * m_maxVelocity / GetMassFactor();
            }
            targetVelocity += ((m_gravityOffset / m_velocitySmoothing) * -Physics.gravity);
            m_tkTarget.Rigidbody.velocity = Vector3.Lerp(m_tkTarget.Rigidbody.velocity, targetVelocity, m_velocitySmoothing * Time.deltaTime);
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
            if (!tkObject.IsGrabbed && suitability > bestSuitability && distance < m_maxGrabRange)
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
            m_reorienting = false;
            m_throwing = false;
            m_tkTarget.Rigidbody.maxAngularVelocity = m_originalMaxAngVel;
            m_tkTarget.IsGrabbed = false;
            m_tkTarget.CollisionNotifier.OnCollision -= OnTargetCollision;

            m_tkTarget = null;
            m_magic.IsUsingMagic = false;
        }
    }

    private float GetMassFactor()
    {
        return m_tkTarget != null ? (m_massFactor * Mathf.Max(m_tkTarget.Rigidbody.mass, 0.5f)) + (1 - m_massFactor) : 1;
    }

    private void OnTargetCollision()
    {
        if (m_throwing)
        {
            StopTK();
        }
    }
}