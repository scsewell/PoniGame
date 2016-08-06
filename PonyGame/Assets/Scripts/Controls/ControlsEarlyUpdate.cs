using UnityEngine;
using System.Collections;

public class ControlsEarlyUpdate : MonoBehaviour
{
    Controls m_controls;

	void Awake()
    {
        m_controls = GetComponent<Controls>();
    }
	
	void Update()
    {
        m_controls.EarlyUpdate();
    }
}
