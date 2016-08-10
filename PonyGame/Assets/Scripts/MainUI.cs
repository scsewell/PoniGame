using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class MainUI : MonoBehaviour
{
    [SerializeField] private CameraRig m_cameraRig;
    [SerializeField] private Canvas m_canvas;
    [SerializeField] private RectTransform m_lockSprite;

    [Tooltip("The angle new lock targets must be within from the input direction to be considered")]
    [SerializeField] [Range(-720, 720)]
    private float m_lockRotateSpeed = 0.0f;


    private static bool m_lockCursor = true;
    public static bool IsCursorLocked
    {
        get { return m_lockCursor; }
    }

    private static bool m_isMenuOpen = false;
    public static bool IsMenuOpen
    {
        get { return m_lockCursor; }
    }

    void Start()
    {
        SetCusorLock(m_lockCursor);
    }
	
	void Update()
    {
        if (Controls.VisualJustDown(GameButton.Menu))
        {
            m_isMenuOpen = !m_isMenuOpen;
            m_lockCursor = m_isMenuOpen;
        }

        SetCusorLock(m_lockCursor);

        m_lockSprite.GetComponent<Image>().enabled = false;
        if (m_cameraRig.LockTarget)
        {
            Vector3 screenPos = Camera.main.WorldToViewportPoint(m_cameraRig.LockTarget.position);
            m_lockSprite.anchorMin = screenPos;
            m_lockSprite.anchorMax = screenPos;
            if (screenPos.z > 0)
            {
                m_lockSprite.GetComponent<Image>().enabled = true;
            }
        }
        m_lockSprite.Rotate(Vector3.forward, m_lockRotateSpeed * Time.deltaTime);
    }
    
    void SetCusorLock(bool locked)
    {
        Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !locked;
    }
}
