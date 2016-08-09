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
        Medium,
        High,
    }

    private VideoQ m_videoQuality = VideoQ.High;
    public VideoQ VideoQuality
    {
        get { return m_videoQuality; }
    }


    void Start()
    {
        SetVideoQuality(m_videoQuality);
	}

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1))
        {
            SetVideoQuality(VideoQ.Low);
        }
        if (Input.GetKeyDown(KeyCode.F2))
        {
            SetVideoQuality(VideoQ.Medium);
        }
        if (Input.GetKeyDown(KeyCode.F3))
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
        Application.targetFrameRate = quality == VideoQ.High ? 999 : 999;

        Camera cam = Camera.main;
        SunShafts sunShafts = cam.GetComponent<SunShafts>();
        Bloom bloom = cam.GetComponent<Bloom>();
        CameraMotionBlur motionBlur = cam.GetComponent<CameraMotionBlur>();
        AntiAliasing antiAliasing = cam.GetComponent<AntiAliasing>();

        if (sunShafts)
        {
            sunShafts.enabled = quality != VideoQ.Low;
        }
        if (bloom)
        {
            bloom.enabled = quality != VideoQ.Low;
            bloom.settings.highQuality = quality == VideoQ.High;
        }
        if (motionBlur)
        {
            motionBlur.enabled = quality != VideoQ.Low;
            motionBlur.velocityDownsample = quality == VideoQ.High ? 1 : 2;
        }
        if (antiAliasing)
        {
            antiAliasing.enabled = quality == VideoQ.Low;
        }
    }
}
