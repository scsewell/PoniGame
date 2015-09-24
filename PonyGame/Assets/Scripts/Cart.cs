using UnityEngine;
using System.Collections;

public class Cart : MonoBehaviour
{
    public Transform harnessCenter;
    public Transform centerOfMass;
    public Rigidbody sweepTester;
    public ConfigurableJoint harnessJoint;
    public AudioSource rattleSource;

    private Transform m_pony;
    private Transform m_waist;
    private ConfigurableJoint m_joint;

    
    void Start ()
    {
        GameController.GetCart().GetComponent<Rigidbody>().centerOfMass = centerOfMass.localPosition;
    }

    void LateUpdate()
    {
        if (m_pony && m_joint)
        {
            m_joint.connectedAnchor = m_pony.InverseTransformPoint(m_waist.position);
        }

        rattleSource.volume = Mathf.Min(GameController.GetCart().GetComponent<Rigidbody>().velocity.magnitude / 2.5f, 0.4f);
    }

    /*
     * Gets the rotation of the harness joint
     */
    public Quaternion GetHarnessRotation()
    {
        return Quaternion.Inverse(GameController.GetCart().transform.rotation) * transform.rotation;
    }

    /*
     * Does a sweep and checks if there is nothing obstucting the forward movement of the cart, ignoring the cart and player itself
     */
    public bool IsFrontClear()
    {
        bool frontIsClear = true;

        sweepTester.GetComponent<Collider>().enabled = true;
        foreach (RaycastHit hit in sweepTester.SweepTestAll(transform.forward, 0.15f))
        {
            bool isSelfHit = hit.transform.root == GameController.GetCart() || hit.transform.root == m_pony;
            bool isKinematic = hit.transform.GetComponent<CharacterController>();
            bool isStatic = hit.transform.gameObject.isStatic;

            if ((isStatic || isKinematic) && !isSelfHit)
            {
                frontIsClear = false;
            }
        }
        sweepTester.GetComponent<Collider>().enabled = false;

        return frontIsClear;
    }

    /*
     * Harnesses a pony to the cart by adding a joint
     */
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

        Vector3 targetPos = m_pony.TransformPoint(new Vector3(0, 0.2f, -0.014f));
        Vector3 dir = (targetPos - transform.position).normalized;
        transform.rotation = Quaternion.LookRotation(dir, m_pony.up);

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

    /*
     * Removes the joint connecting the cart and a pony
     */
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
