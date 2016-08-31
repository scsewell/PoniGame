using UnityEngine;
using System.Collections;

public class MagicAttack1 : MonoBehaviour
{
    public float damage = 12.0f;
    public float speed = 6.5f;

    private Transform m_owner;
    private bool m_alreadyHit = false;

	// Use this for initialization
	void Start ()
    {
        GetComponent<Rigidbody>().velocity = transform.forward * speed;
	}

    public void SetOwner(Transform owner)
    {
        m_owner = owner;
    }
	
	private void OnTriggerEnter (Collider other)
    {
	    if (other.transform.root != m_owner && !m_alreadyHit)
        {
            if (other.transform.root.GetComponent<Health>())
            {
                other.transform.root.GetComponent<Health>().IncrimentHealth(-damage);
            }
            m_alreadyHit = true;
            Destroy(gameObject);
        }
    }
}
