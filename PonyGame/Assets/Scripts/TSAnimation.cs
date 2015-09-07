using UnityEngine;
using System.Collections;

public class TSAnimation : MonoBehaviour 
{
    private Animator m_animator;
    private TSMovement m_movement;

	// Use this for initialization
	void Start () 
    {
		m_animator = GetComponent<Animator>();
        m_movement = GetComponent<TSMovement>();
    }
	
	// Update is called once per frame
	void LateUpdate ()
    {
        float runFactor = Mathf.Max(m_movement.ForwardSpeed - m_movement.walkSpeed, 0) / (m_movement.runSpeed - m_movement.walkSpeed);

        m_animator.SetFloat("Forward", m_movement.ForwardSpeed);
        m_animator.SetFloat("LookHorizontal", (m_movement.AngularlVelocity / m_movement.rotSpeed) * (1 - runFactor * 0.8f));
        m_animator.SetBool("MidAir" , !GetComponent<CharacterController>().isGrounded);
    }
}
