using UnityEngine;
using System.Collections;

public class CameraRig : MonoBehaviour
{
    public Transform posTarget;
    public Transform rotTarget;

    public float lookHeight = 0.65f;
    public float turnLookDistance = 0.2f;
    public float positionSmoothTime = 0.25f;
    public float lookSmoothTime = 0.25f;

    private Transform player;
    private Vector3 mainVelocity = Vector3.zero;
    private Vector3 lookVelocity = Vector3.zero;

    void Update()
    {
        if (!player)
        {
            player = GameObject.FindGameObjectWithTag("Player").transform;

            if (player)
            {
                transform.position = player.position;
                transform.rotation = player.rotation;
                Camera.main.transform.position = posTarget.position;
                Camera.main.transform.LookAt(rotTarget);
            }
        }
    }
	
	// Update is called once per frame
	void LateUpdate () 
	{
        if (player)
        {
            transform.position = player.position;
            transform.rotation = player.rotation;

            Vector3 lookPosTarget = new Vector3(Input.GetAxis("Horizontal") * turnLookDistance, lookHeight, 0);
            rotTarget.localPosition = Vector3.SmoothDamp(rotTarget.localPosition, lookPosTarget, ref lookVelocity, lookSmoothTime);

            Camera.main.transform.position = Vector3.SmoothDamp(Camera.main.transform.position, posTarget.position, ref mainVelocity, positionSmoothTime);
            Camera.main.transform.LookAt(rotTarget);
        }
    }
}
