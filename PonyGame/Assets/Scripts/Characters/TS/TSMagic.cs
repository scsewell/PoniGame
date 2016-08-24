using UnityEngine;
using System.Collections;
using System.Linq;

public class TSMagic : MonoBehaviour
{
    private bool m_canUseMagic = true;
    public bool CanUseMagic
    {
        get { return m_canUseMagic; }
    }

    private void Start()
    {

    }

    public void SetCanUseMagic(bool val)
    {
        m_canUseMagic = val;
    }

    private void FixedUpdate()
    {

	}
}
