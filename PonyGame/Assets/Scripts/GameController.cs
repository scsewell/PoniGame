using UnityEngine;
using System.Collections;

public class GameController : MonoBehaviour
{
    public Transform twilightPrefab;
    public Transform twilightSpawn;

    /*
     * Spawns some amout of Twilights on the ground
     */
	void Start ()
    {
        if (twilightSpawn != null)
        {
            RaycastHit hit;
            Physics.Raycast(twilightSpawn.position, Vector3.down, out hit);

            Transform twilight = Instantiate(twilightPrefab, hit.point, twilightSpawn.rotation) as Transform;

            twilight.tag = "Player";
            twilight.GetComponent<TSAI>().enabled = false;
        }
	}
}
