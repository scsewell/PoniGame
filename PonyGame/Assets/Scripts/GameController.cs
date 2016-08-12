using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

/*
 * Switches the active player between characters
 */
public class GameController : MonoBehaviour
{
    public Transform defaultCharacter;
    public Transform spawnPoint;

    private static List<Transform> m_characters;
    private static Transform m_player;

    public delegate void CharacterChangeHandler(Transform newCharacter);
    public static event CharacterChangeHandler CharacterChanged;

    void Start()
    {
        m_characters = new List<Transform>();
    }

    public static void AddCharacter(Transform transform)
    {
        if (!m_characters.Contains(transform))
        {
            m_characters.Add(transform);
        }
    }

    public static void RemoveCharacter(Transform transform)
    {
        if (m_characters.Contains(transform))
        {
            m_characters.Remove(transform);
        }
    }

    public static void PlayerDied()
    {
        AudioManager.PlayDeathSound();
        CameraManager.FadeToGrey = true;
    }

    private void Update()
    {
        if (m_characters.Count == 0)
        {
            Instantiate(defaultCharacter, spawnPoint.position, spawnPoint.rotation);
        }

        if (m_player == null && m_characters.Count > 0)
        {
            SetPlayer(m_characters.First());
        }

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            SetPlayer(m_characters[(m_characters.IndexOf(m_player) + 1) % m_characters.Count]);
            m_characters.Where(c => c != m_player).ToList().ForEach(c => ResetPlayer(c));
        }
    }

    private void SetPlayer(Transform newPlayer)
    {
        m_player = newPlayer;
        newPlayer.tag = "Player";
        if (CharacterChanged != null)
        {
            CharacterChanged(m_player);
        }
    }

    private void ResetPlayer(Transform oldPlayer)
    {
        oldPlayer.tag = "Untagged";
    }
}
