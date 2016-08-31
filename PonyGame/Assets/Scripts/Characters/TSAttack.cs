using UnityEngine;
using System.Collections;

public class TSAttack : MonoBehaviour
{
    public Transform magicAttackPrefab;

    public Transform magicSpawn;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update ()
    {
	    if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            Transform magic = Instantiate(magicAttackPrefab, magicSpawn.position, Quaternion.LookRotation(Camera.main.transform.forward)) as Transform;
            magic.GetComponent<MagicAttack1>().SetOwner(transform);
        }
	}
}
