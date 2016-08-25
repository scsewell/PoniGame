using UnityEngine;
using System.Collections;
using System.Linq;

public class TSMagic : MonoBehaviour
{
    [SerializeField]
    private Color m_magicColor = Color.white;
    public Color MagicColor
    {
        get { return m_magicColor; }
    }

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
