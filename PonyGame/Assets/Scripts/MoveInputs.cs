using UnityEngine;
using System;
using System.Collections.Generic;

public class MoveInputs
{
    private float m_turn = 0;
    public float Turn
    {
        get { return m_turn; }
        set { m_turn = Mathf.Clamp(value, -180, 180); }
    }

    private float m_forward = 0;
    public float Forward
    {
        get { return m_forward; }
        set { m_forward = Mathf.Clamp01(value); }
    }

    private bool m_run = false;
    public bool Run
    {
        get { return m_run; }
        set { m_run = value; }
    }

    private bool m_jump = false;
    public bool Jump
    {
        get { return m_jump; }
        set { m_jump = value; }
    }

    private static bool m_walk = false;

    public static MoveInputs GetPlayerInputs(Transform player)
    {
        MoveInputs inputs = new MoveInputs();

        float x = Controls.AverageValue(GameAxis.MoveX);
        float y = Controls.AverageValue(GameAxis.MoveY);
        Vector3 raw = Vector3.ClampMagnitude(new Vector3(x, 0, y), 1);

        if (raw.magnitude > 0)
        {
            inputs.Turn = Utils.GetBearing(player.forward, Camera.main.transform.rotation * raw, Vector3.up);
            inputs.Forward = 1;
        }

        m_walk = Controls.JustDown(GameButton.WalkToggle) ? !m_walk : m_walk;

        inputs.Run = raw.magnitude > 0.75f && m_walk == Controls.IsDown(GameButton.Walk);
        inputs.Jump = Controls.JustDown(GameButton.Jump);

        return inputs;
    }
}
