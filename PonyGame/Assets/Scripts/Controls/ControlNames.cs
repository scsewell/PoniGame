namespace InputController
{
    public class ControlNames
    {
        public string GetName(GamepadButton button)
        {
            switch (button)
            {
                case GamepadButton.A:               return "A";
                case GamepadButton.B:               return "B";
                case GamepadButton.X:               return "X";
                case GamepadButton.Y:               return "Y";
                case GamepadButton.RShoulder:       return "R Bumper";
                case GamepadButton.LShoulder:       return "L Bumper";
                case GamepadButton.Back:            return "Back";
                case GamepadButton.Start:           return "Start";
                case GamepadButton.LStick:          return "L Stick";
                case GamepadButton.RStick:          return "R Stick";

                case GamepadButton.LTrigger:        return "L Trigger";
                case GamepadButton.RTrigger:        return "R Trigger";
                case GamepadButton.DpadUp:          return "Dpad Up";
                case GamepadButton.DpadDown:        return "Dpad Down";
                case GamepadButton.DpadLeft:        return "Dpad Left";
                case GamepadButton.DpadRight:       return "Dpad Right";
                case GamepadButton.LStickUp:        return "L Stick Up";
                case GamepadButton.LStickDown:      return "L Stick Down";
                case GamepadButton.LStickLeft:      return "L Stick Left";
                case GamepadButton.LStickRight:     return "L Stick Right";
                case GamepadButton.RStickUp:        return "R Stick Up";
                case GamepadButton.RStickDown:      return "R Stick Down";
                case GamepadButton.RStickLeft:      return "R Stick Left";
                case GamepadButton.RStickRight:     return "R Stick Right";
            }
            return "None";
        }

        public string GetName(GamepadAxis axis)
        {
            switch (axis)
            {
                case GamepadAxis.LStickX:           return "L Stick X";
                case GamepadAxis.LStickY:           return "L Stick Y";
                case GamepadAxis.RStickX:           return "R Stick X";
                case GamepadAxis.RStickY:           return "R Stick Y";
                case GamepadAxis.DpadX:             return "Dpad X";
                case GamepadAxis.DpadY:             return "Dpad Y";
                case GamepadAxis.Triggers:          return "Triggers";
            }
            return "None";
        }

        public string GetName(MouseAxis.Axis axis)
        {
            switch (axis)
            {
                case MouseAxis.Axis.ScrollWheel:    return "ScrollWheel";
                case MouseAxis.Axis.MouseX:         return "Mouse X";
                case MouseAxis.Axis.MouseY:         return "Mouse Y";
            }
            return "None";
        }
    }
}