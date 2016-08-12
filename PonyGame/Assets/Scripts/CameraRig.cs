using UnityEngine;
using System.Collections;

public class CameraRig : MonoBehaviour
{
    public Transform posTarget;
    public Transform rotTarget;
    public Transform pivot;

    public LayerMask cameraBlockLayers;
    public LayerMask lineOfSightBlocking;

    [Tooltip("How fast the character may look around horizontally")]
    [Range(0, 20)]
    public float lookXSensitivity = 4.0f;

    [Tooltip("How fast the character may look around vertically")]
    [Range(0, 20)]
    public float lookYSensitivity = 2.0f;

    [Tooltip("How fast the camera zooms in and out")]
    [Range(0, 2)]
    public float scrollZoomSensitivity = 1.0f;

    [Tooltip("Max amount the character may look around horizontally (Degrees / Second)")]
    [Range(0, 30)]
    public float lookXRateCap = 10.0f;

    [Tooltip("Max amount the character may look around vertically in (Degrees / Second)")]
    [Range(0, 30)]
    public float lookYRateCap = 10.0f;

    [Tooltip("Max angle towards the upper pole the camera may be elevated (Degrees)")]
    [Range(0, 90)]
    public float maxElevation = 45.0f;

    [Tooltip("Max angle towards the lower pole the camera may be depressed (Degrees)")]
    [Range(-90, 0)]
    public float minElevation = -30;

    [Tooltip("How close the camera may be to its pivot (Units)")]
    [Range(0, 5)]
    public float minZoom = 1.0f;

    [Tooltip("How far the camera may be from its pivot (Units)")]
    [Range(0, 5)]
    public float maxZoom = 2.0f;
    
    [Tooltip("How much the camera height is lowered as the camera approaches the pivot")]
    [Range(0, 5)]
    public float zoomLowerHeight = 1.0f;

    [Tooltip("How quickly the camera changes zoom")]
    [Range(0, 16)]
    public float zoomSmoothing = 4.0f;

    [Tooltip("Camera collision radius (Units)")]
    [Range(0, 1)]
    public float radius = 0.1f;

    [Tooltip("The distance under which a target may be locked")]
    [Range(0, 50)]
    public float lockAquireRange = 10.0f;

    [Tooltip("The distance at which a target will be unlocked")]
    [Range(0, 50)]
    public float lockLoseRange = 10.0f;

    [Tooltip("True if objects with line of sight to the camera but are not in the fustrum are unlockable")]
    public bool onlyLockOnScreen = false;

    [Tooltip("Higher values require targets to be more central in the screen to be locked on to")]
    [Range(0, 1)]
    public float minimumLockCenterness = 0.8f;

    [Tooltip("The angle new lock targets must be within from the input direction to be considered")]
    [Range(0, 180)]
    public float lockChangeBearingMax = 50.0f;

    [Tooltip("The higher this value, the greater weighting to targets aligned with the select direction")]
    [Range(0, 10)]
    public float lockDirectioness = 2.0f;

    [Tooltip("How quickly the camera aims at the lock target")]
    [Range(0, 30)]
    public float lockSmoothing = 8.0f;
    
    [Tooltip("Angle offset to adjust the vertical position of the lock target on screen")]
    [Range(-90, 90)]
    public float lockAngleAdjust = 20.0f;
    
    [Tooltip("How quicly the camera will follow the dead character")]
    [SerializeField] [Range(0, 16)]
    private float m_deathSmoothing = 2.0f;

    [Tooltip("The offset to the elevation when the character is dead")]
    [SerializeField] [Range(-90, 90)]
    private float m_deathElevation = 15.0f;

    [Tooltip("How quickly the camera will orbit the dead character")]
    [SerializeField] [Range(-90, 90)]
    private float m_deathRotateSpeed = 20.0f;


    private Transform m_player;
    private Health m_playerHealth;
    private RagdollCamera m_playerRagdoll;
    private TransformInterpolator m_transformInterpolator;
    private TransformInterpolator m_pivotInterpolator;
    private Interpolator<float> m_zoomInterpolator;
    private float m_elevation;
    private float m_zoom;
    private float m_zoomTarget;
    private bool m_alreadyChangedLock = false;
    private Vector3 m_lastLookPos = Vector3.zero;
    private float m_lastRotateSpeed = 0;


    private Transform m_lockTarget;
    public Transform LockTarget
    {
        get { return m_lockTarget; }
    }

    void Start()
    {
        m_transformInterpolator = GetComponent<TransformInterpolator>();
        m_pivotInterpolator = pivot.GetComponent<TransformInterpolator>();

        m_zoomInterpolator = new Interpolator<float>(new InterpolatedFloat(() => (m_zoom), (val) => { m_zoom = val; }));
        gameObject.AddComponent<FloatInterpolator>().Initialize(m_zoomInterpolator);

        GameController.CharacterChanged += Initialize;
    }

    void OnDestroy()
    {
        GameController.CharacterChanged -= Initialize;
    }

    void Initialize(Transform player)
    {
        m_player = player;
        m_playerHealth = m_player.GetComponent<Health>();
        m_playerRagdoll = m_player.GetComponent<RagdollCamera>();

        m_lockTarget = null;

        transform.position = player.position;
        transform.rotation = Quaternion.LookRotation(Vector3.ProjectOnPlane(player.forward, Vector3.up), Vector3.up);
        Camera.main.transform.position = posTarget.position;
        Camera.main.transform.LookAt(rotTarget);

        m_elevation = 0;
        m_zoom = Vector3.Distance(rotTarget.position, posTarget.position);
        m_zoomTarget = Vector3.Distance(rotTarget.position, posTarget.position);

        m_transformInterpolator.ForgetPreviousValues();
        m_pivotInterpolator.ForgetPreviousValues();
        m_zoomInterpolator.ForgetPreviousValues();
    }
	
	void FixedUpdate() 
	{
        if (m_player && m_playerHealth.IsAlive)
        {
            transform.position = m_player.position;

            // aquire new lock
            if (Controls.JustDown(GameButton.Lock))
            {
                if (m_lockTarget)
                {
                    m_lockTarget = null;
                }
                else
                {
                    m_lockTarget = GetLockTarget();
                }
            }

            float rotateX = 0;
            if (MainUI.IsCursorLocked)
            {
                if (m_lockTarget == null)
                {
                    rotateX = Mathf.Clamp(Controls.AverageValue(GameAxis.LookX) * lookXSensitivity, -lookXRateCap, lookXRateCap);
                    m_elevation += Mathf.Clamp(-Controls.AverageValue(GameAxis.LookY) * lookYSensitivity, -lookYRateCap, lookYRateCap);
                }
                m_zoomTarget = Mathf.Clamp(m_zoomTarget + -Controls.AverageValue(GameAxis.Zoom) * scrollZoomSensitivity, minZoom, maxZoom);

                Vector2 lockSearchDir = new Vector2(Controls.AverageValue(GameAxis.LockX), Controls.AverageValue(GameAxis.LockY));
                if (lockSearchDir.magnitude > 0 && !m_alreadyChangedLock)
                {
                    Transform newTarget = ChangeLockTarget(lockSearchDir);
                    m_alreadyChangedLock = (newTarget != m_lockTarget);
                    m_lockTarget = newTarget;
                }
                if (lockSearchDir.magnitude == 0)
                {
                    m_alreadyChangedLock = false;
                }
            }
            transform.Rotate(0, rotateX, 0, Space.Self);
            m_zoom = Mathf.Lerp(m_zoom, m_zoomTarget, Time.deltaTime * zoomSmoothing);
            m_elevation = Mathf.Clamp(m_elevation, minElevation, maxElevation);
            
            // unlock if the current target is too far
            if (m_lockTarget && (m_lockTarget.position - m_player.position).magnitude > lockLoseRange)
            {
                m_lockTarget = null;
            }

            if (m_lockTarget)
            {
                Vector3 disp = m_lockTarget.position - transform.position;
                Vector3 dir = Vector3.ProjectOnPlane(disp, Vector3.up);
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir, Vector3.up), lockSmoothing * Time.deltaTime);
                Vector3 referencePoint = m_lockTarget.position - (pivot.position + transform.position) / 2;
                float targetElevation = (Mathf.Acos(referencePoint.y / referencePoint.magnitude) * Mathf.Rad2Deg - 90);
                float adjustFactor = 1 - (disp.magnitude / lockLoseRange);
                m_elevation = Mathf.Lerp(m_elevation, targetElevation + lockAngleAdjust * adjustFactor, lockSmoothing * Time.deltaTime);
            }

            pivot.rotation = transform.rotation * Quaternion.Euler(m_elevation, 0, 0);
        }
        else if (m_player)
        {
            m_lockTarget = null;
            transform.position = Vector3.Lerp(transform.position, m_playerRagdoll.CenterOfMass.position, m_deathSmoothing * Time.deltaTime);

            m_lastRotateSpeed = Mathf.Lerp(m_lastRotateSpeed, m_deathRotateSpeed, m_deathSmoothing * Time.deltaTime);
            m_elevation = Mathf.Lerp(m_elevation, m_deathElevation, m_deathSmoothing * Time.deltaTime);
            m_zoom = Mathf.Lerp(m_zoom, (minZoom + maxZoom) / 2, m_deathSmoothing * 0.25f * Time.deltaTime);

            transform.Rotate(0, m_deathRotateSpeed * Time.deltaTime, 0);
            pivot.rotation = transform.rotation * Quaternion.Euler(m_elevation, 0, 0);
        }
    }

    void Update()
    {
        float zoomFactor = (1 - Mathf.Clamp01((m_zoom - minZoom) / (maxZoom - minZoom)));

        Vector3 heightAdjust = transform.up * zoomFactor * zoomLowerHeight;
        Vector3 camPos = posTarget.position - heightAdjust;

        Vector3 lookPos;
        if (m_player && !m_playerHealth.IsAlive)
        {
            lookPos = Vector3.Lerp(m_lastLookPos, m_playerRagdoll.CenterOfMass.position, m_deathSmoothing * 2 * Time.deltaTime);
        }
        else
        {
            lookPos = rotTarget.position - heightAdjust * 0.5f;
        }
        m_lastLookPos = lookPos;

        Vector3 disp = (camPos - lookPos).normalized * m_zoom;
        float camDist = disp.magnitude;

        RaycastHit hit;
        if (Physics.SphereCast(lookPos, radius, disp, out hit, m_zoom, cameraBlockLayers))
        {
            camDist = Vector3.Distance(hit.point + hit.normal * radius, lookPos);
        }

        Camera.main.transform.position = lookPos + disp.normalized * camDist;
        Camera.main.transform.LookAt(lookPos);

        Debug.DrawLine(lookPos, Camera.main.transform.position, Color.red);
        Debug.DrawLine(posTarget.position, camPos, Color.cyan);
        Debug.DrawLine(rotTarget.position, lookPos, Color.cyan);
    }

    private Transform GetLockTarget()
    {
        Transform cam = Camera.main.transform;
        Transform mostSuitable = null;
        float mostSuitableCenterness = minimumLockCenterness;
        foreach (GameObject go in GameObject.FindGameObjectsWithTag("Lockable"))
        {
            Vector3 dir = (go.transform.position - cam.position).normalized;
            float centerness = Vector3.Dot(cam.forward, dir);
            float distance = (go.transform.position - m_player.position).magnitude;
            if (centerness > mostSuitableCenterness && distance < lockAquireRange && IsTargetLockable(go.transform, cam))
            {
                mostSuitable = go.transform;
                mostSuitableCenterness = centerness;
            }
        }
        return mostSuitable;
    }

    private Transform ChangeLockTarget(Vector2 searchDir)
    {
        if (m_lockTarget == null)
        {
            return null;
        }
        Transform cam = Camera.main.transform;
        Transform mostSuitable = m_lockTarget;
        float bestKiteNearness = float.MaxValue;
        
        Vector2 lockedEquirect = Utils.CartisianToEquirectangular(Utils.SwizzleXZY((m_lockTarget.position - cam.position).normalized));

        foreach (GameObject go in GameObject.FindGameObjectsWithTag("Lockable"))
        {
            Vector2 goEquirect = Utils.CartisianToEquirectangular(Utils.SwizzleXZY((go.transform.position - cam.position).normalized));

            for (int i = -1; i <= 1; i++)
            {
                Vector2 goEquirectTransformed = goEquirect + new Vector2(Mathf.PI * 2  * i, 0);
                Vector2 disp = goEquirectTransformed - lockedEquirect;
                float angle = Vector2.Angle(disp, searchDir);
                float kiteSlope = lockDirectioness;
                Vector2 rotatedDisp = Utils.Rotate(disp, -Mathf.Atan2(searchDir.y, searchDir.x));
                float kiteMagnitude;
                if (angle < lockChangeBearingMax)
                {
                    kiteMagnitude = rotatedDisp.x + Mathf.Abs(kiteSlope * rotatedDisp.y);
                }
                else
                {
                    kiteMagnitude = float.MaxValue;
                }

                float distance = (go.transform.position - m_player.position).magnitude;
                if (kiteMagnitude < bestKiteNearness &&
                    angle < lockChangeBearingMax &&
                    distance < lockAquireRange &&
                    go.transform != m_lockTarget &&
                    IsTargetLockable(go.transform, cam))
                {
                    mostSuitable = go.transform;
                    bestKiteNearness = kiteMagnitude;
                }
            }
        }
        return mostSuitable;
    }

    private bool IsTargetLockable(Transform target, Transform camera)
    {
        Vector3 screenPos = camera.GetComponent<Camera>().WorldToViewportPoint(target.position);
        bool onScreen = screenPos.z > 0 && screenPos.x > 0 && screenPos.x < 1 && screenPos.y > 0 && screenPos.y < 1;
        return (!onlyLockOnScreen || onScreen) && !Physics.Linecast(camera.position, target.position, lineOfSightBlocking);
    }
}
