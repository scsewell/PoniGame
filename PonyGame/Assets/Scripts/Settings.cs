using UnityEngine;

/*
 * Manages settings for the game
 */
public class Settings : MonoBehaviour
{
    public static Resolution resolution;
    public static bool fullscreen;

    public enum Antialiasing { Off, Low, Medium, High, Ultra }
    public static Antialiasing antialiasing;

    public enum Shadows { Off, Low, High }
    public static Shadows shadows;

    public enum Bloom { Off, Low, High }
    public static Bloom bloom;
    
    public enum MotionBlur { Off, Low, High }
    public static MotionBlur motionBlur;
    
    private void Awake()
    {
        LoadDefaults();
        ApplySettings();
    }

    private static void LoadDefaults()
    {
        resolution = Screen.currentResolution;
        fullscreen = Screen.fullScreen;
        antialiasing = Antialiasing.Ultra;
        shadows = Shadows.High;
        bloom = Bloom.High;
        motionBlur = MotionBlur.High;
    }

    private static void ApplySettings()
    {
        Resolution current = Screen.currentResolution;
        if (resolution.width != current.width || resolution.height != current.height || fullscreen != Screen.fullScreen)
        {
            Screen.SetResolution(resolution.width, resolution.height, fullscreen);
        }

        //Application.targetFrameRate = 999;
        //QualitySettings.vSyncCount = 1;

        QualitySettings.SetQualityLevel((int)shadows);

        switch (antialiasing)
        {
            case Antialiasing.Ultra:    QualitySettings.antiAliasing = 8; break;
            case Antialiasing.High:     QualitySettings.antiAliasing = 4; break;
            case Antialiasing.Medium:   QualitySettings.antiAliasing = 2; break;
            default:                    QualitySettings.antiAliasing = 0; break;
        }
    }
}