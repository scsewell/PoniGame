using UnityEngine;
using System.Collections;

public class RagdollCamera : MonoBehaviour
{
    [SerializeField]
    private Transform m_centerOfMass;

    private Transform m_interpolatedCOM;

    private void Start()
    {
        GameObject go = new GameObject();
        m_interpolatedCOM = go.transform;
        go.AddComponent<TransformInterpolator>();
    }

    private void FixedUpdate()
    {
        m_interpolatedCOM.position = m_centerOfMass.position;
        m_interpolatedCOM.rotation = m_centerOfMass.rotation;
    }

    public Transform CenterOfMass
    {
        get { return m_interpolatedCOM; }
    }
}
