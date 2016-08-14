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
    
    private float m_angVelocity = 0;


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

    private bool m_isRunning = false;
    public bool IsRunning
    {
        get { return m_isRunning; }
    }

    void Start()
    {
        m_controller = GetComponent<CharacterController>();
        m_transformInterpolator = GetComponent<TransformInterpolator>();

        m_transformInterpolator.ForgetPreviousValues();
    }

    /*
     * Moves the character based on the provided input
     */
    public void ExecuteMovement(MoveInputs inputs)
    {
        m_isRunning = inputs.Run;
        m_attemptedSpeed = Mathf.MoveTowards(m_attemptedSpeed, inputs.Forward * (inputs.Run ? runSpeed : walkSpeed), acceleration * Time.deltaTime);
        Vector3 moveVelocity = transform.forward * m_attemptedSpeed;
        moveVelocity.y = m_actualVelocity.y;

        Vector3 lowerSphereCenter = transform.TransformPoint(m_controller.center) + Vector3.down * (m_controller.height * 0.5f - (m_controller.radius + 0.01f));
        RaycastHit[] hits = Physics.SphereCastAll(lowerSphereCenter, m_controller.radius, Vector3.down, 0.02f + m_controller.skinWidth, m_groundSpherecast, QueryTriggerInteraction.Ignore);
        m_isGrounded = hits != null && hits.Length > 0;

        // if there is only one hit, Unity doesn't always return the correct normal, using Vector3.Up instead with SphereCastAll. SphereCast works however...
        if (hits != null && hits.Length == 1)
        {
            RaycastHit hit;
            m_isGrounded = Physics.SphereCast(lowerSphereCenter, m_controller.radius, Vector3.down, out hit, 0.02f + m_controller.skinWidth, m_groundSpherecast, QueryTriggerInteraction.Ignore);
            hits[0] = hit;
        }

        Vector3 alignNormal;
        if (m_isGrounded)
        {
            RaycastHit bestHit = hits.OrderBy(h => h.point.y).First();

            NormalInfo normalInfo = GetGroundNormal(normalSamples, groundSmoothRadius, m_controller.slopeLimit, 90);
            alignNormal = normalInfo.limitedNormal ?? (normalInfo.normal ?? Vector3.up);
            
            if (Vector3.Dot(bestHit.normal, Vector3.up) < Mathf.Cos(m_controller.slopeLimit * Mathf.Deg2Rad) && moveVelocity.y <= 0)
            {
                moveVelocity = Vector3.ProjectOnPlane(Vector3.down * 150 * Time.deltaTime, bestHit.normal);
            }
            else if (inputs.Jump)
            {
                moveVelocity.y = jumpSpeed;
            }
            else if (bestHit.transform.gameObject.layer == LayerMask.NameToLayer("Ground"))
            {
                float slopeFactor = (1 - Mathf.Clamp(Vector3.Dot(bestHit.normal, Vector3.up), 0, 0.5f));
                moveVelocity.y = (-0.05f * slopeFactor + (1 - slopeFactor) * -0.01f) / Time.deltaTime;
            }
        }
        else
        {
            alignNormal = Vector3.up;
        }

        Quaternion targetRot = Quaternion.LookRotation(Vector3.Cross(transform.right, alignNormal), alignNormal);
        if (Quaternion.Angle(transform.rotation, targetRot) > 2.0f)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, (m_isGrounded ? groundAlignSpeed : airAlignSpeed) * Time.deltaTime);
        }

        moveVelocity += gravityFraction * Physics.gravity * Time.deltaTime;

        Vector3 oldPosition = transform.position;
        m_controller.Move(moveVelocity * Time.deltaTime);
        m_actualVelocity = (transform.position - oldPosition) / Time.deltaTime;
        m_actualVelocity.y = m_actualVelocity.y > 0 ? Mathf.Min(m_actualVelocity.y, moveVelocity.y) : Mathf.Max(m_actualVelocity.y, moveVelocity.y);

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
    }

    /*
     * Moves rigidbodies that are blocking the characters path and are moveable
     */
    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        Rigidbody rigidbody = hit.collider.attachedRigidbody;
        if (!(rigidbody == null || rigidbody.isKinematic || Vector3.Dot(hit.normal, Vector3.up) > 0.5f))
        {
            rigidbody.AddForceAtPosition(m_controller.velocity * pushStrength, hit.point, ForceMode.Impulse);
        }
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