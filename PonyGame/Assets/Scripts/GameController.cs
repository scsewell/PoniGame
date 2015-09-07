using UnityEngine;
using System.Collections;

public class GameController : MonoBehaviour
{
    public Transform twilightPrefab;
    public int twilights = 1;

    /*
     * Spawns some amout of Twilights on the ground
     */
	void Start ()
    {
	    for (int i = 0; i < twilights; i++)
        {
            Vector3 pos = new Vector3(Random.Range(-9, 9), 10, Random.Range(-9, 9));

            RaycastHit hit;
            Physics.Raycast(pos, Vector3.down, out hit);

            Transform twilight = Instantiate(twilightPrefab, hit.point, Quaternion.Euler(0, Random.Range(0, 360), 0)) as Transform;

            if (i == 0)
            {
                twilight.tag = "Player";
                Destroy(twilight.GetComponent<TSAI>());
            }
        }
	}
}
