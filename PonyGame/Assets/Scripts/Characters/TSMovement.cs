using UnityEngine;
using System.Collections;
using InControl;

/*
 * Responsible for moving the player character
 */
public class TSMovement : MonoBehaviour
{
    public bool debugView = true;

    [Tooltip("How fast the character may walk (Units / Second)")]
    [Range(0.5f, 2.0f)]
    public float walkSpeed = 1.0f;

    [Tooltip("How fast the character may run (Units / Second)")]
    [Range(1.0f, 4.0f)]
    public float runSpeed = 2.0f;

    [Tooltip("How fast the character accelerates forward (Units / Second^2)")]
    [Range(1.0f, 10.0f)]
    public float acceleration = 5.0f;

    [Tooltip("How fast the character may rotate (Degrees / Second)")]
    [Range(60, 720)]
    public float rotSpeed = 120.0f;

    [Tooltip("How fast the character begins moving on jumping (Units / Second)")]
    [Range(0.5f, 3.0f)]
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


    private CollisionFlags m_CollisionFlags;
    private CharacterController m_controller;
    private float m_forwardVelocity = 0;
    private float m_angVelocity = 0;
    private float m_velocityY = 0;
    private bool m_run = false;

    public float ForwardSpeed
    {
        get { return m_forwardVelocity; }
    }


    void Start ()
    {
        m_controller = GetComponent<CharacterController>();
    }
	
    /*
     * Executes the player's or AI's commands
     */
	void Update ()
    {
        if (tag != "Player")
        {
            ExecuteMovement(GetComponent<TSAI>().GetMovement());
        }
        else
        {
            MoveInputs inputs = new MoveInputs();
            InputDevice device = InputManager.ActiveDevice;

            Vector3 move = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
            move += new Vector3(device.LeftStick.X, 0, device.LeftStick.Y);
            move = Vector3.ClampMagnitude(move, 1);

            if (move.magnitude > 0)
            {
                inputs.turn = GetBearing(transform.forward, Camera.main.transform.rotation * move);
                inputs.forward = move.magnitude;
            }

            m_run = Input.GetKeyDown(KeyCode.C) || device.LeftStickButton.WasPressed ? !m_run : m_run;
            inputs.run = Input.GetKey(KeyCode.LeftShift) || device.RightTrigger.State ? !m_run : m_run;
            inputs.jump = (Input.GetKey(KeyCode.Space) && !device.Action4.State) || device.Action3.State;

            ExecuteMovement(inputs);
        }
    }

    /*
     * Gets the bearing in degrees between two vectors as viewed from a certain direction
     */
    public float GetBearing(Vector3 dir1, Vector3 dir2)
    {
        float vec1 = Quaternion.LookRotation(Vector3.ProjectOnPlane(dir1, Vector3.up), Vector3.up).eulerAngles.y;
        float vec2 = Quaternion.LookRotation(Vector3.ProjectOnPlane(dir2, Vector3.up), Vector3.up).eulerAngles.y;
        return -Mathf.DeltaAngle(vec2, vec1);
    }


    /*
     * Moves the character based on the provided input
     */
    private void ExecuteMovement(MoveInputs inputs)
    {
        // linearly accelerate towards some target velocity
        m_forwardVelocity = Mathf.MoveTowards(m_forwardVelocity, inputs.forward * (inputs.run ? runSpeed : walkSpeed), acceleration * Time.deltaTime);
        Vector3 moveVelocity = transform.forward * m_forwardVelocity;

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
        m_CollisionFlags = m_controller.Move(move);

        float targetAngVelocity = Mathf.Clamp(inputs.turn, -rotSpeed * Time.deltaTime, rotSpeed * Time.deltaTime);
        m_angVelocity = Mathf.MoveTowards(m_angVelocity, targetAngVelocity, 20.0f * Time.deltaTime) * Mathf.Clamp01((Mathf.Abs(inputs.turn) + 25.0f) / 45.0f);
        bool willOvershoot = Mathf.Abs(inputs.turn) < Mathf.Abs(m_angVelocity);
        m_angVelocity = willOvershoot ? targetAngVelocity : m_angVelocity;
        transform.Rotate(0, m_angVelocity, 0, Space.Self);
    }


    /*
     * Moves rigidbodies that are blocking the characters path and are moveable
     */
    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        Rigidbody body = hit.collider.attachedRigidbody;

        // dont move the rigidbody if the character is on top of it
        if (m_CollisionFlags == CollisionFlags.Below)
        {
            return;
        }

        // if we are airborne and hit our head reverse our vertical velocity
        if ((m_CollisionFlags & CollisionFlags.Above) != 0 && !m_controller.isGrounded)
        {
            m_velocityY *= -0.5f;
        }

        if (body == null || body.isKinematic)
        {
            return;
        }

        Vector3 pushDir = new Vector3(hit.moveDirection.x, 0, hit.moveDirection.y);
        body.AddForceAtPosition(pushDir * 0.5f, hit.point, ForceMode.Impulse);
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
            Vector3 SamplePos = transform.TransformPoint(offset + m_controller.center);
            Vector3 SampleDir = transform.TransformPoint(offset + Vector3.down * 0.05f);
           
            RaycastHit hit;
            if (Physics.Linecast(SamplePos, SampleDir, out hit) && hit.transform.gameObject.layer == LayerMask.NameToLayer("Ground") && Vector3.Dot(hit.normal, Vector3.up) > 0.75f)
            {
                normal += hit.normal;

                if (debugView)
                {
                    Debug.DrawLine(SamplePos, hit.point, Color.cyan);
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
