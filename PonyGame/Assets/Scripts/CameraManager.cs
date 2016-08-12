using UnityEngine;
using UnityStandardAssets.ImageEffects;
using UnityStandardAssets.CinematicEffects;
using System.Collections;

public class CameraManager : MonoBehaviour
{
    [Tooltip("How quickly the screen fades towards grey when the player dies")]
    [SerializeField]
    [Range(0, 4)]
    private float m_fadeToGreyRate = 1.0f;

    [SerializeField]
    [Range(-1, 0)]
    private float m_greyVibrance = -0.5f;

    [SerializeField]
    [Range(0, 1)]
    private float m_greySaturation = 0.5f;

    [SerializeField]
    [Range(0, 1)]
    private float m_greyValue = 0.65f;


    private Camera m_cam;
    private SunShafts m_sunShafts;
    private Bloom m_bloom;
    private CameraMotionBlur m_motionBlur;
    private AntiAliasing m_antiAliasing;
    private TonemappingColorGrading m_colorGrading;

    private TonemappingColorGrading.ColorGradingSettings m_defaultGrading;


    private static bool m_fadeToGrey = false;
    public static bool FadeToGrey
    {
        get { return m_fadeToGrey; }
        set { m_fadeToGrey = value; }
    }

    void Start()
    {
        m_cam = Camera.main;
        m_sunShafts = m_cam.GetComponent<SunShafts>();
        m_bloom = m_cam.GetComponent<Bloom>();
        m_motionBlur = m_cam.GetComponent<CameraMotionBlur>();
        m_antiAliasing = m_cam.GetComponent<AntiAliasing>();
        m_colorGrading = m_cam.GetComponent<TonemappingColorGrading>();

        m_defaultGrading = m_colorGrading.colorGrading;
    }

    void Update()
    {
        if (m_sunShafts)
        {
            m_sunShafts.enabled = Settings.bloom != Settings.Bloom.Off;
        }
        if (m_bloom)
        {
            m_bloom.enabled = Settings.bloom != Settings.Bloom.Off;
            m_bloom.settings.highQuality = Settings.bloom == Settings.Bloom.High;
        }
        if (m_motionBlur)
        {
            m_motionBlur.enabled = Settings.motionBlur != Settings.MotionBlur.Off;
            m_motionBlur.velocityDownsample = Settings.motionBlur == Settings.MotionBlur.High ? 1 : 2;
        }
        if (m_antiAliasing)
        {
            m_antiAliasing.enabled = Settings.antialiasing == Settings.Antialiasing.Low;
        }

        if (m_colorGrading)
        {
            TonemappingColorGrading.ColorGradingSettings colorSettings = m_colorGrading.colorGrading;
            if (m_fadeToGrey)
            {
                float lerpFac = m_fadeToGreyRate * Time.deltaTime;
                colorSettings.basics.saturation =   Mathf.Lerp(colorSettings.basics.saturation, m_greySaturation,   lerpFac);
                colorSettings.basics.vibrance =     Mathf.Lerp(colorSettings.basics.vibrance,   m_greyVibrance,     lerpFac);
                colorSettings.basics.value =        Mathf.Lerp(colorSettings.basics.value,      m_greyValue,        lerpFac);
            }
            else
            {
                colorSettings.basics.saturation = m_defaultGrading.basics.saturation;
                colorSettings.basics.vibrance = m_defaultGrading.basics.vibrance;
                colorSettings.basics.value = m_defaultGrading.basics.value;
            }
            m_colorGrading.colorGrading = colorSettings;
        }
    }
}
