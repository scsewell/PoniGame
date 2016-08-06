using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace InputController
{
    /*
     * Stores all the mouse and joystick axes that are relevant to a specific in game command.
     */
    public class BufferedAxis
    {
        private List<AxisSource> m_sources;
        private List<List<Dictionary<AxisSource, float>>> m_buffers;

        public BufferedAxis(List<AxisSource> sources)
        {
            m_sources = new List<AxisSource>(sources);

            m_buffers = new List<List<Dictionary<AxisSource, float>>>();
            m_buffers.Add(new List<Dictionary<AxisSource, float>>());
            m_buffers.Last().Add(new Dictionary<AxisSource, float>());
            foreach (AxisSource source in m_sources)
            {
                m_buffers.Last().Last().Add(source, source.GetValue());
            }
            m_buffers.Add(new List<Dictionary<AxisSource, float>>());
        }

        /*
         * Returns the average value of the axes over the last gamplay update frame, or the last visual update.
         */
        public float AverageValue()
        {
            return GetRelevantInput().Average((visualUpdateInputs) => (visualUpdateInputs.Values.Sum()));
        }

        /*
         * Returns the cumulative value of the axes over the last gamplay update frame, or the last visual update.
         */
        public float CumulativeValue()
        {
            return GetRelevantInput().Sum((visualUpdateInputs) => (visualUpdateInputs.Values.Sum()));
        }

        /*
         * Run at the end of every visual update frame to record the input state for that frame.
         */
        public void RecordUpdateState()
        {
            m_buffers.Last().Add(new Dictionary<AxisSource, float>());

            foreach (AxisSource source in m_sources)
            {
                m_buffers.Last().Last().Add(source, source.GetValue());
            }
        }

        /*
         * Run at the end of every fixed update to remove old inputs from buffers.
         */
        public void RecordFixedUpdateState()
        {
            // ensures there are inputs from two visual updates and inputs from two fixed updates in the buffer
            while (m_buffers.GetRange(1, m_buffers.Count - 1).Sum((gameplayUpdateInputs) => (gameplayUpdateInputs.Count)) >= 1)
            {
                m_buffers.RemoveAt(0);
            }
            m_buffers.Add(new List<Dictionary<AxisSource, float>>());
        }

        private List<Dictionary<AxisSource, float>> GetRelevantInput()
        {
            List<Dictionary<AxisSource, float>> buffer = new List<Dictionary<AxisSource, float>>();
            if (m_buffers.Last().Count == 0)
            {
                buffer.Add(m_buffers.GetRange(0, m_buffers.Count - 1).Last((gameplayUpdate) => (gameplayUpdate.Any())).Last());
            }
            return buffer.Concat(m_buffers.Last()).ToList();
        }
    }
}