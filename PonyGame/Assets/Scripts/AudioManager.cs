using UnityEngine;
using System.Collections;

public class AudioManager : MonoBehaviour
{
    [SerializeField] private AudioSource m_ambience;
    [SerializeField] private AudioSource m_deathSound;

    private void Start()
    {
        GameController.GameOver += OnGameOver;
    }

    private void OnDestroy()
    {
        GameController.GameOver -= OnGameOver;
    }

    public void OnGameOver()
    {
        m_deathSound.Play();
    }
}
