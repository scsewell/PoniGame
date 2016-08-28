using UnityEngine;

namespace InputController
{
    /*
     * Stores an axis type input for the mouse.
     */
    public class MouseAxis : AxisSource
    {
        public enum Axis { None, ScrollWheel, MouseX, MouseY }

        private Axis m_axis;
        private float m_threshold;

        public MouseAxis(Axis axis, float threshold = 0)
        {
            m_axis = axis;
            m_threshold = threshold;
        }

        // returns the value of the relevant axis
        public float GetValue()
        {
            return GetAxisValue(m_axis);
        }

        private float GetAxisValue(Axis mouseAxis)
        {
            switch (mouseAxis)
            {
                case Axis.ScrollWheel: return ThresholdValue(Input.GetAxis("Mouse ScrollWheel")) * 0.08f / Time.deltaTime;
                case Axis.MouseX: return ThresholdValue(Input.GetAxis("Mouse X")) * 0.008f / Time.deltaTime;
                case Axis.MouseY: return ThresholdValue(Input.GetAxis("Mouse Y")) * 0.008f / Time.deltaTime;
            }
            return 0;
        }

        private float ThresholdValue(float value)
        {
            return Mathf.Abs(value) > m_threshold ? value : 0;
        }
    }
}