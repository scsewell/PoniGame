using UnityEngine;
using System.Collections;

public class RagdollCamera : MonoBehaviour
{
    [SerializeField]
    private Transform m_centerOfMass;
    public Transform CenterOfMass
    {
        get { return m_centerOfMass; }
    }
}
