using UnityEngine;
using System.Collections;

public class Parasprite : MonoBehaviour
{
    [Tooltip("The layers that targetable gameobjects are in.")]
    public LayerMask targetLayers;

    [Tooltip("The radius within which players are detected and targeted.")]
    [Range(0f, 20f)]
    public float targetSearchRadius = 5.0f;

    [Tooltip("Layers with objects the parasprite should try to fly above.")]
    public LayerMask hoverLayers;

    [Tooltip("How high to hover above objects.")]
    [Range(0.01f, 2.0f)]
    public float hoverHeight;

    [Tooltip("The radius within which a target is considered reached.")]
    [Range(0f, 2.0f)]
    public float targetRadius = 1.0f;

    [Tooltip("How fast the Parasprite may move.")]
    [Range(0.1f, 5.0f)]
    public float speed;

    [Tooltip("How fast the Parasprite may accelerate.")]
    [Range(0.5f, 8.0f)]
    public float acceleration;

    private Animator m_animator;
    private Transform m_target;
    private float m_speed = 0;

    void Start ()
    {
       m_animator = GetComponent<Animator>();
    }
	
	void Update ()
    {
        // if we don't have a target, look for one
	    if (!m_target)
        {
            Collider[] targetable = Physics.OverlapSphere(transform.position, targetSearchRadius, targetLayers);

            if (targetable.Length > 0)
            {
                m_target = targetable[Random.Range(0, targetable.Length - 1)].transform;
            }
        }


        // if we have a target, move towards it
        if (m_target)
        {
            Vector3 disp = m_target.position - transform.position;

            Quaternion targetRot = Quaternion.LookRotation(Vector3.ProjectOnPlane(disp, Vector3.up));
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * 4.0f);

            bool inRange = disp.magnitude < targetRadius;

            // if we are far from our target move toward it
            m_speed = Mathf.MoveTowards(m_speed, inRange ? 0 : speed, acceleration * Time.deltaTime);
            Vector3 move = disp.normalized * m_speed * Time.deltaTime;

            RaycastHit hit;
            if (Physics.Raycast(move + transform.position, Vector3.down, out hit, 10.0f, hoverLayers))
            {
                move = (hit.point + Vector3.up * hoverHeight - transform.position).normalized * m_speed * Time.deltaTime;
            }

            transform.position += move;

            m_animator.SetBool("Attack", inRange);
        }
	}
}
