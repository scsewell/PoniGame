using UnityEngine;

namespace InputController
{
    /*
     * Stores an axis type input for the mouse.
     */
    public class MouseAxis : AxisSource
    {
        public enum Axis { None, ScrollWheel, MouseX, MouseY }
        public Axis axis;

        //private float m_scale = 1;
        //private float m_lastVelocity = 0;

        public MouseAxis(Axis axis)
        {
            this.axis = axis;
        }

        // returns the value of the relevant axis
        public float GetValue()
        {
            return GetAxis(axis);
        }

        public float GetAxis(Axis mouseAxis)
        {
            switch (mouseAxis)
            {
                case Axis.ScrollWheel: return Input.GetAxis("Mouse ScrollWheel") * 0.8f / Time.deltaTime;
                case Axis.MouseX: return (Input.GetAxis("Mouse X") * 0.008f / Time.deltaTime);
                case Axis.MouseY: return Input.GetAxis("Mouse Y") * 0.008f / Time.deltaTime;
            }
            return 0;
        }

        //public void RecordState()
        //{
        //    m_scale = Time.fixedDeltaTime / Time.deltaTime;
        //    m_lastVelocity = GetAxis(axis);
        //}
    }
}