using UnityEngine;
using System;
using System.Collections;

public class Health : MonoBehaviour
{
    public float maxHealth = 100.0f;

    private float m_health;
    public float CurrentHealth
    {
        get { return m_health; }
    }

    public delegate void HealthChangedHandler(float healthChange);
    public event HealthChangedHandler HealthChanged;

    public delegate void DiedHandler();
    public event DiedHandler Die;

    // Use this for initialization
    void Start ()
    {
        m_health = maxHealth;
	}

    public void IncrimentHealth(float delta)
    {
        ModifyHealth(Mathf.Clamp(m_health + delta, 0, maxHealth) - m_health);
    }

    public void SetHealth(float newHealth)
    {
        ModifyHealth(Mathf.Clamp(newHealth, 0, maxHealth) - m_health);
    }

    private void ModifyHealth(float healthChange)
    {
        if (healthChange != 0)
        {
            m_health += healthChange;

            if (Die != null && m_health == 0)
            {
                Die();
            }

            if (HealthChanged != null)
            {
                HealthChanged(healthChange);
            }
        }
    }
}
