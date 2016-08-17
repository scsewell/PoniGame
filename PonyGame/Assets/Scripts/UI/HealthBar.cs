using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class HealthBar : MonoBehaviour
{
    [SerializeField] private Image m_barImage;
    [SerializeField] private Color[] m_colors;
    [SerializeField] private Color m_hurtColor;
    
    [SerializeField]
    [Range(0, 16)]
    private float m_hurtFadeSpeed = 4f;

    private Health m_health;
    private float m_hurtFac;
    
    private void Awake()
    {
        GameController.CharacterChanged += PlayerCharacterChanged;
    }

    private void OnDestroy()
    {
        GameController.CharacterChanged -= PlayerCharacterChanged;
        if (m_health)
        {
            m_health.OnHurt -= Hurt;
        }
    }

    private void PlayerCharacterChanged(Transform newCharacter)
    {
        if (m_health)
        {
            m_health.OnHurt -= Hurt;
        }
        m_health = newCharacter.GetComponent<Health>();
        m_health.OnHurt += Hurt;
        m_hurtFac = 0;
    }

    private void Hurt()
    {
        m_hurtFac = 1;
    }

    private void Update()
    {
        if (m_health)
        {
            SetValue(m_health.HealthFraction);
        }
        m_hurtFac = Mathf.Lerp(m_hurtFac, 0, m_hurtFadeSpeed * Time.deltaTime);
    }

    public void SetValue(float healthFraction)
    {
        healthFraction = Mathf.Clamp01(healthFraction);
        m_barImage.fillAmount = healthFraction;

        float arrayPos = (1 - healthFraction) * (m_colors.Length - 1);
        int firstColorIndex = (int)Mathf.Floor(arrayPos);
        int secondColorIndex = (int)Mathf.Ceil(arrayPos);
        float fac = arrayPos % 1;
        Color healthColor = Color.Lerp(m_colors[firstColorIndex], m_colors[secondColorIndex], fac);

        m_barImage.color = Color.Lerp(healthColor, m_hurtColor, m_hurtFac);
    }
}