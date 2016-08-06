using UnityEngine;

namespace InputController
{
    class KeyButton : ButtonSource
    {
        public KeyCode button;

        public KeyButton(KeyCode button)
        {
            this.button = button;
        }

        public bool IsDown()
        {
            return Input.GetKey(button);
        }
    }
}
