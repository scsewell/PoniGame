using UnityEngine;
using System.Collections;

public class TSAnimation : MonoBehaviour 
{
    [Tooltip("The bearing between the Camera and character forward directions at which the head is at maximum rotation")]
    [Range(10.0f, 90.0f)]
    public float headHorizontalAng = 40.0f;

    [Tooltip("The bearing between the Camera and character forward directions at which the head looks at the camera")]
    [Range(90.0f, 170.0f)]
    public float headReverseAng = 120.0f;

    [Tooltip("How fast the head will face the camera direction")]
    [Range(0.5f, 16.0f)]
    public float headRotateSpeed = 4.0f;

    private Animator m_animator;
    private TSMovement m_movement;
    private float m_horizontalLook = 0;

	// Use this for initialization
	void Start () 
    {
		m_animator = GetComponent<Animator>();
        m_movement = GetComponent<TSMovement>();
    }
	
	// Update is called once per frame
	void LateUpdate ()
    {
        //float runFactor = Mathf.Max(m_movement.ForwardSpeed - m_movement.walkSpeed, 0) / (m_movement.runSpeed - m_movement.walkSpeed);

        float lookBearing = m_movement.GetBearing(transform.forward, Camera.main.transform.forward);
        float targetBearing = Mathf.Abs(lookBearing) < headReverseAng ? lookBearing : -Mathf.Sign(lookBearing) * (180 - Mathf.Abs(lookBearing));
        float horizontalTarget = Mathf.Clamp(targetBearing / headHorizontalAng, -1, 1);
        m_horizontalLook = Mathf.Lerp(m_horizontalLook, horizontalTarget, Time.deltaTime * headRotateSpeed);

        m_animator.SetFloat("Forward", m_movement.ForwardSpeed);
        m_animator.SetFloat("LookHorizontal", m_horizontalLook);
        m_animator.SetBool("MidAir" , !GetComponent<CharacterController>().isGrounded);
    }
}