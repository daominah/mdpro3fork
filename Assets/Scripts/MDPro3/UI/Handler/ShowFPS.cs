using UnityEngine;
using System.Collections;
using DG.Tweening;
using UnityEngine.UI;
using UnityEngine.Events;

namespace MDPro3.UI
{
    public class ShowFPS : MonoBehaviour
    {
        private float m_lastUpdateShowTime = 0f;
        private readonly float m_updateTime = 0.5f;
        private int m_frames = 0;
        private float m_FPS = 0;

        Text m_label;

        private void Start()
        {
            m_lastUpdateShowTime = Time.realtimeSinceStartup;
            m_label = GetComponent<Text>();
        }

        private void Update()
        {
            m_frames++;
            if (Time.realtimeSinceStartup - m_lastUpdateShowTime >= m_updateTime)
            {
                m_FPS = m_frames / (Time.realtimeSinceStartup - m_lastUpdateShowTime);
                m_lastUpdateShowTime = Time.realtimeSinceStartup;
                m_frames = 0;
                m_label.text = ((int)m_FPS).ToString();
            }
        }
    }
}
