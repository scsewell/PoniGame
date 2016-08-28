using UnityEngine;

namespace InputController
{
    /*
     * Stores an axis type input for a pair of keys.
     */
    public class JoystickButtonAxis : AxisSource
    {
        private GamepadButton m_positive;
        private GamepadButton m_negative;
        private float m_multiplier;

        public JoystickButtonAxis(GamepadButton positive, GamepadButton negative, float multiplier)
        {
            m_positive = positive;
            m_negative = negative;
            m_multiplier = multiplier;
        }

        // returns the value of the axis
        public float GetValue()
        {
            return (GetButtonValue(m_positive) - GetButtonValue(m_negative)) * m_multiplier;
        }

        private float GetButtonValue(GamepadButton button)
        {
            return JoystickButton.GetButtonValue(button) ? 1 : 0;
        }
    }
}