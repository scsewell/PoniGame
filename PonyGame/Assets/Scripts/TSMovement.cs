using UnityEngine;
using System.Collections;

public class TSMovement : MonoBehaviour
{
    public bool debugView = true;

    public float walkSpeed = 1.0f;
    public float runSpeed = 2.0f;
    public float strafeSpeed = 1.0f;
    public float acceleration = 5.0f;
    public float strafeAcceleration = 7.0f;
    public float mouseLookSpeed = 0.8f;
    public float rotSpeed = 120.0f;
    public float jumpSpeed = 0.03f;
    public float gravityFraction = 0.02f;
    public int normalSamples = 4;
    public float groundSmoothRadius = 0.1f;     // radius from ground contact point that the character will sample ground mesh normals to align along
    public float groundAlignSpeed = 10.0f;
    public float airAlignSpeed = 1.5f;

    private CollisionFlags m_CollisionFlags;
    private CharacterController m_controller;
    private Vector3 m_move = Vector3.zero;

    private float m_forwardVelocity = 0;
    public float ForwardSpeed
    {
        get { return m_forwardVelocity; }
    }

    private float m_strafeVelocity = 0;
    public float StrafeSpeed
    {
        get { return m_strafeVelocity; }
    }

    private float m_angularVelocity = 0;
    public float AngularlVelocity
    {
        get { return m_angularVelocity; }
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
        if (GetComponent<TSAI>())
        {
            ExecuteMovement(GetComponent<TSAI>().GetMovement());
        }
        else
        {
            MoveInputs inputs = new MoveInputs();

            // show and hide cursor as appropriate
            if (Input.GetMouseButtonDown(1))
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
            else if (Input.GetMouseButtonUp(1))
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }

            // if rmb held down, use mouse-view
            if (Input.GetMouseButton(1))
            {
                inputs.turn = Input.GetAxis("Mouse X") * mouseLookSpeed;
                inputs.strafe = Input.GetAxis("Horizontal");
            }
            else
            {
                inputs.turn = Input.GetAxis("Horizontal");
                inputs.strafe = 0;
            }
            inputs.forward = Input.GetAxis("Vertical");
            inputs.run = Input.GetKey(KeyCode.LeftShift) && Input.GetAxis("Vertical") > 0;
            inputs.jump = Input.GetButtonDown("Jump");

            ExecuteMovement(inputs);
        }
    }

    /*
     * Moves the character based on the provided input
     */
    private void ExecuteMovement(MoveInputs inputs)
    {
        m_forwardVelocity = Mathf.MoveTowards(m_forwardVelocity, inputs.forward * (inputs.run ? runSpeed : walkSpeed), acceleration * Time.deltaTime);
        m_strafeVelocity = Mathf.MoveTowards(m_strafeVelocity, inputs.strafe * strafeSpeed, strafeAcceleration * Time.deltaTime);
        Vector3 desiredMove = transform.forward * m_forwardVelocity * Time.deltaTime + transform.right * m_strafeVelocity * Time.deltaTime;

        m_angularVelocity = rotSpeed * inputs.turn;

        m_move = new Vector3(desiredMove.x, m_move.y, desiredMove.z);

        if (m_controller.isGrounded)
        {
            // align the character to the ground being stood on
            Vector3 normal = GetGroundNormal(normalSamples, groundSmoothRadius);
            Quaternion targetRot = Quaternion.LookRotation(Vector3.Cross(transform.right, normal), normal);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * groundAlignSpeed);

            // keeps the character on the ground by applying a small downwards velocity proportional to the slope the character is standing on
            float slopeFactor = (1 - Mathf.Clamp01(Vector3.Dot(normal, Vector3.up)));
            m_move.y = -0.2f * slopeFactor + (1 - slopeFactor) * -0.01f;

            // jumping
            if (inputs.jump)
            {
                m_move.y = jumpSpeed;
            }
        }
        else
        {
            // slowly orient the character up in the air
            Vector3 normal = Vector3.up;
            Quaternion targetRot = Quaternion.LookRotation(Vector3.Cross(transform.right, normal), normal);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * airAlignSpeed);

            // gravity when character is in the air
            m_move += Physics.gravity * gravityFraction * Time.deltaTime;
        }

        m_CollisionFlags = m_controller.Move(m_move);
        transform.Rotate(0, m_angularVelocity * Time.deltaTime, 0, Space.Self);
    }


    /*
     * Moves rigidbodies that are blocking the characters path and are moveable
     */
    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        Rigidbody body = hit.collider.attachedRigidbody;

        //dont move the rigidbody if the character is on top of it
        if (m_CollisionFlags == CollisionFlags.Below)
        {
            return;
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
    public float    strafe      = 0;
    public bool     run         = false;
    public bool     jump        = false;
}
