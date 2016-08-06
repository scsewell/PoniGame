using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class InterpolationController : MonoBehaviour
{
    private static List<float> m_lastGameplayTimes;
    
    private static float m_interpolationFactor;
    public static float InterpolationFactor
    {
        get { return m_interpolationFactor; }
    }

    private void Start()
    {
        m_lastGameplayTimes = new List<float>();
        m_lastGameplayTimes.Add(Time.time);
        m_lastGameplayTimes.Add(Time.time);
    }

    public void FixedUpdate()
    {
        m_lastGameplayTimes.RemoveAt(0);
        m_lastGameplayTimes.Add(Time.time);
    }

    public void Update()
    {
        float mostRecentTime = m_lastGameplayTimes[m_lastGameplayTimes.Count - 1];
        float nextRecentTime = m_lastGameplayTimes[m_lastGameplayTimes.Count - 2];

        if (mostRecentTime != nextRecentTime)
        {
            m_interpolationFactor = (Time.time - mostRecentTime) / (mostRecentTime - nextRecentTime);
        }
        else
        {
            m_interpolationFactor = 1;
        }
    }
}
