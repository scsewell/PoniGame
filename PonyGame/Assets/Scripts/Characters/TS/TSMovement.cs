using UnityEngine;
using System;
using System.Collections;
using System.Linq;

/*
 * Responsible for moving the player character
 */
public class TSMovement : MonoBehaviour
{
    [SerializeField]
    private PhysicMaterial m_rigidbodyPhysics;

    [SerializeField]
    private bool m_debugView = true;

    [SerializeField]
    private LayerMask m_groundSpherecast;

    [SerializeField]
    private LayerMask m_groundRaycast;

    [Tooltip("How fast the character may walk (Units / Second)")]
    [Range(0, 4)]
    public float walkSpeed = 1.0f;

    [Tooltip("How fast the character may run (Units / Second)")]
    [Range(0, 4)]
    public float runSpeed = 2.0f;

    [Tooltip("How fast the character accelerates forward (Units / Second^2)")]
    [Range(0, 10)]
    public float acceleration = 5.0f;

    [Tooltip("How fast the character may rotate (Degrees / Second)")]
    [Range(0, 2000)]
    public float rotSpeed = 120.0f;

    [Tooltip("How fast the character may rotate while in the air (Degrees / Second)")]
    [Range(0, 2000)]
    public float airRotSpeed = 45.0f;

    [Tooltip("The target angular velocity when the character is facing away from the goal direction (Degrees / Second)")]
    [Range(0, 2000)]
    public float oppositeAngVelocity = 100.0f;

    [Tooltip("The target angular velocity when the character is facing in the goal direction (Degrees / Second)")]
    [Range(0, 2000)]
    public float forwardAngVelocity = 100.0f;

    [Tooltip("The character's torque (Degrees / Second^2)")]
    [Range(0, 10000)]
    public float maxTorque = 100.0f;

    [Tooltip("How fast the character begins moving on jumping (Units / Second)")]
    [Range(0, 6)]
    public float jumpSpeed = 1.0f;

    [Tooltip("Fraction of the world's gravity is applied when in the air")]
    [Range(0, 5)]
    public float gravityFraction = 1.0f;

    [Tooltip("The number of raycasts done in a circle around the character to get an average ground normal")]
    [Range(0, 21)]
    public int normalSamples = 7;

    [Tooltip("The radius of the raycast circle around the character to get an average ground normal (Units)")]
    [Range(0, 1)]
    public float groundSmoothRadius = 0.1f;

    [Tooltip("The higher this value, the faster the character will align to the ground normal when on terrain")]
    [Range(0, 24)]
    public float groundAlignSpeed = 10.0f;

    [Tooltip("The higher this value, the faster the character will align upwards when midair")]
    [Range(0, 24)]
    public float airAlignSpeed = 1.5f;

    [Tooltip("Higher values allow the character to push rigidbodies faster")]
    [Range(0, 5)]
    public float pushStrength = 0.5f;


    private CharacterController m_controller;
    private TransformInterpolator m_transformInterpolator;

    private CollisionFlags m_collisionFlags;
    private float m_angVelocity = 0;
    private float m_velocityY = 0;


    private Vector3 m_actualVelocity = Vector3.zero;
    public Vector3 ActualVelocity
    {
        get { return m_actualVelocity; }
    }

    private float m_attemptedSpeed = 0;
    public float AttemptedSpeed
    {
        get { return m_attemptedSpeed; }
    }

    private bool m_isGrounded = false;
    public bool IsGrounded
    {
        get { return m_isGrounded; }
    }

    private Rigidbody m_rigidbody;
    private CapsuleCollider m_collider;
    private bool m_useRigidbody = false;
    private int m_toggleWaitFrames = 0;
    private bool m_wasPreviousFixedUpdate = false;

    void Start()
    {
        m_controller = GetComponent<CharacterController>();
        m_transformInterpolator = GetComponent<TransformInterpolator>();

        m_transformInterpolator.ForgetPreviousValues();

        GameObject go = new GameObject();
        go.layer = LayerMask.NameToLayer("Player");

        m_collider = go.AddComponent<CapsuleCollider>();
        m_collider.radius = m_controller.radius + m_controller.skinWidth;
        m_collider.height = m_controller.height;
        m_collider.material = m_rigidbodyPhysics;

        m_rigidbody = go.AddComponent<Rigidbody>();
        m_rigidbody.useGravity = false;
        m_rigidbody.freezeRotation = true;
        m_rigidbody.mass = 25;
        SetRigidbody(false, Vector3.zero);
    }

    private void SetRigidbody(bool enabled, Vector3 velocity)
    {
        if (m_toggleWaitFrames > 0)
        {
            return;
        }
        m_useRigidbody = enabled;
        m_collider.enabled = enabled;
        m_rigidbody.velocity = velocity;
        m_controller.enabled = !enabled;
        m_toggleWaitFrames = 3;
        //Debug.Log(enabled);
    }

    /*
     * Moves the character based on the provided input
     */
    public void ExecuteMovement(MoveInputs inputs)
    {
        if (m_useRigidbody && m_wasPreviousFixedUpdate)
        {
            Vector3 oldPosition = transform.position;
            transform.position = m_rigidbody.transform.position - (transform.TransformPoint(m_controller.center) - transform.position);
            m_actualVelocity = (transform.position - oldPosition) / Time.deltaTime;
        }

        // Move to a dedicated FixedUpdate?
        m_toggleWaitFrames--;

        m_attemptedSpeed = Mathf.MoveTowards(m_attemptedSpeed, inputs.Forward * (inputs.Run ? runSpeed : walkSpeed), acceleration * Time.deltaTime);
        Vector3 moveVelocity = transform.forward * m_attemptedSpeed;
        Vector3 lowerSphereCenter = transform.TransformPoint(m_controller.center) + Vector3.down * (m_controller.height * 0.5f - (m_controller.radius + 0.01f));
        RaycastHit[] hits = Physics.SphereCastAll(lowerSphereCenter, m_controller.radius, Vector3.down, 0.02f + m_controller.skinWidth, m_groundSpherecast);
        Debug.DrawLine(lowerSphereCenter + Vector3.down * m_controller.radius, lowerSphereCenter + Vector3.down * (m_controller.radius + m_controller.skinWidth + 0.02f), Color.magenta);
        m_isGrounded = hits != null && hits.Length > 0;

        if (m_isGrounded && hits.Length == 1)
        {
            RaycastHit hit;
            Physics.SphereCast(lowerSphereCenter, m_controller.radius, Vector3.down, out hit, 0.02f + m_controller.skinWidth, m_groundSpherecast);
            hits[0] = hit;
        }

        Vector3 alignNormal;
        if (m_isGrounded)
        {
            RaycastHit bestHit = hits.OrderBy(h => h.point.y).First();
            Debug.DrawLine(bestHit.point, bestHit.point + bestHit.normal, Color.magenta);

            NormalInfo normalInfo = GetGroundNormal(normalSamples, groundSmoothRadius, m_controller.slopeLimit, 90);
            alignNormal = normalInfo.limitedNormal ?? (normalInfo.normal ?? Vector3.up);

            //if (bestHit.normal != Vector3.up)
            //{
                bool isSteep = Vector3.Dot(bestHit.normal, Vector3.up) < Mathf.Cos(m_controller.slopeLimit * Mathf.Deg2Rad);
                if (isSteep && !m_useRigidbody)
                {
                    Debug.Log(bestHit.transform + " : " + hits.Length + " : " + bestHit.normal.y + " : " + Vector3.Dot(bestHit.normal, Vector3.up));
                    SetRigidbody(true, m_actualVelocity);
                }
                else if (!isSteep && m_useRigidbody)
                {
                    Debug.Log(bestHit.transform + " : " + hits.Length + " : " + bestHit.normal.y + " : " + Vector3.Dot(bestHit.normal, Vector3.up));
                    SetRigidbody(false, m_actualVelocity);
                }
            //}

            if (!m_useRigidbody)
            {
                if (inputs.Jump)
                {
                    m_velocityY = jumpSpeed;
                    SetRigidbody(true, new Vector3(m_actualVelocity.x, m_velocityY, m_actualVelocity.z));
                }
                else if (bestHit.transform.gameObject.layer == LayerMask.NameToLayer("Ground"))
                {
                    float slopeFactor = (1 - Mathf.Clamp(Vector3.Dot(bestHit.normal, Vector3.up), 0, 0.5f));
                    m_velocityY = (-0.05f * slopeFactor + (1 - slopeFactor) * -0.01f) / Time.deltaTime;
                }
                else
                {
                    m_velocityY = -0.5f;
                }
            }
        }
        else
        {
            alignNormal = Vector3.up;
        }

        Quaternion targetRot = Quaternion.LookRotation(Vector3.Cross(transform.right, alignNormal), alignNormal);
        if (Quaternion.Angle(transform.rotation, targetRot) > 2.0f)
        {
            //transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, (m_isGrounded ? groundAlignSpeed : airAlignSpeed) * Time.deltaTime);
        }

        m_velocityY += gravityFraction * Physics.gravity.y * Time.deltaTime;

        if (m_useRigidbody)
        {
            m_rigidbody.velocity += gravityFraction * Physics.gravity * Time.deltaTime;
        }
        else
        {
            Vector3 oldPosition = transform.position;
            m_collisionFlags = m_controller.Move(new Vector3(moveVelocity.x, m_velocityY, moveVelocity.z) * Time.deltaTime);
            m_rigidbody.transform.position = transform.TransformPoint(m_controller.center);
            m_rigidbody.velocity = Vector3.zero;
            m_actualVelocity = (transform.position - oldPosition) / Time.deltaTime;
        }

        //Vector3 oldPosition = transform.position;
        //m_collisionFlags = m_controller.Move(new Vector3(moveVelocity.x, m_velocityY, moveVelocity.z) * Time.deltaTime);
        //m_actualVelocity = (transform.position - oldPosition) / Time.deltaTime;
        
        float maxTurnSpeed = m_isGrounded ? rotSpeed : airRotSpeed;
        float targetAngVelocity = forwardAngVelocity * Mathf.Sign(inputs.Turn) + (oppositeAngVelocity - forwardAngVelocity) * (inputs.Turn / 180);
        m_angVelocity = Mathf.Clamp(Mathf.MoveTowards(m_angVelocity, targetAngVelocity, maxTorque * Time.deltaTime), -maxTurnSpeed, maxTurnSpeed);

        float deltaRotation = m_angVelocity * Time.deltaTime;
        if (Mathf.Abs(inputs.Turn) < Mathf.Abs(deltaRotation))
        {
            deltaRotation = inputs.Turn;
            m_angVelocity = 0;
        }

        transform.Rotate(0, deltaRotation, 0, Space.Self);

        m_wasPreviousFixedUpdate = true;
    }

    public void Update()
    {
        if (m_useRigidbody && m_wasPreviousFixedUpdate)
        {
            Vector3 oldPosition = transform.position;
            transform.position = m_rigidbody.transform.position - (transform.TransformPoint(m_controller.center) - transform.position);
            m_actualVelocity = (transform.position - oldPosition) / Time.deltaTime;
            m_wasPreviousFixedUpdate = false;
        }
    }

    /*
     * Moves rigidbodies that are blocking the characters path and are moveable
     */
    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        // dont move the rigidbody if the character is on top of it
        if (m_collisionFlags == CollisionFlags.Below)
        {
            return;
        }

        // if we are airborne and hit our head reverse our vertical velocity
        if ((m_collisionFlags & CollisionFlags.Above) != 0 && !m_controller.isGrounded)
        {
            m_velocityY *= -0.5f;
        }

        Rigidbody body = hit.collider.attachedRigidbody;

        if (body == null || body.isKinematic)
        {
            return;
        }

        Vector3 pushDir = new Vector3(hit.moveDirection.x, 0, hit.moveDirection.y);
        body.AddForceAtPosition(pushDir * pushStrength, hit.point, ForceMode.Impulse);
    }


    /*
     * Samples the ground beneath the character in a circle and returns the average normal of the ground at those points
     */
    private NormalInfo GetGroundNormal(int samples, float radius, float slopeAngleLimit, float steepAngleLimit)
    {
        Vector3 limitedNormal = Vector3.zero;
        Vector3 normal = Vector3.zero;

        for (int i = 0; i < samples; i++)
        {
            Vector3 offset = Quaternion.Euler(0, i * (360.0f / samples), 0) * Vector3.forward * radius;
            Vector3 samplePos = transform.TransformPoint(offset + m_controller.center);
            Vector3 sampleDir = transform.TransformPoint(offset + Vector3.down * 0.05f);

            RaycastHit hit;
            if (Physics.Linecast(samplePos, sampleDir, out hit, m_groundRaycast))
            {
                if (Vector3.Dot(hit.normal, Vector3.up) > Mathf.Cos(slopeAngleLimit * Mathf.Deg2Rad))
                {
                    limitedNormal += hit.normal;
                }
                if (Vector3.Dot(hit.normal, Vector3.up) > Mathf.Cos(steepAngleLimit * Mathf.Deg2Rad))
                {
                    normal += hit.normal;
                }

                if (m_debugView)
                {
                    Debug.DrawLine(samplePos, sampleDir, Color.cyan);
                    Debug.DrawLine(hit.point, hit.point + hit.normal * 0.25f, Color.green);
                }
            }
        }

        if (m_debugView && limitedNormal != Vector3.zero)
        {
            Debug.DrawLine(transform.position, transform.position + limitedNormal.normalized * 0.5f, Color.red);
        }
        return new NormalInfo(
            limitedNormal != Vector3.zero ? (Vector3?)limitedNormal.normalized : null,
            normal != Vector3.zero ? (Vector3?)normal.normalized : null
        );
    }

    private class NormalInfo
    {
        public Vector3? limitedNormal;
        public Vector3? normal;

        public NormalInfo(Vector3? limitedNormal, Vector3? normal)
        {
            this.limitedNormal = limitedNormal;
            this.normal = normal;
        }
    }
}