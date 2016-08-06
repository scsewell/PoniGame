using UnityEngine;

namespace InputController
{
    public class JoystickButton : ButtonSource
    {
        public const float MAIN_THRESHOLD = 0.5f;
        public const float TRIGGER_THRESHOLD = 0.3f;

        public GamepadButton button;

        public JoystickButton(GamepadButton button)
        {
            this.button = button;
        }
    
        public bool IsDown()
        {
            return GetButton(button);
        }

        public static bool GetButton(GamepadButton button)
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
                case GamepadButton.DpadUp:         return JoystickAxis.GetAxis(GamepadAxis.DpadY) > MAIN_THRESHOLD;
                case GamepadButton.DpadDown:       return JoystickAxis.GetAxis(GamepadAxis.DpadY) < -MAIN_THRESHOLD;
                case GamepadButton.DpadLeft:       return JoystickAxis.GetAxis(GamepadAxis.DpadX) < -MAIN_THRESHOLD;
                case GamepadButton.DpadRight:      return JoystickAxis.GetAxis(GamepadAxis.DpadX) > MAIN_THRESHOLD;
                case GamepadButton.LStickUp:       return JoystickAxis.GetAxis(GamepadAxis.LStickY) < -MAIN_THRESHOLD;
                case GamepadButton.LStickDown:     return JoystickAxis.GetAxis(GamepadAxis.LStickY) > MAIN_THRESHOLD;
                case GamepadButton.LStickLeft:     return JoystickAxis.GetAxis(GamepadAxis.LStickX) < -MAIN_THRESHOLD;
                case GamepadButton.LStickRight:    return JoystickAxis.GetAxis(GamepadAxis.LStickX) > MAIN_THRESHOLD;
                case GamepadButton.RStickUp:       return JoystickAxis.GetAxis(GamepadAxis.RStickY) < -MAIN_THRESHOLD;
                case GamepadButton.RStickDown:     return JoystickAxis.GetAxis(GamepadAxis.RStickY) > MAIN_THRESHOLD;
                case GamepadButton.RStickLeft:     return JoystickAxis.GetAxis(GamepadAxis.RStickX) < -MAIN_THRESHOLD;
                case GamepadButton.RStickRight:    return JoystickAxis.GetAxis(GamepadAxis.RStickX) > MAIN_THRESHOLD;
            }
            return false;
        }
    }
}