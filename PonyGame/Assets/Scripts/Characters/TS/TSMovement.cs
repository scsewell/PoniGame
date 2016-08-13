using UnityEngine;
using System;
using System.Collections;

/*
 * Responsible for moving the player character
 */
public class TSMovement : MonoBehaviour
{
    [SerializeField]
    private bool m_debugView = true;

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
    private float m_forwardVelocity = 0;
    private float m_angVelocity = 0;
    private float m_velocityY = 0;


    private Vector3 m_actualVelocity = Vector3.zero;
    public Vector3 ActualVelocity
    {
        get { return m_actualVelocity; }
    }

    private Vector3 m_lastVelocity = Vector3.zero;
    public Vector3 AttemptedVelocity
    {
        get { return m_lastVelocity; }
    }

    public bool IsGrounded
    {
        get { return m_controller.isGrounded; }
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
        //m_forwardVelocity = Vector3.ProjectOnPlane(m_actualVelocity, Vector3.up).magnitude;
        m_velocityY = m_actualVelocity.y;

        // linearly accelerate towards some target velocity
        m_forwardVelocity = Mathf.MoveTowards(m_forwardVelocity, inputs.Forward * (inputs.Run ? runSpeed : walkSpeed), acceleration * Time.deltaTime);
        Vector3 moveVelocity = transform.forward * m_forwardVelocity;
        m_lastVelocity = moveVelocity;

        RaycastHit hit;
        Vector3 lowerSphereCenter = transform.TransformPoint(m_controller.center) + Vector3.down * (m_controller.height * 0.5f - m_controller.radius);
        bool onGround = Physics.SphereCast(lowerSphereCenter, m_controller.radius, Vector3.down, out hit, 0.01f, m_groundRaycast);

        Vector3 alignNormal;
        if (onGround)
        {
            NormalInfo normalInfo = GetGroundNormal(normalSamples, groundSmoothRadius, m_controller.slopeLimit, 90);
            alignNormal = normalInfo.limitedNormal ?? (normalInfo.normal ?? Vector3.up);

            if (inputs.Jump && normalInfo.limitedNormal.HasValue)
            {
                // jumping
                m_velocityY = jumpSpeed;
            }
            else
            {
                if (!normalInfo.limitedNormal.HasValue)
                {
                    moveVelocity += Vector3.ProjectOnPlane(alignNormal, Vector3.up).normalized;
                    m_actualVelocity += Vector3.ProjectOnPlane(gravityFraction * Physics.gravity * Time.deltaTime, alignNormal);
                }
            }
        }
        else
        {
            // orient the character up when in the air
            alignNormal = Vector3.up;
        }

        Quaternion targetRot = Quaternion.LookRotation(Vector3.Cross(transform.right, alignNormal), alignNormal);
        if (Quaternion.Angle(transform.rotation, targetRot) > 2.0f)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, (onGround ? groundAlignSpeed : airAlignSpeed) * Time.deltaTime);
        }

        m_velocityY += gravityFraction * Physics.gravity.y * Time.deltaTime;

        Vector3 oldPosition = transform.position;
        Vector3 move = new Vector3(moveVelocity.x, m_velocityY, moveVelocity.z) * Time.deltaTime;
        m_collisionFlags = m_controller.Move(move);
        //m_collisionFlags = m_controller.Move(new Vector3(gravityFraction * Physics.gravity.y * Time.deltaTime, 0, 0));
        //m_collisionFlags = m_controller.Move(new Vector3(0, gravityFraction * Physics.gravity.y * Time.deltaTime, 0));
        m_actualVelocity = (transform.position - oldPosition) / Time.deltaTime;
        Debug.Log(m_actualVelocity);

        float maxTurnSpeed = onGround ? rotSpeed : airRotSpeed;
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

    public void ExecuteMovementZZZ(MoveInputs inputs)
    {
        //m_collisionFlags = m_controller.Move(-1 * Vector3.up);
        String toLog = "";

        toLog += "  a:" + m_actualVelocity + " : " + m_actualVelocity.magnitude;
        /*
        Vector3 forwardVelocityVector = Vector3.ProjectOnPlane(m_actualVelocity, Vector3.up);
        float forwardVelocity = forwardVelocityVector.magnitude;
        forwardVelocity = Mathf.MoveTowards(forwardVelocity, inputs.Forward * (inputs.Run ? runSpeed : walkSpeed), acceleration * Time.deltaTime);
        m_actualVelocity = forwardVelocity * Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized + (m_actualVelocity - forwardVelocityVector);
        /**/

        /*
        Vector3 forwardVelocityVector = Vector3.ProjectOnPlane(m_actualVelocity, transform.up);
        float forwardVelocity = forwardVelocityVector.magnitude;
        forwardVelocity = Mathf.Clamp(Mathf.MoveTowards(forwardVelocity, inputs.Forward * (inputs.Run ? runSpeed : walkSpeed), acceleration * Time.deltaTime), -runSpeed, runSpeed);
        m_actualVelocity = forwardVelocity * transform.forward + (m_actualVelocity - forwardVelocityVector);
        /**/

        /**/
        Vector3 forwardVelocity = inputs.Forward * (inputs.Run ? runSpeed : walkSpeed) * transform.forward;
        m_actualVelocity = Vector3.ClampMagnitude(Vector3.Lerp(m_actualVelocity, forwardVelocity, 0.1f), runSpeed);
        //m_actualVelocity = Vector3.Lerp(m_actualVelocity, forwardVelocity, 0.1f);
        /**/
        toLog += "  b:" + m_actualVelocity + " : " + m_actualVelocity.magnitude;

        RaycastHit hit;
        Vector3 lowerSphereCenter = transform.TransformPoint(m_controller.center) + Vector3.down * (m_controller.height * 0.5f - m_controller.radius);
        bool onGround = Physics.SphereCast(lowerSphereCenter, m_controller.radius, Vector3.down, out hit, 0.01f, m_groundRaycast);

        Vector3 alignNormal;
        if (onGround)
        {
            NormalInfo normalInfo = GetGroundNormal(normalSamples, groundSmoothRadius, m_controller.slopeLimit, 90);
            alignNormal = normalInfo.limitedNormal ?? (normalInfo.normal ?? Vector3.up);

            if (!normalInfo.limitedNormal.HasValue)
            {
                m_actualVelocity += Vector3.ProjectOnPlane(gravityFraction * Physics.gravity * Time.deltaTime, alignNormal);
                toLog += "  c1:" + m_actualVelocity + " : " + m_actualVelocity.magnitude;
            }
            else if (inputs.Jump)
            {
                // jumping
                m_actualVelocity.y = jumpSpeed;
                toLog += "  c2:" + m_actualVelocity + " : " + m_actualVelocity.magnitude;
            }
        }
        else
        {
            // orient the character up when in the air
            alignNormal = Vector3.up;
        }

        Quaternion targetRot = Quaternion.LookRotation(Vector3.Cross(transform.right, alignNormal), alignNormal);
        if (Quaternion.Angle(transform.rotation, targetRot) > 2.0f)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, (onGround ? groundAlignSpeed : airAlignSpeed) * Time.deltaTime);
        }

        m_actualVelocity += gravityFraction * Physics.gravity * Time.deltaTime;
        toLog += "  d:" + m_actualVelocity + " : " + m_actualVelocity.magnitude;

        Vector3 oldPosition = transform.position;
        //toLog += "  dz:" + (m_actualVelocity * Time.deltaTime) + " : " + (m_actualVelocity * Time.deltaTime).magnitude;
        toLog += "  ein:" + Vector3.ProjectOnPlane(m_actualVelocity, Vector3.up) + " : " + Vector3.ProjectOnPlane(m_actualVelocity, Vector3.up).magnitude;
        //m_collisionFlags = m_controller.Move(m_actualVelocity * Time.deltaTime);
        m_collisionFlags = m_controller.Move(1 * Vector3.up);
        m_collisionFlags = m_controller.Move(Vector3.ProjectOnPlane(m_actualVelocity, Vector3.up) * Time.deltaTime);
        toLog += "  e:" + (transform.position - oldPosition) / Time.deltaTime + " : " + (transform.position - oldPosition).magnitude / Time.deltaTime;
        Vector3 midPosition = transform.position;
        toLog += "  fin:" + (m_actualVelocity.y * Vector3.up) + " : " + (m_actualVelocity.y * Vector3.up).magnitude;
        m_collisionFlags = m_controller.Move(-1 * Vector3.up);
        m_collisionFlags = m_controller.Move(m_actualVelocity.y * Vector3.up * Time.deltaTime);
        //toLog += "  ez:" + (transform.position - oldPosition) + " : " + (transform.position - oldPosition).magnitude;
        //m_collisionFlags = m_controller.Move(new Vector3(gravityFraction * Physics.gravity.y * Time.deltaTime, 0, 0));
        //m_collisionFlags = m_controller.Move(new Vector3(0, gravityFraction * Physics.gravity.y * Time.deltaTime, 0));
        m_actualVelocity = (transform.position - oldPosition) / Time.deltaTime;
        toLog += "  f:" + (transform.position - midPosition) / Time.deltaTime + " : " + (transform.position - midPosition).magnitude / Time.deltaTime;
        toLog += "  g:" + m_actualVelocity + " : " + m_actualVelocity.magnitude;
        //Debug.Log(m_actualVelocity);
        m_lastVelocity = m_actualVelocity;

        //Debug.Log(m_actualVelocity.magnitude);
        if (m_actualVelocity.magnitude > 10)
        {
            int asdf = 1;
            //int asdf2 = 0 / (1 - asdf);
        }

        float maxTurnSpeed = onGround ? rotSpeed : airRotSpeed;
        float targetAngVelocity = forwardAngVelocity * Mathf.Sign(inputs.Turn) + (oppositeAngVelocity - forwardAngVelocity) * (inputs.Turn / 180);
        m_angVelocity = Mathf.Clamp(Mathf.MoveTowards(m_angVelocity, targetAngVelocity, maxTorque * Time.deltaTime), -maxTurnSpeed, maxTurnSpeed);

        float deltaRotation = m_angVelocity * Time.deltaTime;
        if (Mathf.Abs(inputs.Turn) < Mathf.Abs(deltaRotation))
        {
            deltaRotation = inputs.Turn;
            m_angVelocity = 0;
        }

        transform.Rotate(0, deltaRotation, 0, Space.Self);
        if (Controls.IsDown(GameButton.Testing))
        {
            Debug.Log(toLog);
        }
        //Debug.Log(toLog);
        //m_collisionFlags = m_controller.Move(1 * Vector3.up);
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