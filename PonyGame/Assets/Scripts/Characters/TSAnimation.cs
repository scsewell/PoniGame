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

    [Tooltip("Walk sound volume")]
    [Range(0.0f, 1.0f)]
    public float walkVolume = 0.5f;

    [Tooltip("Run sound volume")]
    [Range(0.0f, 1.0f)]
    public float runVolume = 0.5f;

    public AudioSource m_soundTrot;
    public AudioSource m_soundGallop;

    private Animator m_animator;
    private TSMovement m_movement;
    private CameraRig m_camRig;
    private float m_lookH = 0;
    private float m_lookV = 0;

    // Use this for initialization
    void Start () 
    {
		m_animator = GetComponent<Animator>();
        m_movement = GetComponent<TSMovement>();
        m_camRig = FindObjectOfType<CameraRig>();
    }
	
	// Update is called once per frame
	void LateUpdate ()
    {
        bool isPlayer = gameObject.tag == "Player";

        float lookBearingH = m_movement.GetBearing(transform.forward, Camera.main.transform.forward);
        bool faceCamera = Mathf.Abs(lookBearingH) > headReverseAng;
        float targetBearingH = faceCamera ? -Mathf.Sign(lookBearingH) * (180 - Mathf.Abs(lookBearingH)) : lookBearingH;
        float targetH = Mathf.Clamp(targetBearingH / headHorizontalAng, -1, 1);
        
        float lookBearingV = Mathf.DeltaAngle(0, (Quaternion.Inverse(transform.rotation) * m_camRig.pivot.rotation).eulerAngles.x);
        float targetBearingV = faceCamera ? lookBearingV + 20 : -lookBearingV;
        float targetV = Mathf.Clamp(targetBearingV / 40, -1, 1);

        m_lookH = Mathf.Lerp(m_lookH, isPlayer ? targetH : 0, Time.deltaTime * headRotateSpeed);
        m_lookV = Mathf.Lerp(m_lookV, isPlayer ? targetV : 0, Time.deltaTime * headRotateSpeed);

        m_animator.SetFloat("Forward", m_movement.ForwardSpeed);
        m_animator.SetFloat("LookHorizontal", m_lookH);
        m_animator.SetFloat("LookVertical", m_lookV);
        m_animator.SetBool("MidAir" , !GetComponent<CharacterController>().isGrounded);

        // audio
        float runFactor = Mathf.Max(m_movement.ForwardSpeed - m_movement.walkSpeed, 0) / (m_movement.runSpeed - m_movement.walkSpeed);
        bool grounded = GetComponent<CharacterController>().isGrounded;

        m_soundTrot.volume = grounded ? (Mathf.Min(m_movement.ForwardSpeed / m_movement.walkSpeed, 1) - runFactor) * walkVolume : 0;
        m_soundGallop.volume = grounded ? runFactor * runVolume : 0;

        if (m_soundTrot.volume < 0.001f) {
            m_soundTrot.Stop();
        } else if (!m_soundTrot.isPlaying) {
            m_soundTrot.Play();
        }

        if (m_soundGallop.volume < 0.001f) {
            m_soundGallop.Stop();
        } else if (!m_soundGallop.isPlaying) {
            m_soundGallop.Play();
        }
    }
}