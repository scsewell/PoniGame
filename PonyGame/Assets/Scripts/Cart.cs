using UnityEngine;
using System.Collections;

public class Cart : MonoBehaviour
{
    public Transform harnessCenter;
    public Transform centerOfMass;

    private Transform m_pony;
    private Transform m_waist;
    private ConfigurableJoint m_joint;
    private Vector3 m_restPos;

    // Use this for initialization
    void Start ()
    {
        GameController.GetCart().GetComponent<Rigidbody>().centerOfMass = centerOfMass.localPosition;
        m_restPos = GameController.GetCart().InverseTransformPoint(transform.position);
    }

    void LateUpdate()
    {
        if (m_pony && m_joint)
        {
            m_joint.connectedAnchor = m_pony.InverseTransformPoint(m_waist.position);
        }
    }

    public float GetJointDistance()
    {
        return Vector3.Distance(GameController.GetCart().TransformPoint(m_restPos), transform.position);
    }

    public void Harness(Transform pony)
    {
        m_pony = pony;
        transform.parent = m_pony;

        foreach (GameObject go in GameObject.FindGameObjectsWithTag("Waist"))
        {
            if (go.transform.root == pony)
            {
                m_waist = go.transform;
            }
        }

        Vector3 targetPos = GameController.GetPlayer().position + new Vector3(0, 0.2f, -0.014f);
        Vector3 dir = (targetPos - transform.position).normalized;
        transform.rotation = Quaternion.LookRotation(dir, GameController.GetPlayer().up);

        if (m_joint)
        {
            Destroy(m_joint);
        }
        
        m_joint = gameObject.AddComponent<ConfigurableJoint>();

        m_joint.autoConfigureConnectedAnchor = false;
        m_joint.axis = Vector3.up;
        m_joint.xMotion = ConfigurableJointMotion.Locked;
        m_joint.yMotion = ConfigurableJointMotion.Locked;
        m_joint.zMotion = ConfigurableJointMotion.Locked;
        m_joint.angularXMotion = ConfigurableJointMotion.Limited;
        m_joint.angularYMotion = ConfigurableJointMotion.Limited;
        m_joint.angularZMotion = ConfigurableJointMotion.Limited;
        SoftJointLimit xLimit1 = new SoftJointLimit();
        SoftJointLimit xLimit2 = new SoftJointLimit();
        SoftJointLimit yLimit = new SoftJointLimit();
        SoftJointLimit zLimit = new SoftJointLimit();
        xLimit1.limit = -15.0f;
        xLimit2.limit = 15.0f;
        yLimit.limit = 35.0f;
        zLimit.limit = 35.0f;
        m_joint.lowAngularXLimit = xLimit1;
        m_joint.highAngularXLimit = xLimit2;
        m_joint.angularYLimit = yLimit;
        m_joint.angularZLimit = zLimit;
        m_joint.angularZMotion = ConfigurableJointMotion.Locked;
        m_joint.breakForce = float.MaxValue;
        m_joint.breakTorque = float.MaxValue;

        m_joint.connectedBody = pony.GetComponent<Rigidbody>();
        m_joint.anchor = harnessCenter.localPosition;
    }

    public void RemoveHarness()
    {
        m_pony = null;
        transform.parent = GameController.GetCart();

        if (m_joint)
        {
            Destroy(m_joint);
        }
    }
}
