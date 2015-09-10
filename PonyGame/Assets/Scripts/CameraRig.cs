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

    [Tooltip("Camera collision radius (Units)")]
    [Range(0.0f, 1.0f)]
    public float radius = 0.1f;

    private Transform m_player;
    private float m_elevation = 0;

    void Update()
    {
        if (!m_player)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");

            if (player)
            {
                m_player = player.transform;
                transform.position = m_player.position;
                transform.rotation = m_player.rotation;
                Camera.main.transform.position = posTarget.position;
                Camera.main.transform.LookAt(rotTarget);
            }
        }
    }
	
	// Update is called once per frame
	void LateUpdate () 
	{
        if (m_player)
        {
            transform.position = m_player.position;

            InputDevice device = InputManager.ActiveDevice;

            float rotateX = Mathf.Clamp(Input.GetAxis("Mouse X") * mouseLookXSensitivity + device.RightStick.X * stickLookXSensitivity, -lookXRateCap, lookXRateCap);
            transform.Rotate(0, rotateX, 0, Space.Self);

            m_elevation += Mathf.Clamp(-Input.GetAxis("Mouse Y") * mouseLookYSensitivity - device.RightStick.Y * stickLookYSensitivity, -lookYRateCap, lookYRateCap);
            m_elevation = Mathf.Clamp(m_elevation, minElevation, maxElevation);
            pivot.rotation = transform.rotation * Quaternion.Euler(m_elevation, 0, 0);

            Vector3 disp = posTarget.position - pivot.position;
            float camDist = disp.magnitude;

            RaycastHit hit;
            if (Physics.SphereCast(pivot.position, radius, disp, out hit, disp.magnitude, layers))
            {
                camDist = (hit.point + hit.normal * radius - pivot.position).magnitude;
            }
            
            Camera.main.transform.position = pivot.position + disp.normalized * camDist;
            Camera.main.transform.LookAt(rotTarget);
        }
    }
}
