using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TSAnimation : MonoBehaviour 
{
    public SkinnedMeshRenderer body;
    public SkinnedMeshRenderer upperEyelashes;
    public SkinnedMeshRenderer lowerEyelashes;
    public Transform headBone;
    public Transform chestBone;

    [Tooltip("The bearing between the Camera and character forward directions at which the head is at maximum rotation")]
    [Range(10, 90)]
    public float headHorizontalAng = 60.0f;

    [Tooltip("The bearing between the Camera and character forward directions at which the head begins looking at the camera")]
    [Range(90, 180)]
    public float headLookBeginAng = 120.0f;

    [Tooltip("The bearing between the Camera and character forward directions at which the head stops looking at the camera")]
    [Range(90, 180)]
    public float headLookEndAng = 120.0f;

    [Tooltip("How fast the head will face the camera direction")]
    [Range(0, 32)]
    public float headRotateSpeed = 4.0f;

    [Tooltip("The softcap on the head's vertical look angle")]
    [Range(0, 45)]
    public float headVerticalAngSoft = 20.0f;

    [Tooltip("Proportionally how much the head is not rorated beyond the soft cap angle")]
    [Range(0, 1)]
    public float headVerticalSoftFactor = 0.5f;
    
    public AnimationCurve blinkCurve;

    [Tooltip("Changes the speed of the blink animation")]
    [Range(0, 30)]
    public float blinkSpeedScale = 1.0f;

    [Tooltip("The length of the blinking animation (Seconds)")]
    [Range(0, 1)]
    public float blinkTime = 0.5f;

    [Tooltip("The average time between blinks (Seconds)")]
    [Range(0, 30)]
    public float blinkChance = 5.0f;

    [Tooltip("Always blink if the head is rotating faster than this speed (Degrees / Second)")]
    [Range(0, 360)]
    public float blinkingMotionThreshold = 40.0f;

    public AudioSource frontLeftHoof;
    public AudioSource frontRightHoof;
    public AudioSource backLeftHoof;
    public AudioSource backRightHoof;
    public AudioClip[] hoofsteps;
    
    private Animator m_animator;
    private TSMovement m_movement;
    private Health m_health;
    private CameraRig m_camRig;
    private float m_lookH = 0;
    private float m_lookV = 0;
    private Quaternion m_lastHeadRot;
    private bool m_lookAtCamera = false;
    private float m_currentBlinkTime = 0;
    private Dictionary<Transform, TransformData> m_basePose;
    
    void Start() 
    {
		m_animator = GetComponent<Animator>();
        m_movement = GetComponent<TSMovement>();
        m_health = GetComponent<Health>();
        m_camRig = FindObjectOfType<CameraRig>();

        m_health.OnDie += OnDie;

        SetRagdoll(false);

        m_basePose = new Dictionary<Transform, TransformData>();
        foreach (Transform child in GetComponentsInChildren<Transform>())
        {
            if (child != transform && !child.GetComponent<Rigidbody>())
            {
                m_basePose.Add(child, new TransformData(child.localPosition, child.localRotation, child.localScale));
            }
        }
    }

    void OnDestroy()
    {
        m_health.OnDie -= OnDie;
    }
    
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

        m_animator.SetFloat("Forward", m_movement.Velocity.magnitude);
        m_animator.SetFloat("LookHorizontal", m_lookH);
        m_animator.SetFloat("LookVertical", m_lookV);
        m_animator.SetBool("MidAir", !GetComponent<CharacterController>().isGrounded);
    }

    void LateUpdate()
    {
        if (!m_health.IsAlive)
        {
            m_animator.enabled = false;
            SetBlink(Mathf.Lerp(GetBlink(), 1, 1.5f * Time.deltaTime));

            foreach (Transform child in m_basePose.Keys)
            {
                TransformData basePose = m_basePose[child];
                child.localPosition = Vector3.Lerp(child.localPosition, basePose.position, 4.0f * Time.deltaTime);
                child.localRotation = Quaternion.Slerp(child.localRotation, basePose.rotation, 4.0f * Time.deltaTime);
                child.localScale = Vector3.Lerp(child.localScale, basePose.scale, 4.0f * Time.deltaTime);
            }
            return;
        }

        // head camera tracking
        float lookAngVelocity = 0;
        if (m_lookAtCamera)
        {
            Vector3 disp = Camera.main.transform.position - headBone.position;
            Quaternion lookDir = Quaternion.LookRotation(disp, transform.up);
            float vertLookAng = 90 - Vector3.Angle(lookDir * Vector3.forward, transform.up);
            float verticalSoftening = Mathf.Max(Mathf.Abs(vertLookAng) - headVerticalAngSoft, 0) * headVerticalSoftFactor;
            Quaternion verticalSoftened = lookDir * Quaternion.AngleAxis(verticalSoftening, Vector3.right);
            Quaternion targetRot = verticalSoftened * Quaternion.AngleAxis(30.0f, Vector3.right);

            Quaternion newRot = Quaternion.Slerp(m_lastHeadRot, targetRot, Time.deltaTime * headRotateSpeed);
            headBone.rotation = newRot;
            lookAngVelocity = Quaternion.Angle(newRot, m_lastHeadRot) / Time.deltaTime;
        }
        m_lastHeadRot = headBone.rotation;

        // blinking
        if (m_currentBlinkTime == 0 && (Random.value * blinkChance < Time.deltaTime || lookAngVelocity > blinkingMotionThreshold))
        {
            m_currentBlinkTime += Time.deltaTime;
        }

        if (m_currentBlinkTime > 0)
        {
            SetBlink(blinkCurve.Evaluate(m_currentBlinkTime * blinkSpeedScale));
            m_currentBlinkTime += Time.deltaTime;
            if (m_currentBlinkTime > blinkTime)
            {
                m_currentBlinkTime = 0;
            }
        }
    }

    private void SetBlink(float weight)
    {
        float scaledWeight = Mathf.Clamp01(weight) * 100;
        body.SetBlendShapeWeight(0, scaledWeight);
        upperEyelashes.SetBlendShapeWeight(0, scaledWeight);
        lowerEyelashes.SetBlendShapeWeight(0, scaledWeight);
    }

    private float GetBlink()
    {
        return Mathf.Clamp01(body.GetBlendShapeWeight(0) / 100);
    }

    private void OnDie()
    {
        SetRagdoll(true);
    }

    private void SetRagdoll(bool activated)
    {
        foreach (Rigidbody body in GetComponentsInChildren<Rigidbody>())
        {
            if (body.gameObject != gameObject)
            {
                body.isKinematic = !activated;
                if (activated)
                {
                    body.velocity = m_movement.Velocity;
                }
            }
        }
    }

    public void FrontLeftHoofstep()
    {
        PlayHoofstep(frontLeftHoof);
    }
    public void FrontRightHoofstep()
    {
        PlayHoofstep(frontRightHoof);
    }
    public void BackLeftHoofstep()
    {
        PlayHoofstep(backLeftHoof);
    }
    public void BackRightHoofstep()
    {
        PlayHoofstep(backRightHoof);
    }

    private void PlayHoofstep(AudioSource source)
    {
        source.clip = hoofsteps[Random.Range(0, hoofsteps.Length)];
        source.pitch = 0.95f + 0.1f * Random.value;
        source.Play();
    }
}