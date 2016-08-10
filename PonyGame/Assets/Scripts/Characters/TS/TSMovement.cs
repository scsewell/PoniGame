using UnityEngine;
using System.Collections;

/*
 * Responsible for moving the player character
 */
public class TSMovement : MonoBehaviour
{
    public bool debugView = true;

    [Tooltip("How fast the character may walk (Units / Second)")]
    [Range(0, 2.0f)]
    public float walkSpeed = 1.0f;

    [Tooltip("How fast the character may run (Units / Second)")]
    [Range(0, 4.0f)]
    public float runSpeed = 2.0f;
    
    [Tooltip("How fast the character accelerates forward (Units / Second^2)")]
    [Range(0, 10.0f)]
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
    [Range(0.0f, 6.0f)]
    public float jumpSpeed = 1.0f;

    [Tooltip("Fraction of the world's gravity is applied when in the air")]
    [Range(0.25f, 2.5f)]
    public float gravityFraction = 1.0f;

    [Tooltip("The number of raycasts done in a circle around the character to get an average ground normal")]
    [Range(0, 21)]
    public int normalSamples = 4;

    [Tooltip("The radius of the raycast circle around the character to get an average ground normal (Units)")]
    [Range(0.0f, 1.0f)]
    public float groundSmoothRadius = 0.1f;

    [Tooltip("The higher this value, the faster the character will align to the ground normal when on terrain")]
    [Range(1.0f, 24.0f)]
    public float groundAlignSpeed = 10.0f;

    [Tooltip("The higher this value, the faster the character will align upwards when midair")]
    [Range(0.25f, 24.0f)]
    public float airAlignSpeed = 1.5f;

    [Tooltip("Higher values allow the character to push rigidbodies faster")]
    [Range(0.0f, 5.0f)]
    public float pushStrength = 0.5f;

    private Health m_health;
    private TSAI m_AI;
    private CharacterController m_controller;
    private TransformInterpolator m_transformInterpolator;
    private CollisionFlags m_collisionFlags;
    private float m_forwardVelocity = 0;
    private float m_angVelocity = 0;
    private float m_velocityY = 0;
    private bool m_walk = false;
    
    private Vector3 m_lastVelocity = Vector3.zero;
    public Vector3 Velocity
    {
        get { return m_lastVelocity; }
    }

    void Start()
    {
        m_health = GetComponent<Health>();
        m_AI = GetComponent<TSAI>();
        m_controller = GetComponent<CharacterController>();
        m_transformInterpolator = GetComponent<TransformInterpolator>();

        m_transformInterpolator.ForgetPreviousValues();
        GameController.AddCharacter(transform);
    }

    void OnDestroy()
    {
        GameController.RemoveCharacter(transform);
    }
	
    /*
     * Executes the player's or AI's commands
     */
	void FixedUpdate()
    {
        m_health.ApplyDamage(25 * Time.deltaTime);
        if (!m_health.IsAlive)
        {
            m_controller.enabled = false;
            return;
        }

        if (tag == "Player")
        {
            MoveInputs inputs = new MoveInputs();

            float x = Controls.AverageValue(GameAxis.MoveX);
            float y = Controls.AverageValue(GameAxis.MoveY);
            Vector3 raw = Vector3.ClampMagnitude(new Vector3(x, 0, y), 1);

            if (raw.magnitude > 0)
            {
                inputs.turn = Utils.GetBearing(transform.forward, Camera.main.transform.rotation * raw, Vector3.up);
                inputs.forward = 1;
            }

            m_walk = Controls.JustDown(GameButton.WalkToggle) ? !m_walk : m_walk;
            
            inputs.run = raw.magnitude > 0.75f && m_walk == Controls.IsDown(GameButton.Walk);
            inputs.jump = Controls.JustDown(GameButton.Jump);

            ExecuteMovement(inputs);
        }
        else
        {
            ExecuteMovement(m_AI.GetMovement());
        }
    }

    /*
     * Moves the character based on the provided input
     */
    private void ExecuteMovement(MoveInputs inputs)
    {
        // linearly accelerate towards some target velocity
        m_forwardVelocity = Mathf.MoveTowards(m_forwardVelocity, inputs.forward * (inputs.run ? runSpeed : walkSpeed), acceleration * Time.deltaTime);
        Vector3 moveVelocity = transform.forward * m_forwardVelocity;
        m_lastVelocity = moveVelocity;

        if (m_controller.isGrounded)
        {
            // align the character to the ground being stood on
            Vector3 normal = GetGroundNormal(normalSamples, groundSmoothRadius);
            Quaternion targetRot = Quaternion.LookRotation(Vector3.Cross(transform.right, normal), normal);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * groundAlignSpeed);

            if (inputs.jump)
            {
                // jumping
                m_velocityY = jumpSpeed;
            }
            else
            {
                // keeps the character on the ground by applying a small downwards velocity that increases with the slope the character is standing on
                float slopeFactor = (1 - Mathf.Clamp01(Vector3.Dot(normal, Vector3.up)));
                m_velocityY = (-0.5f * slopeFactor + (1 - slopeFactor) * -0.01f) / Time.deltaTime;
            }
        }
        else
        {
            // slowly orient the character up in the air
            Vector3 normal = Vector3.up;
            Quaternion targetRot = Quaternion.LookRotation(Vector3.Cross(transform.right, normal), normal);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * airAlignSpeed);

            // apply downwards acceleration when the character is in the air
            m_velocityY += Physics.gravity.y * Time.deltaTime * gravityFraction;
        }
        
        Vector3 move = new Vector3(moveVelocity.x, m_velocityY, moveVelocity.z) * Time.deltaTime;
        m_collisionFlags = m_controller.Move(move);

        float maxTurnSpeed = m_controller.isGrounded ? rotSpeed : airRotSpeed;
        float targetAngVelocity = forwardAngVelocity * Mathf.Sign(inputs.turn) + (oppositeAngVelocity - forwardAngVelocity) * (inputs.turn / 180);
        m_angVelocity = Mathf.Clamp(Mathf.MoveTowards(m_angVelocity, targetAngVelocity, maxTorque * Time.deltaTime), -maxTurnSpeed, maxTurnSpeed);

        float deltaRotation = m_angVelocity * Time.deltaTime;
        if (Mathf.Abs(inputs.turn) < Mathf.Abs(deltaRotation))
        {
            deltaRotation = inputs.turn;
            m_angVelocity = 0;
        }

        transform.Rotate(0, deltaRotation, 0, Space.Self);
    }

    /*
     * Moves rigidbodies that are blocking the characters path and are moveable
     */
    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        Rigidbody body = hit.collider.attachedRigidbody;

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
    private Vector3 GetGroundNormal(int samples, float radius)
    {
        Vector3 normal = Vector3.zero;

        for (int i = 0; i < samples; i++)
        {
            Vector3 offset = Quaternion.Euler(0, i * (360.0f / samples), 0) * Vector3.forward * radius;
            Vector3 samplePos = transform.TransformPoint(offset + m_controller.center);
            Vector3 sampleDir = transform.TransformPoint(offset + Vector3.down * 0.05f);
           
            RaycastHit hit;
            if (Physics.Linecast(samplePos, sampleDir, out hit) && hit.transform.gameObject.layer == LayerMask.NameToLayer("Ground") && Vector3.Dot(hit.normal, Vector3.up) > 0.75f)
            {
                normal += hit.normal;

                if (debugView)
                {
                    Debug.DrawLine(samplePos, hit.point, Color.cyan);
                    Debug.DrawLine(hit.point, hit.point + hit.normal * 0.25f, Color.yellow);
                }
            }
        }

        if (normal != Vector3.zero)
        {
            if (debugView)
            {
                Debug.DrawLine(transform.position, transform.position + normal.normalized * 0.35f, Color.red);
            }
            return normal.normalized;
        }
        else
        {
            return Vector3.up;
        }
    }
}

public class MoveInputs
{
    public float    turn        = 0;
    public float    forward     = 0;
    public bool     run         = false;
    public bool     jump        = false;
}
