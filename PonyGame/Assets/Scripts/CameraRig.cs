using UnityEngine;
using System.Collections;

public class CameraRig : MonoBehaviour
{
    public Transform posTarget;
    public Transform rotTarget;
    public Transform pivot;

    public LayerMask layers; // which colliders the camera will put itself in front of if one is obscuring the character

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
    [Range(0, 2)]
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

    [Tooltip("Higher values require targets to be more central in the screen to be locked on to")]
    [Range(0, 1)]
    public float minimumLockCenterness = 0.8f;

    [Tooltip("How quickly the camera aims at the lock target")]
    [Range(0, 30)]
    public float lockSmoothing = 8.0f;


    private Transform m_player;
    private Transform m_lockTarget;
    private TransformInterpolator m_transformInterpolator;
    private TransformInterpolator m_pivotInterpolator;
    private Interpolator<float> m_zoomInterpolator;
    private float m_elevation = 0;
    private float m_zoom;
    private float m_zoomTarget;

    void Start()
    {
        GameController.CharacterChanged += Initialize;

        m_transformInterpolator = GetComponent<TransformInterpolator>();
        m_pivotInterpolator = pivot.GetComponent<TransformInterpolator>();

        m_zoomInterpolator = new Interpolator<float>(new InterpolatedFloat(() => (m_zoom), (val) => { m_zoom = val; }));
        gameObject.AddComponent<FloatInterpolator>().Initialize(m_zoomInterpolator);
    }

    void OnDestroy()
    {
        GameController.CharacterChanged -= Initialize;
    }

    void Initialize(Transform player)
    {
        m_player = player;
        transform.position = player.position;
        transform.rotation = Quaternion.LookRotation(Vector3.ProjectOnPlane(player.forward, Vector3.up), Vector3.up);
        Camera.main.transform.position = posTarget.position;
        Camera.main.transform.LookAt(rotTarget);

        m_zoom = Vector3.Distance(rotTarget.position, posTarget.position);
        m_zoomTarget = Vector3.Distance(rotTarget.position, posTarget.position);

        m_transformInterpolator.ForgetPreviousValues();
        m_pivotInterpolator.ForgetPreviousValues();
        m_zoomInterpolator.ForgetPreviousValues();
    }
	
	void FixedUpdate() 
	{
        if (m_player)
        {
            transform.position = m_player.position;

            float rotateX = 0;
            if (MainUI.IsCursorLocked)
            {
                rotateX = Mathf.Clamp(Controls.AverageValue(GameAxis.LookX) * lookXSensitivity, -lookXRateCap, lookXRateCap);
                m_elevation += Mathf.Clamp(-Controls.AverageValue(GameAxis.LookY) * lookYSensitivity, -lookYRateCap, lookYRateCap);
                m_zoomTarget = Mathf.Clamp(m_zoomTarget + -Controls.AverageValue(GameAxis.Zoom) * scrollZoomSensitivity, minZoom, maxZoom);
            }
            transform.Rotate(0, rotateX, 0, Space.Self);
            m_zoom = Mathf.Lerp(m_zoom, m_zoomTarget, Time.deltaTime * zoomSmoothing);
            m_elevation = Mathf.Clamp(m_elevation, minElevation, maxElevation);

            // aquire new lock
            bool changedLock = false;
            if (Controls.JustDown(GameButton.Lock))
            {
                Transform newTarget = GetLockTarget();
                changedLock = newTarget != m_lockTarget;
                m_lockTarget = m_lockTarget == null ? newTarget : null;
            }

            // unlock if the current target is too far
            if (m_lockTarget && (m_lockTarget.position - m_player.position).magnitude > lockLoseRange)
            {
                m_lockTarget = null;
            }

            if (m_lockTarget)
            {
                Vector3 dir = Vector3.ProjectOnPlane(m_lockTarget.position - transform.position, Vector3.up);
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir, Vector3.up), lockSmoothing * Time.deltaTime);
                m_elevation = Mathf.Lerp(m_elevation, 0, lockSmoothing * Time.deltaTime);
            }

            pivot.rotation = transform.rotation * Quaternion.Euler(m_elevation, 0, 0);

            if (changedLock)
            {
                m_transformInterpolator.ForgetPreviousValues();
                m_pivotInterpolator.ForgetPreviousValues();
            }
        }
    }

    void Update()
    {
        float zoomFactor = (1 - Mathf.Clamp01((m_zoom - minZoom) / (maxZoom - minZoom)));

        Vector3 camPos = posTarget.position - transform.up * zoomFactor * 0.6f * zoomLowerHeight;
        Vector3 lookPos = rotTarget.position - transform.up * zoomFactor * 0.15f * zoomLowerHeight;

        Vector3 disp = (camPos - lookPos).normalized * m_zoom;
        float camDist = disp.magnitude;

        RaycastHit hit;
        if (Physics.SphereCast(lookPos, radius, disp, out hit, m_zoom, layers))
        {
            camDist = Vector3.Distance(hit.point + hit.normal * radius, lookPos);
        }

        Camera.main.transform.position = pivot.position + disp.normalized * camDist;

        if (m_lockTarget)
        {
            Camera.main.transform.LookAt(m_lockTarget.position);
        }
        else
        {
            Camera.main.transform.LookAt(lookPos);
        }

        Camera.main.transform.LookAt(lookPos);

        Debug.DrawLine(lookPos, Camera.main.transform.position, Color.red);
    }

    private Transform GetLockTarget()
    {
        Transform mostSuitable = null;
        float mostSuitableCenterness = minimumLockCenterness;
        foreach (GameObject go in GameObject.FindGameObjectsWithTag("Lockable"))
        {
            Vector3 dir = (go.transform.position - Camera.main.transform.position).normalized;
            float centerness = Vector3.Dot(Camera.main.transform.forward, dir);
            float distance = (go.transform.position - m_player.position).magnitude;
            if (centerness > mostSuitableCenterness && distance < lockAquireRange)
            {
                mostSuitable = go.transform;
                mostSuitableCenterness = centerness;
            }
        }
        return mostSuitable;
    }
}
