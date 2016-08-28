using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class MainUI : MonoBehaviour
{
    [SerializeField] private Canvas m_gameCanvas;
    [SerializeField] private Canvas m_gameOverCanvas;
    [SerializeField] private bool m_fadeUIOnGameOver = true;
    [SerializeField] private float m_gameOverFadeWait = 5.0f;
    [SerializeField] private float m_gameOverFadeDuration = 2.0f;

    private float m_gameOverFadeTime = 0;

    private static bool m_lockCursor = true;
    public static bool IsCursorLocked
    {
        get { return m_lockCursor; }
    }

    private static bool m_isMenuOpen = false;
    public static bool IsMenuOpen
    {
        get { return IsMenuOpen; }
    }

    private void Start()
    {
        m_gameOverCanvas.GetComponentsInChildren<CanvasRenderer>().ToList().ForEach(r => r.SetAlpha(0));

        GameController.GameOver += OnGameOver;
    }

    private void OnDestroy()
    {
        GameController.GameOver -= OnGameOver;
    }

    private void Update()
    {
        if (Controls.JustDown(GameButton.Menu))
        {
            m_isMenuOpen = !m_isMenuOpen;
        }
        
        Controls.IsMuted = m_isMenuOpen;
        SetCusorLock(!m_isMenuOpen);

        if (m_gameOverFadeTime != 0)
        {
            float fade = Mathf.Clamp01((Time.time - (m_gameOverFadeTime + m_gameOverFadeWait)) / m_gameOverFadeDuration);
            m_gameCanvas.GetComponentsInChildren<CanvasRenderer>().ToList().ForEach(r => r.SetAlpha(1 - fade));
            m_gameOverCanvas.GetComponentsInChildren<CanvasRenderer>().ToList().ForEach(r => r.SetAlpha(fade));
            if (fade == 1)
            {
                m_gameOverFadeTime = 0;
            }
        }
    }

    private void OnGameOver()
    {
        if (m_fadeUIOnGameOver)
        {
            m_gameOverFadeTime = Time.time;
        }
    }

    private void SetCusorLock(bool locked)
    {
        m_lockCursor = locked;
        Cursor.visible = !locked;
        Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
    }
}
