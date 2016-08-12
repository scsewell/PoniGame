using UnityEngine;
using System.Collections;

public class AudioManager : MonoBehaviour
{
    [SerializeField]
    private AudioSource m_ambience;
    [SerializeField]
    public AudioSource m_deathSound;

    private static bool m_playDeathSound = false;

    void Update()
    {
        if (m_playDeathSound)
        {
            m_deathSound.Play();
            m_playDeathSound = false;
        }
	}

    public static void PlayDeathSound()
    {
        m_playDeathSound = true;
    }
}
