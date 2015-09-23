using UnityEngine;
using System.Collections;

/*
 * Switches the active player between characters
 */
public class GameController : MonoBehaviour
{
    public Transform[] characters;
    public Transform harness;

    private static Transform m_player;
    public static Transform m_harness;
    private static bool m_playerSwitched = false;


    void Start()
    {
        m_harness = harness;
    }

    void Update()
    {
        m_playerSwitched = false;

        if (!PlayerExists())
        {
            SetPlayer(characters[0]);
        }

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (characters[0] == m_player)
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
        m_player = newPlayer;
        newPlayer.tag = "Player";
        newPlayer.GetComponent<TSAI>().enabled = false;
        m_playerSwitched = true;
    }

    private void ResetPlayer(Transform oldPlayer)
    {
        oldPlayer.tag = "Untagged";
        oldPlayer.GetComponent<TSAI>().enabled = true;
    }

    // returns true if there is a player object
    public static bool PlayerExists()
    {
        return (m_player != null);
    }

    // returns true if the player object was changed this frame
    public static bool PlayerChanged()
    {
        return m_playerSwitched;
    }

    // returns the player's transform if it exists
    public static Transform GetPlayer()
    {
        if (PlayerExists())
        {
            return m_player;
        }
        else
        {
            Debug.LogError("No player set!");
            return null;
        }
    }

    // returns the harness transform if it is set
    public static Transform GetHarness()
    {
        if (m_harness != null)
        {
            return m_harness;
        }
        else
        {
            Debug.LogError("No harness set!");
            return null;
        }
    }

    // returns the cart transform if it is set
    public static Transform GetCart()
    {
        if (m_harness != null)
        {
            return m_harness.root;
        }
        else
        {
            Debug.LogError("No harness set!");
            return null;
        }
    }
}
