using UnityEngine;
using System.Collections;

public class Billboard : MonoBehaviour
{
	void LateUpdate()
    {
         transform.rotation = Quaternion.LookRotation(Vector3.ProjectOnPlane(Camera.main.transform.forward, Vector3.up), Vector3.up);
	}
}
