using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class TSAnimation : MonoBehaviour 
{
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

    [SerializeField] private SkinnedMeshRenderer m_bodyMesh;
    [SerializeField] private SkinnedMeshRenderer m_upperEyelashesMesh;
    [SerializeField] private SkinnedMeshRenderer m_lowerEyelashesMesh;
    [SerializeField] private SkinnedMeshRenderer m_mouthMesh;
    [SerializeField] private Transform m_headBone;
    [SerializeField] private Transform m_chestBone;
    [SerializeField] private AudioSource frontLeftHoof;
    [SerializeField] private AudioSource frontRightHoof;
    [SerializeField] private AudioSource backLeftHoof;
    [SerializeField] private AudioSource backRightHoof;
    [SerializeField] private AudioClip[] hoofsteps;
    
    private TSMovement m_movement;
    private Animator m_animator;
    private CameraRig m_camRig;

    private float m_lookH = 0;
    private float m_lookV = 0;
    private float m_forwardSpeed = 0;
    private Quaternion m_lastHeadRot;
    private Quaternion m_lastHeadLocalRot;
    private bool m_lookAtCamera = false;
    private float m_currentBlinkTime = 0;
    private Dictionary<Transform, TransformData> m_basePose;
    private float m_deathTime = float.NegativeInfinity;
    private bool m_basePoseApplied = false;
    private float m_blinkWeight = 0;
    private float m_mouthOpenWeight = 0;
    private float m_frownWeight = 0;
    private bool m_flinch = false;
    private int m_flinchVariation = 0;


    void Start()
    {
        m_movement = GetComponent<TSMovement>();
        m_animator = GetComponent<Animator>();
        m_camRig = FindObjectOfType<CameraRig>();

        InterpolatedFloat speed = new InterpolatedFloat(() => (m_forwardSpeed), (val) => { m_forwardSpeed = val; });
        gameObject.AddComponent<FloatInterpolator>().Initialize(speed);

        SetRagdoll(false);
        StoreBasePose();
        m_lastHeadRot = m_headBone.rotation;
        m_lastHeadLocalRot = m_headBone.localRotation;
    }

    public void OnDie()
    {
        SetRagdoll(true);
        m_deathTime = Time.time;
    }

    public void Flinch()
    {
        if (m_movement.IsGrounded)
        {
            m_currentBlinkTime += Time.deltaTime;
            m_flinch = true;
            m_flinchVariation = (m_flinchVariation + 1) % 2;
            m_animator.SetInteger("FlinchVariation", m_flinchVariation);
        }
    }

    public void FixedUpdate()
    {
        m_forwardSpeed = m_movement.AttemptedSpeed;
    }

    public void PreAnimationUpdate(bool isPlayer)
    {
        m_animator.enabled = true;
        
        float targetH = 0;
        float targetV = 0;

        if (isPlayer)
        {
            float lookBearingH = Utils.GetBearing(transform.forward, Camera.main.transform.forward, Vector3.up);

            m_lookAtCamera = Mathf.Abs(Utils.GetBearing(transform.forward, Camera.main.transform.forward, transform.up)) > (m_lookAtCamera ? headLookEndAng : headLookBeginAng) &&
                                Vector3.Dot((Camera.main.transform.position - transform.position).normalized, transform.forward) > Mathf.Cos(Mathf.Deg2Rad * (m_lookAtCamera ? 75 : 70));

            float targetBearingH = m_lookAtCamera ? -Mathf.Sign(lookBearingH) * (180 - Mathf.Abs(lookBearingH)) : lookBearingH;
            targetH = Mathf.Clamp(targetBearingH / headHorizontalAng, -1, 1);

            float lookBearingV = Mathf.DeltaAngle(0, (Quaternion.Inverse(transform.rotation) * m_camRig.pivot.rotation).eulerAngles.x);
            float targetBearingV = m_lookAtCamera ? lookBearingV + 20 : -lookBearingV;
            targetV = Mathf.Clamp(targetBearingV / 40, -1, 1);
        }
        else
        {
            m_lookAtCamera = false;
        }

        float lerpFac = headRotateSpeed * Time.deltaTime;
        m_lookH = Mathf.Lerp(m_lookH, targetH, lerpFac);
        m_lookV = Mathf.Lerp(m_lookV, targetV, lerpFac);

        m_animator.SetFloat("Forward", m_forwardSpeed);
        m_animator.SetFloat("LookHorizontal", m_lookH);
        m_animator.SetFloat("LookVertical", m_lookV);
        m_animator.SetBool("MidAir", !m_movement.IsGrounded);
        m_animator.SetBool("Flinch", m_flinch);

        if (m_flinch && IsFlinching())
        {
            m_flinch = false;
        }
    }

    public void PostAnimationUpdate(Health health)
    {
        float frownTarget;
        float mouthOpenTarget;

        if (health.IsAlive)
        {
            // head camera tracking
            if (m_lookAtCamera && !IsFlinching())
            {
                Vector3 disp = Camera.main.transform.position - m_headBone.position;
                Quaternion lookDir = Quaternion.LookRotation(disp, transform.up);
                float vertLookAng = 90 - Vector3.Angle(lookDir * Vector3.forward, transform.up);
                float verticalSoftening = Mathf.Max(Mathf.Abs(vertLookAng) - headVerticalAngSoft, 0) * headVerticalSoftFactor;
                Quaternion verticalSoftened = lookDir * Quaternion.AngleAxis(verticalSoftening, Vector3.right);
                Quaternion targetRot = verticalSoftened * Quaternion.AngleAxis(30.0f, Vector3.right);
                
                m_headBone.rotation = Quaternion.Slerp(m_lastHeadRot, targetRot, headRotateSpeed * Time.deltaTime);
            }
            m_lastHeadRot = m_headBone.rotation;

            // blinking
            float lookAngVelocity = Quaternion.Angle(m_lastHeadLocalRot, m_headBone.localRotation) / Time.deltaTime;
            m_lastHeadLocalRot = m_headBone.localRotation;

            if (m_currentBlinkTime == 0 && (Random.value * blinkChance < Time.deltaTime || lookAngVelocity > blinkingMotionThreshold))
            {
                m_currentBlinkTime += Time.deltaTime;
            }

            if (m_currentBlinkTime > 0)
            {
                m_blinkWeight = blinkCurve.Evaluate(m_currentBlinkTime * blinkSpeedScale);
                m_currentBlinkTime += Time.deltaTime;
                if (m_currentBlinkTime > blinkTime)
                {
                    m_currentBlinkTime = 0;
                }
            }

            frownTarget = IsFlinching() ? 1.0f : Mathf.Clamp01(Mathf.Pow(1 - health.HealthFraction, 3));
            mouthOpenTarget = IsFlinching() ? 0.5f : 0;
        }
        else
        {
            m_animator.enabled = false;
            m_blinkWeight = (Mathf.Lerp(m_blinkWeight, 1, 1.5f * Time.deltaTime));
            frownTarget = 0.5f;
            mouthOpenTarget = 0.2f;

            if (!m_basePoseApplied)
            {
                float lerpFac = (Time.time - m_deathTime);
                if (lerpFac > 1)
                {
                    lerpFac = 1;
                    m_basePoseApplied = true;
                }
                foreach (Transform child in m_basePose.Keys)
                {
                    TransformData.Apply(TransformData.Interpolate(child, m_basePose[child], lerpFac), child);
                }
            }
        }
        
        // smoothing shape keys
        m_frownWeight = Mathf.Lerp(m_frownWeight, frownTarget, 4.0f * Time.deltaTime);
        m_mouthOpenWeight = Mathf.Lerp(m_mouthOpenWeight, mouthOpenTarget, 12.0f * Time.deltaTime);

        // blink shape keys
        float blinkWeight = Mathf.Clamp01(m_blinkWeight) * 100;
        m_bodyMesh.SetBlendShapeWeight(0, blinkWeight);
        m_upperEyelashesMesh.SetBlendShapeWeight(0, blinkWeight);
        m_lowerEyelashesMesh.SetBlendShapeWeight(0, blinkWeight);

        // mouth open shape keys
        float mouthOpenWeight = Mathf.Clamp01(m_mouthOpenWeight) * 100;
        m_bodyMesh.SetBlendShapeWeight(1, mouthOpenWeight);
        m_mouthMesh.SetBlendShapeWeight(0, mouthOpenWeight);
        
        // frown shape keys
        float frownWeight = Mathf.Clamp01(m_frownWeight) * 100;
        m_bodyMesh.SetBlendShapeWeight(2, frownWeight);
    }

    private bool IsFlinching()
    {
        if (!m_animator)
        {
            return false;
        }
        return m_animator.GetCurrentAnimatorStateInfo(0).IsTag("Flinch") || m_animator.GetNextAnimatorStateInfo(0).IsTag("Flinch");
    }

    private void StoreBasePose()
    {
        m_basePose = new Dictionary<Transform, TransformData>();
        foreach (Transform child in GetComponentsInChildren<Transform>())
        {
            if (!child.GetComponent<Rigidbody>() && (child.name.Contains("DEF_") || child.name.Contains("CON_")))
            {
                m_basePose.Add(child, new TransformData(child));
            }
        }
    }

    private void SetRagdoll(bool activated)
    {
        foreach (Rigidbody rigidbody in GetComponentsInChildren<Rigidbody>())
        {
            rigidbody.isKinematic = !activated;
            if (activated)
            {
                rigidbody.velocity = m_movement.ActualVelocity;
                if (rigidbody.transform == m_chestBone && m_movement.ActualVelocity.magnitude < 1.0f)
                {
                    rigidbody.velocity += 1.5f * m_chestBone.right * (Random.value > 0.5f ? 1 : -1);
                }
                if (!rigidbody.GetComponent<TransformInterpolator>())
                {
                    TransformInterpolator interpolator = rigidbody.gameObject.AddComponent<TransformInterpolator>();
                    interpolator.SetThresholds(true, 0.005f, 0.5f, 0.01f);
                }
            }
        }
        foreach (Collider collider in GetComponentsInChildren<Collider>())
        {
            if (collider.gameObject != gameObject)
            {
                collider.enabled = activated;
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