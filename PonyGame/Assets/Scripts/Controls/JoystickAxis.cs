using UnityEngine;

namespace InputController
{
    /*
     * Stores an axis type input for a joystick.
     */
    public class JoystickAxis : AxisSource
    {
        private GamepadAxis m_axis;
        private float m_exponent;
        private float m_multiplier;

        public JoystickAxis(GamepadAxis axis, float exponent, float multiplier)
        {
            m_axis = axis;
            m_exponent = exponent;
            m_multiplier = multiplier;
        }

        // returns the value of the relevant axis, and applies an exponent while preserving the +/-
        public float GetValue()
        {
            float value = GetAxisValue(m_axis);
            return Mathf.Sign(value) * Mathf.Pow(Mathf.Abs(value), m_exponent) * m_multiplier;
        }

        public static float GetAxisValue(GamepadAxis axis)
        {
            switch (axis)
            {
                case GamepadAxis.DpadX:
                    return Input.GetAxis("DPad_XAxis");
                case GamepadAxis.DpadY:
                    return -Input.GetAxis("DPad_YAxis");
                case GamepadAxis.LStickX:
                    return Input.GetAxis("L_XAxis");
                case GamepadAxis.LStickY:
                    return -Input.GetAxis("L_YAxis");
                case GamepadAxis.RStickX:
                    return Input.GetAxis("R_XAxis");
                case GamepadAxis.RStickY:
                    return -Input.GetAxis("R_YAxis");
                case GamepadAxis.Triggers:
                    //*
                    return Input.GetAxis("Triggers");
                    /*/
                    float LTrigger = Input.GetAxis("TriggersL");
                    float RTrigger = Input.GetAxis("TriggersR");
                    return RTrigger - LTrigger;
                    //*/
            }
            return 0;
        }
    }
}