using UnityEngine;
using System.Collections;

public class MainUI : MonoBehaviour
{
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
