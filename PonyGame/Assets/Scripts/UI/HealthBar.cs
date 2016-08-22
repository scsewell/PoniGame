using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class HealthBar : MonoBehaviour
{
    [SerializeField] private RectTransform m_damageBarPrefab;
    [SerializeField] private RectTransform m_damageBarParent;
    [SerializeField] private Image m_mainBarImage;
    [SerializeField] private Color[] m_colors;
    [SerializeField] private Color m_hurtColor;
    
    [SerializeField]
    [Range(0, 16)]
    private float m_hurtFadeSpeed = 4f;

    [SerializeField]
    [Range(0, 2)]
    private float m_damageWaitTime = 1f;

    [SerializeField]
    [Range(0, 2)]
    private float m_damageFadeTime = 1f;


    private Health m_health;
    private Dictionary<Image, float> m_damageBars;
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
        m_damageBars = new Dictionary<Image, float>();
        foreach (Transform child in m_damageBarParent)
        {
            Destroy(child.gameObject);
        }
    }

    private void Hurt(float healthFractionLost)
    {
        m_hurtFac = 1;

        RectTransform damageBar = Instantiate(m_damageBarPrefab, m_damageBarParent) as RectTransform;
        damageBar.localScale = Vector3.one;
        damageBar.localPosition = Vector2.zero;
        damageBar.anchoredPosition = Vector2.zero;
        damageBar.sizeDelta = Vector2.zero;
        damageBar.anchorMin = Vector2.zero;
        damageBar.anchorMax = Vector2.one;

        Image damageImage = damageBar.GetComponent<Image>();
        damageImage.fillAmount = m_mainBarImage.fillAmount;
        m_damageBars.Add(damageImage, Time.time);
    }

    private void Update()
    {
        if (m_health)
        {
            SetHeath(m_health.HealthFraction);
        }

        m_hurtFac = Mathf.Lerp(m_hurtFac, 0, m_hurtFadeSpeed * Time.deltaTime);

        List<Image> damageBars = new List<Image>(m_damageBars.Keys);
        foreach (Image damageBar in damageBars)
        {
            float alpha = 1f - ((Time.time - (m_damageBars[damageBar] + m_damageWaitTime)) / m_damageFadeTime);
            if (alpha > 0)
            {
                SetAlpha(damageBar, alpha);
            }
            else
            {
                m_damageBars.Remove(damageBar);
                Destroy(damageBar.gameObject);
            }
        }
    }

    public void SetHeath(float healthFraction)
    {
        healthFraction = Mathf.Clamp01(healthFraction);
        m_mainBarImage.fillAmount = healthFraction;

        float arrayPos = (1 - healthFraction) * (m_colors.Length - 1);
        int firstColorIndex = (int)Mathf.Floor(arrayPos);
        int secondColorIndex = (int)Mathf.Ceil(arrayPos);
        float fac = arrayPos % 1;
        Color healthColor = Color.Lerp(m_colors[firstColorIndex], m_colors[secondColorIndex], fac);

        m_mainBarImage.color = Color.Lerp(healthColor, m_hurtColor, m_hurtFac);
    }

    private void SetAlpha(Image image, float alpha)
    {
        Color col = image.color;
        col.a = Mathf.Clamp01(alpha);
        image.color = col;
    }
}