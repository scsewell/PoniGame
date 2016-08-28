using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

/*
 * Switches the active player between characters
 */
public class GameController : MonoBehaviour
{
    //[SerializeField] private Transform defaultCharacterPrefab;
    //[SerializeField] private Transform spawnPoint;
    [SerializeField] private CameraRig cameraRig;

    private static List<Transform> m_characters;

    private static Transform m_player;
    public static Transform Player
    {
        get { return m_player; }
    }

    private static CameraRig m_cameraRig;
    public static CameraRig CameraRig
    {
        get { return m_cameraRig; }
    }

    private static bool m_isGameOver = false;
    public static bool IsGameOver
    {
        get { return m_isGameOver; }
    }

    public delegate void GameOverHandler();
    public static event GameOverHandler GameOver;

    public delegate void CharacterChangeHandler(Transform newCharacter);
    public static event CharacterChangeHandler CharacterChanged;

    void Awake()
    {
        m_characters = new List<Transform>();
        m_cameraRig = cameraRig;
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
        if (m_characters.Remove(transform))
        {
            ResetCharacter(transform);
            if (transform == m_player && m_characters.Count > 0)
            {
                SetPlayerCharacter(m_characters.First());
            }
            else if (transform == m_player)
            {
                m_player = null;
                m_isGameOver = true;
                if (GameOver != null)
                {
                    GameOver();
                }
            }
        }
    }

    private void Update()
    {
        if (m_player == null && m_characters.Count > 0)
        {
            SetPlayerCharacter(m_characters.First());
        }
        if (m_characters.Count > 1 && Controls.JustDown(GameButton.SwitchCharacter))
        {
            SetPlayerCharacter(m_characters[(m_characters.IndexOf(m_player) + 1) % m_characters.Count]);
        }
        if (m_player != null && Controls.JustDown(GameButton.ConsiderSuicide))
        {
            m_player.GetComponent<Health>().ApplyDamage(10);
        }
    }

    private static void SetPlayerCharacter(Transform newPlayer)
    {
        m_characters.ForEach(c => ResetCharacter(c));
        m_player = newPlayer;
        newPlayer.tag = "Player";
        if (CharacterChanged != null)
        {
            CharacterChanged(m_player);
        }
    }

    private static void ResetCharacter(Transform oldPlayer)
    {
        oldPlayer.tag = "Untagged";
    }
}
