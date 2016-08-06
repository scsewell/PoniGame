using UnityEngine;

namespace InputController
{
    /*
     * Stores an axis type input for a joystick.
     */
    public class JoystickAxis : AxisSource
    {
        public GamepadAxis axis;
        public float exponent;
        public float multiplier;

        public JoystickAxis(GamepadAxis axis, float exponent, float multiplier)
        {
            this.axis = axis;
            this.exponent = exponent;
            this.multiplier = multiplier;
        }

        // returns the value of the relevant axis, and applies an exponent while preserving the +/-
        public float GetValue()
        {
            float value = GetAxis(axis);
            return Mathf.Sign(value) * Mathf.Pow(Mathf.Abs(value), exponent) * multiplier;
        }

        public static float GetAxis(GamepadAxis axis)
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
                    float LTrigger = Input.GetAxis("TriggersL");
                    float RTrigger = Input.GetAxis("TriggersR");
                    return RTrigger - LTrigger;
            }
            return 0;
        }
    }
}