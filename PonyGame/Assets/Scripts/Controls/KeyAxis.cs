using UnityEngine;

namespace InputController
{
    /*
     * Stores an axis type input for a pair of keys.
     */
    public class KeyAxis : AxisSource
    {
        private KeyCode m_positive;
        private KeyCode m_negative;

        public KeyAxis(KeyCode positive, KeyCode negative)
        {
            m_positive = positive;
            m_negative = negative;
        }

        // returns the value of the axis
        public float GetValue()
        {
            return GetKeyValue(m_positive) - GetKeyValue(m_negative);
        }

        private float GetKeyValue(KeyCode key)
        {
            return Input.GetKey(key) ? 1 : 0;
        }
    }
}