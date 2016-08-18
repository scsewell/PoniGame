using UnityEngine;
using System.Collections.Generic;

public class TSAI : MonoBehaviour
{
    [Tooltip("Should we attempt to go near the player.")]
    public bool followPlayer = true;

    [Tooltip("What is the farthest distance we should be from the player.")]
    [Range(0.2f, 5.0f)]
    public float followRadius = 3.0f;

    [Tooltip("Calculate a new path once the target has strayed this much from where the last path was calulated to.")]
    [Range(0.1f, 4.0f)]
    public float newPathTolerance = 4.0f;

    [Tooltip("Waypoints are considered reached within this distance.")]
    [Range(0.01f, 1.0f)]
    public float waypointTolerance = 0.5f;

    [Tooltip("Run towards the objective if it is futher then this.")]
    [Range(0.5f, 5.0f)]
    public float runDistance = 1.5f;
    

    private NavMeshAgent m_agent;

    private List<Vector3> m_path;
    private Transform m_player;
    private Vector3 m_destination;
    private bool m_run = false;


	void Start()
    {
        m_agent = GetComponent<NavMeshAgent>();

        GameController.CharacterChanged += SetPlayer;

        m_path = new List<Vector3>();
    }
    
    void OnDestroy()
    {
        GameController.CharacterChanged -= SetPlayer;
    }

    void SetPlayer(Transform player)
    {
        m_player = player;
    }

    public void UpdateAI()
    {
        // if we are following the player but don't have a path or the player has strayed from where we last generated at path to the player, find an updated path to the player
        if (followPlayer)
        {
            if (m_player && (m_path.Count == 0 || Vector3.Distance(m_player.position, m_path[m_path.Count - 1]) > newPathTolerance))
            {
                NavMeshPath path = new NavMeshPath();

                try
                {
                    m_agent.enabled = true;
                    m_agent.areaMask = 1; //...000001
                    m_agent.CalculatePath(m_player.position, path);
                    m_agent.enabled = false;
                }
                catch
                {
                    Debug.LogWarning("Attempt to find path to player failed...");
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
        }

        float targetDistance = 0;
        if (m_path.Count > 0)
        {
            targetDistance = Vector3.Distance(m_path[m_path.Count - 1], transform.position);
        }

        // if we don't have a target, have no path, or are at our targer, don't move
        if (!followPlayer || m_path.Count == 0 || targetDistance < followRadius)
        {
            m_destination = transform.position;
        }

        // run if the objective is far away and the next path segment is roughly aligned with this, preventing overshooting a turn
        m_run = targetDistance > runDistance;

        if (m_path.Count > 1)
        {
            Vector3 dir1 = (m_path[0] - transform.position).normalized;
            Vector3 dir2 = (m_path[1] - m_path[0]).normalized;
            m_run = Vector3.Dot(dir1, dir2) > 0.65f && Vector3.Distance(m_path[0], transform.position) > 0.75f;
        }
    }

    public MoveInputs GetInputs()
    {
        MoveInputs moveInput = new MoveInputs();
        
        Quaternion rot = Quaternion.LookRotation(Vector3.ProjectOnPlane(transform.forward, Vector3.up), Vector3.up);
        Vector3 disp = Quaternion.Inverse(rot) * (m_destination - transform.position);
        float bearing = 0;
        if (disp.sqrMagnitude > 0.001f)
        {
            bearing = Mathf.DeltaAngle(Quaternion.LookRotation(disp).eulerAngles.y, 0);
        }

        Debug.DrawLine(transform.position, m_destination);
        
        moveInput.Forward = disp.magnitude > 0.2f ? 1 : 0;
        moveInput.Turn = -bearing;
        moveInput.Run = m_run;
        moveInput.Jump = false;

        return moveInput;
    }

    public float GetTargetBearing()
    {
        return m_player != null ? Utils.GetBearing(transform.forward, m_player.position - transform.position, Vector3.up) : 0;
    }
}
