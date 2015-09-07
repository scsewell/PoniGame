using UnityEngine;
using System.Collections;

public class TSAI : MonoBehaviour
{
    public float maxDesinationX = 9;
    public float jumpChance = 0.05f;
    public float newDestChance = 0.05f;
    public float newSpeedChance = 0.05f;
    
    private Vector3 m_destination = Vector3.zero;
    private MoveInputs m_moveInput;
    private bool m_run = false;


	void Start ()
    {
        m_moveInput = new MoveInputs();

        PickNewDestination();
        m_run = Random.Range(0.0f, 1.0f) < 0.35f;
    }
	
	void Update ()
    {
        if (Random.Range(0.0f, 1.0f) < newDestChance * Time.deltaTime)
        {
            PickNewDestination();
        }

        if (Random.Range(0.0f, 1.0f) < newSpeedChance * Time.deltaTime)
        {
            m_moveInput.run = !m_run;
        }
    }

    public MoveInputs GetMovement()
    {
        Vector3 pos = new Vector3(transform.position.x, 0, transform.position.z);
        Quaternion rot = Quaternion.LookRotation(Vector3.ProjectOnPlane(transform.forward, Vector3.up), Vector3.up);
        Vector3 disp = Quaternion.Inverse(rot) * (m_destination - pos);
        float bearing = Mathf.DeltaAngle(Quaternion.LookRotation(disp).eulerAngles.y, 0);

        //Debug.DrawLine(transform.position, m_destination);

        float turnTarget = Mathf.Abs(bearing) > 10 ? -Mathf.Clamp(bearing, -1, 1) : 0;
        m_moveInput.turn = Mathf.MoveTowards(m_moveInput.turn, turnTarget, Time.deltaTime * 4);

        m_moveInput.forward = disp.magnitude > 0.5f ? 1 : 0;
        m_moveInput.jump = Random.Range(0.0f, 1.0f) < jumpChance * Time.deltaTime;

        return m_moveInput;
    }

    private void PickNewDestination()
    {
        m_destination = new Vector3(Random.Range(-maxDesinationX, maxDesinationX), 0, Random.Range(-maxDesinationX, maxDesinationX));
    }
}
