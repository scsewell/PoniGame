using UnityEngine;

namespace InputController
{
    public class JoystickButton : ButtonSource
    {
        private const float MAIN_THRESHOLD = 0.5f;
        private const float TRIGGER_THRESHOLD = 0.3f;

        public GamepadButton m_button;

        public JoystickButton(GamepadButton button)
        {
            m_button = button;
        }
    
        public bool IsDown()
        {
            return GetButtonValue(m_button);
        }

        public static bool GetButtonValue(GamepadButton button)
        {
            switch (button)
            {
                case GamepadButton.A:              return Input.GetKey(KeyCode.JoystickButton0);
                case GamepadButton.B:              return Input.GetKey(KeyCode.JoystickButton1);
                case GamepadButton.X:              return Input.GetKey(KeyCode.JoystickButton2);
                case GamepadButton.Y:              return Input.GetKey(KeyCode.JoystickButton3);
                case GamepadButton.RShoulder:      return Input.GetKey(KeyCode.JoystickButton5);
                case GamepadButton.LShoulder:      return Input.GetKey(KeyCode.JoystickButton4);
                case GamepadButton.Back:           return Input.GetKey(KeyCode.JoystickButton6);
                case GamepadButton.Start:          return Input.GetKey(KeyCode.JoystickButton7);
                case GamepadButton.LStick:         return Input.GetKey(KeyCode.JoystickButton8);
                case GamepadButton.RStick:         return Input.GetKey(KeyCode.JoystickButton9);
            
                case GamepadButton.LTrigger:       return Input.GetAxis("TriggersL") > TRIGGER_THRESHOLD;
                case GamepadButton.RTrigger:       return Input.GetAxis("TriggersR") > TRIGGER_THRESHOLD;

                case GamepadButton.DpadLeft:       return OverThreshold(GamepadAxis.DpadX, -MAIN_THRESHOLD);
                case GamepadButton.DpadRight:      return OverThreshold(GamepadAxis.DpadX, MAIN_THRESHOLD);
                case GamepadButton.DpadUp:         return OverThreshold(GamepadAxis.DpadY, MAIN_THRESHOLD);
                case GamepadButton.DpadDown:       return OverThreshold(GamepadAxis.DpadY, -MAIN_THRESHOLD);

                case GamepadButton.LStickLeft:     return OverThreshold(GamepadAxis.LStickX, -MAIN_THRESHOLD);
                case GamepadButton.LStickRight:    return OverThreshold(GamepadAxis.LStickX, MAIN_THRESHOLD);
                case GamepadButton.LStickUp:       return OverThreshold(GamepadAxis.LStickY, MAIN_THRESHOLD);
                case GamepadButton.LStickDown:     return OverThreshold(GamepadAxis.LStickY, -MAIN_THRESHOLD);

                case GamepadButton.RStickLeft:     return OverThreshold(GamepadAxis.RStickX, -MAIN_THRESHOLD);
                case GamepadButton.RStickRight:    return OverThreshold(GamepadAxis.RStickX, MAIN_THRESHOLD);
                case GamepadButton.RStickUp:       return OverThreshold(GamepadAxis.RStickY, MAIN_THRESHOLD);
                case GamepadButton.RStickDown:     return OverThreshold(GamepadAxis.RStickY, -MAIN_THRESHOLD);
            }
            return false;
        }

        private static bool OverThreshold(GamepadAxis axis, float threshold)
        {
            return JoystickAxis.GetAxisValue(axis) / threshold > 1;
        }
    }
}