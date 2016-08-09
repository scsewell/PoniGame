using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class MainUI : MonoBehaviour
{
    public CameraRig cameraRig;
    public Canvas canvas;
    public RectTransform lockSprite;


    [Tooltip("The angle new lock targets must be within from the input direction to be considered")]
    [SerializeField] [Range(-720, 720)]
    private float m_lockRotateSpeed = 0.0f;


    private static bool m_lockCursor = true;
    public static bool IsCursorLocked
    {
        get { return m_lockCursor; }
    }

	void Start()
    {
        SetCusorLock(m_lockCursor);
    }
	
	void Update()
    {
        lockSprite.GetComponent<Image>().enabled = false;
        if (cameraRig.LockTarget)
        {
            Vector3 screenPos = Camera.main.WorldToViewportPoint(cameraRig.LockTarget.position);
            lockSprite.anchorMin = screenPos;
            lockSprite.anchorMax = screenPos;
            if (screenPos.z > 0)
            {
                lockSprite.GetComponent<Image>().enabled = true;
            }
        }
        lockSprite.Rotate(Vector3.forward, m_lockRotateSpeed * Time.deltaTime);

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            m_lockCursor = !m_lockCursor;
        }

        SetCusorLock(m_lockCursor);
    }

    /*
     * Sets the lock state of the mouse cursor
     */
    void SetCusorLock(bool locked)
    {
        Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !locked;
    }
}
