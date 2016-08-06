using UnityEngine;
using UnityStandardAssets.ImageEffects;
using UnityStandardAssets.CinematicEffects;

/*
 * Manages settings for the game
 */
public class Settings : MonoBehaviour
{
    public enum VideoQ
    {
        Low,
        High,
    }

    private VideoQ m_videoQuality;
    public VideoQ VideoQuality
    {
        get { return m_videoQuality; }
    }


    void Start()
    {
        m_videoQuality = (VideoQ)QualitySettings.GetQualityLevel();
	}

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1))
        {
            SetVideoQuality(VideoQ.Low);
        }
        if (Input.GetKeyDown(KeyCode.F2))
        {
            SetVideoQuality(VideoQ.High);
        }
    }
    
    /*
     * Sets the graphics quality for the game and applies them
     */
    private void SetVideoQuality(VideoQ quality)
    {
        m_videoQuality = quality;

        QualitySettings.SetQualityLevel((int)quality);
        Application.targetFrameRate = quality == VideoQ.High ? 30 : 999;

        if (Camera.main.GetComponent<AntiAliasing>())
        {
            Camera.main.GetComponent<AntiAliasing>().enabled = quality == VideoQ.Low;
        }

        if (Camera.main.GetComponent<SunShafts>())
        {
            Camera.main.GetComponent<SunShafts>().enabled = quality == VideoQ.High;
        }

        if (Camera.main.GetComponent<UnityStandardAssets.CinematicEffects.Bloom>())
        {
            Camera.main.GetComponent<UnityStandardAssets.CinematicEffects.Bloom>().enabled = quality == VideoQ.High;
        }
    }
}
