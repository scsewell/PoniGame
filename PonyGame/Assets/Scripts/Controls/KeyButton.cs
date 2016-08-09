using UnityEngine;

namespace InputController
{
    class KeyButton : ButtonSource
    {
        private KeyCode m_button;

        public KeyButton(KeyCode button)
        {
            m_button = button;
        }

        public bool IsDown()
        {
            return Input.GetKey(m_button);
        }
    }
}
