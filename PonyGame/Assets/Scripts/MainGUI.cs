using UnityEngine;
using System.Collections;

public class MainGUI : MonoBehaviour
{
    public bool lockCursor = true;

	void Start ()
    {
        SetCusorLock(lockCursor);
    }
	
	void Update ()
    {
        // show and hide cursor as appropriate
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            lockCursor = !lockCursor;
        }

        SetCusorLock(lockCursor);
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
