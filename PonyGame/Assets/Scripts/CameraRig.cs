using UnityEngine;
using System.Collections;
using InControl;

public class CameraRig : MonoBehaviour
{
    public Transform posTarget;
    public Transform rotTarget;
    public Transform pivot;

    public LayerMask layers; // which colliders the camera will put itself in front of if one is obscuring the character

    [Tooltip("How fast the character may look around horizontally with the mouse (No Units)")]
    [Range(1.0f, 10.0f)]
    public float mouseLookXSensitivity = 4.0f;

    [Tooltip("How fast the character may look around vertically with the mouse (No Units)")]
    [Range(1.0f, 10.0f)]
    public float mouseLookYSensitivity = 2.0f;

    [Tooltip("How fast the character may look around horizontally with the gamepad (No Units)")]
    [Range(1.0f, 15.0f)]
    public float stickLookXSensitivity = 6.0f;

    [Tooltip("How fast the character may look around vertically with the gamepad (No Units)")]
    [Range(1.0f, 15.0f)]
    public float stickLookYSensitivity = 3.0f;

    [Tooltip("How fast the camera zooms in and out (No Units)")]
    [Range(0.25f, 4.0f)]
    public float scrollZoomSensitivity = 2.0f;

    [Tooltip("Max amount the character may look around horizontally (Degrees / Second)")]
    [Range(1.0f, 30.0f)]
    public float lookXRateCap = 10.0f;

    [Tooltip("Max amount the character may look around vertically in (Degrees / Second)")]
    [Range(1.0f, 30.0f)]
    public float lookYRateCap = 10.0f;

    [Tooltip("Max angle towards the upper pole the camera may be elevated (Degrees)")]
    [Range(0, 90.0f)]
    public float maxElevation = 45.0f;

    [Tooltip("Max angle towards the lower pole the camera may be depressed (Degrees)")]
    [Range(-90, 0)]
    public float minElevation = -30;

    [Tooltip("How close the camera may be to its pivot (Units)")]
    [Range(0, 2.0f)]
    public float minZoom = 1.0f;

    [Tooltip("How far the camera may be from its pivot (Units)")]
    [Range(1.0f, 3.0f)]
    public float maxZoom = 2.0f;

    [Tooltip("How close the camera may be to its pivot while the player is pulling the cart (Units)")]
    [Range(0, 2.0f)]
    public float minZoomCart = 1.0f;

    [Tooltip("How far the camera may be from its pivot while the player is pulling the cart (Units)")]
    [Range(1.0f, 3.0f)]
    public float maxZoomCart = 2.0f;

    [Tooltip("How much the camera height is lowered as the camera approaches the pivot")]
    [Range(0, 2.0f)]
    public float zoomLowerHeight = 1.0f;

    [Tooltip("Camera collision radius (Units)")]
    [Range(0.0f, 1.0f)]
    public float radius = 0.1f;

    private float m_elevation = 0;
    private float m_zoom;
    private float m_zoomTarget;


    void Update()
    {
        if (GameController.PlayerChanged())
        {
            Transform player = GameController.GetPlayer();
            transform.position = player.position;
            transform.rotation = Quaternion.LookRotation(Vector3.ProjectOnPlane(player.forward, Vector3.up), Vector3.up);
            Camera.main.transform.position = posTarget.position;
            Camera.main.transform.LookAt(rotTarget);

            m_zoom = Vector3.Distance(rotTarget.position, posTarget.position);
            m_zoomTarget = Vector3.Distance(rotTarget.position, posTarget.position);
        }
    }
	
	// Update is called once per frame
	void LateUpdate () 
	{
        if (GameController.PlayerExists())
        {
            InputDevice device = InputManager.ActiveDevice;

            float rotateX = Mathf.Clamp(Input.GetAxis("Mouse X") * mouseLookXSensitivity + device.RightStick.X * stickLookXSensitivity, -lookXRateCap, lookXRateCap);
            transform.Rotate(0, rotateX, 0, Space.Self);

            m_elevation += Mathf.Clamp(-Input.GetAxis("Mouse Y") * mouseLookYSensitivity - device.RightStick.Y * stickLookYSensitivity, -lookYRateCap, lookYRateCap);
            m_elevation = Mathf.Clamp(m_elevation, minElevation, maxElevation);
            pivot.rotation = transform.rotation * Quaternion.Euler(m_elevation, 0, 0);

            transform.position = GameController.GetPlayer().position;

            if (GameController.GetPlayer().GetComponent<TSMovement>().PullingCart)
            {
                m_zoomTarget = Mathf.Clamp(m_zoomTarget + -Input.GetAxis("Mouse ScrollWheel") * scrollZoomSensitivity, minZoomCart, maxZoomCart);
            }
            else
            {
                m_zoomTarget = Mathf.Clamp(m_zoomTarget + -Input.GetAxis("Mouse ScrollWheel") * scrollZoomSensitivity, minZoom, maxZoom);
            }
            m_zoom = Mathf.Lerp(m_zoom, m_zoomTarget, Time.deltaTime * 4.0f);

            float zoomFactor = (1 - Mathf.Clamp01((m_zoom - minZoom) / (maxZoom - minZoom)));

            Vector3 m_camPos = posTarget.position - transform.up * zoomFactor * 0.6f * zoomLowerHeight;
            Vector3 m_lookPos = rotTarget.position - transform.up * zoomFactor * 0.15f * zoomLowerHeight;

            Vector3 disp = (m_camPos - m_lookPos).normalized * m_zoom;
            float camDist = disp.magnitude;

            RaycastHit hit;
            if (Physics.SphereCast(m_lookPos, radius, disp, out hit, m_zoom, layers))
            {
                camDist = Vector3.Distance(hit.point + hit.normal * radius, m_lookPos);
            }

            Camera.main.transform.position = pivot.position + disp.normalized * camDist;
            Camera.main.transform.LookAt(m_lookPos);
        }
    }
}
