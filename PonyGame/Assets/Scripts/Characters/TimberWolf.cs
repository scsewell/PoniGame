using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TimberWolf : MonoBehaviour
{
    public bool debugView = true;

    [Tooltip("The layers that targetable gameobjects are in.")]
    public LayerMask targetLayers;

    [Tooltip("The number of seconds likely to pass before moving to a nearby random position when without a target.")]
    [Range(0, 60)]
    public float idleMoveChange = 10.0f;

    [Tooltip("The max distance a wolf may move from it's spawn while it has no target.")]
    [Range(0, 10)]
    public float idleMoveRadius = 4.0f;

    [Tooltip("The radius within which players are detected and targeted.")]
    [Range(0f, 20f)]
    public float targetSearchRadius = 5.0f;

    [Tooltip("The radius within which a target is considered reached.")]
    [Range(0f, 2.0f)]
    public float targetRadius = 1.0f;

    [Tooltip("Calculate a new path once the target has strayed this much from where the last path was calulated to.")]
    [Range(0.1f, 4.0f)]
    public float newPathTolerance = 4.0f;

    [Tooltip("Waypoints are considered reached within this distance.")]
    [Range(0.01f, 1.0f)]
    public float waypointTolerance = 0.5f;
    
    [Tooltip("How fast the character may walk (Units / Second)")]
    [Range(0.25f, 2.0f)]
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

    [Tooltip("The radius within which the Timberwolf will stationary attack.")]
    [Range(0f, 2.0f)]
    public float closeAttackRadius = 1.0f;

    [Tooltip("The radius within which the Timberwolf will leap attack.")]
    [Range(0f, 2.0f)]
    public float leapAttackRadius = 1.0f;

    [Tooltip("how many seconds must pass between attacks.")]
    [Range(0f, 2.0f)]
    public float attackRate = 1.0f;

    [Tooltip("How much damage is required in a single hit to stagger the Timberwolf.")]
    public float staggerDamage = 10.0f;

    [Tooltip("How long the Timberwolf remains staggered for after taking enough damage.")]
    [Range(0, 2)]
    public float staggerDuration = 1.0f;

    private CharacterController m_controller;
    private NavMeshAgent m_agent;
    private List<Vector3> m_path;
    private Transform m_target;
    private Vector3 m_destination;
    private Health m_health;
    private Animator m_animator;
    private CollisionFlags m_CollisionFlags;
    private float m_forwardVelocity = 0;
    private float m_angVelocity = 0;
    private float m_velocityY = 0;
    private bool m_staggered = false;
    private bool m_canAttack = true;
    private Vector3 m_idlePos;

    void Start ()
    {
        m_agent = GetComponent<NavMeshAgent>();
        m_controller = GetComponent<CharacterController>();
        m_path = new List<Vector3>();
        m_health = GetComponent<Health>();
        m_animator = GetComponent<Animator>();

        m_health.HealthChanged += HealthChanged;
        m_health.Die += Killed;

        foreach (Rigidbody body in GetComponentsInChildren<Rigidbody>())
        {
            body.isKinematic = true;
        }
        foreach (Collider collider in GetComponentsInChildren<Collider>())
        {
            if (collider.GetType() != typeof(CharacterController))
            {
                collider.isTrigger = true;
            }
        }

        m_idlePos = transform.position;
    }

    void OnDestroy()
    {
        m_health.HealthChanged -= HealthChanged;
        m_health.Die -= Killed;
    }

    // Update is called once per frame
    void Update ()
    {
        // if we don't have a target, look for one
        //if (!m_target)
        //{
            foreach (Transform target in GameController.GetTargetable())
            {
                float distance = Vector3.Distance(transform.position, target.position);

                if (distance < targetSearchRadius && (!m_target || distance < Vector3.Distance(transform.position, m_target.position)))
                {
                    m_target = target;
                }
            }
        //}

        // if we have a target, find a path to move towards it if the current one is out of date
        if ((m_target && (m_path.Count == 0 || Vector3.Distance(m_target.position, m_path[m_path.Count - 1]) > newPathTolerance)) || (!m_target && Random.Range(0, idleMoveChange) < Time.deltaTime))
        {
            NavMeshPath path = new NavMeshPath();

            try
            {
                m_agent.enabled = true;
                Vector3 destination;
                if (m_target)
                {
                    destination = m_target.position;
                }
                else
                {
                    destination = m_idlePos + Vector3.ProjectOnPlane(Random.insideUnitSphere * idleMoveRadius, Vector3.up);
                }
                m_agent.CalculatePath(destination, path);
                m_agent.enabled = false;
            }
            catch
            {
                Debug.LogWarning("Attempt to find path to target failed...");
            }

            m_path.Clear();

            foreach (Vector3 pos in path.corners)
            {
                m_path.Add(pos);
            }
        }

        // remove waypoints we have reached
        if (m_path.Count > 0 && Vector3.Distance(m_path[0], transform.position) < waypointTolerance)
        {
            m_path.RemoveAt(0);
        }

        // move to the next waypoint in the list if avaliable
        if (m_path.Count > 0)
        {
            m_destination = m_path[0];
        }

        // draw path to target
        if (debugView && m_path.Count > 0)
        {
            Debug.DrawLine(transform.position, m_path[0], Color.magenta);

            for (int i = 1; i < m_path.Count; i++)
            {
                float brightness = 1.0f * (float)i / m_path.Count;
                Debug.DrawLine(m_path[i - 1], m_path[i], new Color(brightness, brightness * 0.5f, brightness));
            }
        }

        //---------- movement -------------
        float targetDistance = 0;
        if (m_target)
        {
            targetDistance = Vector3.Distance(m_target.position, transform.position);
        }

        // if we don't have a target, have no path, or are at our targer, don't move
        if ((!m_target && m_path.Count == 0) || (m_target && targetDistance < targetRadius))
        {
            m_destination = transform.position;
        }

        Quaternion rot = Quaternion.LookRotation(Vector3.ProjectOnPlane(transform.forward, Vector3.up), Vector3.up);
        Vector3 disp = Quaternion.Inverse(rot) * (m_destination - transform.position);
        float bearing = 0;
        if (disp.sqrMagnitude > 0.001f)
        {
            bearing = Mathf.DeltaAngle(Quaternion.LookRotation(disp).eulerAngles.y, 0);
        }

        // linearly accelerate towards some target velocity
        float targetVelocity = !(m_staggered || disp.magnitude < 0.001f) ? (m_target ? runSpeed : walkSpeed) : 0;
        m_forwardVelocity = Mathf.MoveTowards(m_forwardVelocity, targetVelocity, acceleration * Time.deltaTime);
        Vector3 moveVelocity = transform.forward * m_forwardVelocity;
        m_animator.SetFloat("Movement", m_forwardVelocity);

        if (m_controller.isGrounded)
        {
            // align the character to the ground being stood on
            Vector3 normal = Utils.GetGroundNormal(transform, m_controller, normalSamples, groundSmoothRadius, debugView);
            Quaternion targetRot = Quaternion.LookRotation(Vector3.Cross(transform.right, normal), normal);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * groundAlignSpeed);
            
            // keeps the character on the ground by applying a small downwards velocity that increases with the slope the character is standing on
            float slopeFactor = (1 - Mathf.Clamp01(Vector3.Dot(normal, Vector3.up)));
            m_velocityY = (-0.5f * slopeFactor + (1 - slopeFactor) * -0.01f) / Time.deltaTime;
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

        // turn towards the target if we are close enough to not move
        if (m_target && (m_path.Count == 0 || targetDistance < targetRadius))
        {
            disp = Quaternion.Inverse(rot) * (m_target.position - transform.position);
            bearing = Mathf.DeltaAngle(Quaternion.LookRotation(disp).eulerAngles.y, 0);
        }

        float maxTurnSpeed = rotSpeed * Time.deltaTime;
        float targetAngVelocity = Mathf.Clamp(-bearing, -maxTurnSpeed, maxTurnSpeed);

        m_angVelocity = Mathf.MoveTowards(m_angVelocity, !m_staggered ? targetAngVelocity : 0, 20.0f * Time.deltaTime) * Mathf.Clamp01((Mathf.Abs(bearing) + 25.0f) / 45.0f);
        bool willOvershoot = Mathf.Abs(bearing) < Mathf.Abs(m_angVelocity);
        m_angVelocity = willOvershoot ? targetAngVelocity : m_angVelocity;

        transform.Rotate(0, m_angVelocity, 0, Space.Self);

        //---------- Attack ----------
        if (m_canAttack && !m_staggered && m_target && Mathf.Abs(Utils.GetBearing(transform.forward, m_target.position - transform.position)) < 10f && Vector3.Dot((m_target.position - transform.position).normalized, transform.forward) > 0.55f)
        {
            if (targetDistance < closeAttackRadius)
            {
                AttackForwardStationary();
            }
            else if (targetDistance < leapAttackRadius)
            {
                AttackForwardLeap();
            }
        }
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
        body.AddForceAtPosition(pushDir * 0.35f, hit.point, ForceMode.Impulse);
    }


    /*
     * Called when the ememy is killed, activating the rigidbodies
     */
    private void Killed()
    {
        if (m_animator)
        {
            Destroy(m_animator);
        }

        GetComponent<CharacterController>().enabled = false;

        foreach (Collider collider in GetComponentsInChildren<Collider>())
        {
            collider.isTrigger = false;
        }
        foreach (Rigidbody body in GetComponentsInChildren<Rigidbody>())
        {
            body.transform.SetParent(transform);
            body.isKinematic = false;
        }

        Destroy(this);
    }

    /*
     * Called when the ememy is hurt
     */
    private void HealthChanged(float healthChange)
    {
        if (healthChange < -staggerDamage)
        {
            StartCoroutine(Stagger());
        }
    }

    IEnumerator Stagger()
    {
        m_staggered = true;

        if (m_animator && m_animator.isInitialized)
        {
            m_animator.SetBool("Hurt", true);
        }

        yield return 0;

        if (m_animator && m_animator.isInitialized)
        {
            m_animator.SetBool("Hurt", false);
        }

        yield return new WaitForSeconds(staggerDuration);
        
        m_staggered = false;
    }


    /*
     * Does a leaping attack facing forward
     */
    private void AttackForwardLeap()
    {
        StartCoroutine(AttackLeap("AttackLeap"));
    }

    /*
     * Does a stationary attack facing forward
     */
    private void AttackForwardStationary()
    {
        StartCoroutine(AttackLeap("AttackShort"));
    }

    IEnumerator AttackLeap(string AttackAnimation)
    {
        m_canAttack = false;

        if (m_animator && m_animator.isInitialized)
        {
            m_animator.SetBool(AttackAnimation, true);
        }

        yield return 0;

        if (m_animator && m_animator.isInitialized)
        {
            m_animator.SetBool(AttackAnimation, false);
        }

        yield return new WaitForSeconds(attackRate);

        m_canAttack = true;
    }
}
