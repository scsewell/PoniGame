using UnityEngine;
using System.Collections;

public class TSAnimation : MonoBehaviour 
{
    public Transform headBone;

    [Tooltip("The bearing between the Camera and character forward directions at which the head is at maximum rotation")]
    [Range(10.0f, 90.0f)]
    public float headHorizontalAng = 60.0f;

    [Tooltip("The bearing between the Camera and character forward directions at which the head begins looking at the camera")]
    [Range(90.0f, 170.0f)]
    public float headLookBeginAng = 120.0f;

    [Tooltip("The bearing between the Camera and character forward directions at which the head stops looking at the camera")]
    [Range(90.0f, 170.0f)]
    public float headLookEndAng = 120.0f;

    [Tooltip("How fast the head will face the camera direction")]
    [Range(0.5f, 32.0f)]
    public float headRotateSpeed = 4.0f;

    [Tooltip("The softcap on the head's vertical look angle")]
    [Range(00.0f, 45.0f)]
    public float headVerticalAngSoft = 20.0f;

    [Tooltip("Proportionally how much the head is not rorated beyond the soft cap angle")]
    [Range(0, 1f)]
    public float headVerticalSoftFactor = 0.5f;

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
    private Quaternion m_lastHeadRot;
    private bool m_lookAtCamera = false;

    // Use this for initialization
    void Start() 
    {
		m_animator = GetComponent<Animator>();
        m_movement = GetComponent<TSMovement>();
        m_camRig = FindObjectOfType<CameraRig>();
    }
	
	// Update is called once per frame
	void Update()
    {
        bool isPlayer = gameObject.tag == "Player";

        float lookBearingH = Utils.GetBearing(transform.forward, Camera.main.transform.forward, Vector3.up);

        m_lookAtCamera = Mathf.Abs(lookBearingH) > (m_lookAtCamera ? headLookEndAng : headLookBeginAng);

        float targetBearingH = m_lookAtCamera ? -Mathf.Sign(lookBearingH) * (180 - Mathf.Abs(lookBearingH)) : lookBearingH;
        float targetH = Mathf.Clamp(targetBearingH / headHorizontalAng, -1, 1);
        
        float lookBearingV = Mathf.DeltaAngle(0, (Quaternion.Inverse(transform.rotation) * m_camRig.pivot.rotation).eulerAngles.x);
        float targetBearingV = m_lookAtCamera ? lookBearingV + 20 : -lookBearingV;
        float targetV = Mathf.Clamp(targetBearingV / 40, -1, 1);

        m_lookH = Mathf.Lerp(m_lookH, isPlayer ? targetH : 0, Time.deltaTime * headRotateSpeed);
        m_lookV = Mathf.Lerp(m_lookV, isPlayer ? targetV : 0, Time.deltaTime * headRotateSpeed);

        m_animator.SetFloat("Forward", m_movement.ForwardSpeed);
        m_animator.SetFloat("LookHorizontal", m_lookH);
        m_animator.SetFloat("LookVertical", m_lookV);
        m_animator.SetBool("MidAir", !GetComponent<CharacterController>().isGrounded);

        // audio
        float runFactor = Mathf.Max(m_movement.ForwardSpeed - m_movement.walkSpeed, 0) / (m_movement.runSpeed - m_movement.walkSpeed);
        bool grounded = GetComponent<CharacterController>().isGrounded;

        m_soundTrot.volume = grounded ? (Mathf.Min(m_movement.ForwardSpeed / m_movement.walkSpeed, 1) - runFactor) * walkVolume : 0;
        m_soundGallop.volume = grounded ? runFactor * runVolume : 0;

        // stop sounds with no volume
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

    void LateUpdate()
    {
        if (m_lookAtCamera)
        {
            Vector3 disp = Camera.main.transform.position - headBone.position;
            Quaternion lookDir = Quaternion.LookRotation(disp, transform.up);
            float vertLookAng = 90 - Vector3.Angle(lookDir * Vector3.forward, transform.up);
            float verticalSoftening = Mathf.Max(Mathf.Abs(vertLookAng) - headVerticalAngSoft, 0) * headVerticalSoftFactor;
            Quaternion verticalSoftened = lookDir * Quaternion.AngleAxis(verticalSoftening, Vector3.right);
            Quaternion targetRot = verticalSoftened * Quaternion.AngleAxis(30.0f, Vector3.right);

            headBone.rotation = Quaternion.Slerp(m_lastHeadRot, targetRot, Time.deltaTime * headRotateSpeed);
        }
        m_lastHeadRot = headBone.rotation;
    }
}