using UnityEngine;
using System.Collections;

/*
 * Switches the active player between characters
 */
public class GameController : MonoBehaviour
{
    public Transform[] characters;
    
	void Start ()
    {
        SetPlayer(characters[0]);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (characters[0].tag == "Player")
            {
                SetPlayer(characters[1]);
                ResetPlayer(characters[0]);
            }
            else
            {
                SetPlayer(characters[0]);
                ResetPlayer(characters[1]);
            }
        }
    }

    private void SetPlayer(Transform newPlayer)
    {
        newPlayer.tag = "Player";
        newPlayer.GetComponent<TSAI>().enabled = false;
    }

    private void ResetPlayer(Transform oldPlayer)
    {
        oldPlayer.tag = "Untagged";
        oldPlayer.GetComponent<TSAI>().enabled = true;
    }
}
