using UnityEngine;
using System.Collections;

public class Health : MonoBehaviour
{
    [SerializeField] private float m_maxHealth = 100;

    private float m_currentHealth;

    public delegate void DeathHandler();
    public event DeathHandler OnDie;

    public delegate void HurtHandler();
    public event HurtHandler OnHurt;

    private void Start()
    {
        m_currentHealth = m_maxHealth;
    }

    public bool IsAlive
    {
        get { return m_currentHealth > 0; }
    }

    public float CurrentHealth
    {
        get { return m_currentHealth; }
    }

    public float MaxHealth
    {
        get { return m_maxHealth; }
    }

    public float HealthFraction
    {
        get { return m_currentHealth / m_maxHealth; }
    }

    public void ApplyDamage(float damage)
    {
        if (!IsAlive)
        {
            return;
        }
        m_currentHealth = Mathf.Max(m_currentHealth - damage, 0);
        if (OnHurt != null)
        {
            OnHurt();
        }
        if (OnDie != null && !IsAlive)
        {
            OnDie();
        }
    }

    public void Heal(float healthGained)
    {
        if (!IsAlive)
        {
            return;
        }
        m_currentHealth = Mathf.Min(m_currentHealth + healthGained, m_maxHealth);
    }
}
