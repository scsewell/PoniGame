using UnityEngine;
using System.Collections;

public class Cart : MonoBehaviour
{
    public Transform harnessCenter;
    public Transform centerOfMass;

    private Transform m_pony;


	// Use this for initialization
	void Start ()
    {
        transform.root.GetComponent<Rigidbody>().centerOfMass = centerOfMass.localPosition;
    }
	
	// Update is called once per frame
	void LateUpdate ()
    {
        if (m_pony && gameObject.GetComponent<ConfigurableJoint>())
        {
            gameObject.GetComponent<ConfigurableJoint>().connectedAnchor = m_pony.position + m_pony.rotation * new Vector3(0, 0.2f, -0.014f);
        }
	}

    public void Harness(Transform pony)
    {
        m_pony = pony;
        
        Vector3 targetPos = GameController.GetPlayer().position + new Vector3(0, 0.2f, -0.014f);
        Vector3 dir = (targetPos - transform.position).normalized;
        transform.rotation = Quaternion.LookRotation(dir, GameController.GetPlayer().up);

        if (gameObject.GetComponent<ConfigurableJoint>())
        {
            Destroy(gameObject.GetComponent<ConfigurableJoint>());
        }

        ConfigurableJoint joint;
        joint = gameObject.AddComponent<ConfigurableJoint>();
        joint.autoConfigureConnectedAnchor = false;
        joint.anchor = harnessCenter.localPosition;
        joint.axis = Vector3.up;
        joint.xMotion = ConfigurableJointMotion.Locked;
        joint.yMotion = ConfigurableJointMotion.Locked;
        joint.zMotion = ConfigurableJointMotion.Locked;

        joint.breakForce = float.MaxValue;
        joint.breakTorque = float.MaxValue;
    }

    public void RemoveHarness()
    {
        m_pony = null;

        if (gameObject.GetComponent<ConfigurableJoint>())
        {
            Destroy(gameObject.GetComponent<ConfigurableJoint>());
        }
    }
}
