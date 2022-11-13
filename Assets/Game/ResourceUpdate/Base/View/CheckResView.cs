using strange.extensions.mediation.impl;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Wanderer
{
    public class CheckResView : View
    {
        private float m_guiScale;

        private string m_error="";
        private string m_downloading = "";

        private Slider m_slider;
        private TextMeshProUGUI m_tipText;

        protected override void Awake()
        {
            base.Awake();

            float widthScale = Screen.width / 1344.0f;
            float heightScale = Screen.height / 750f;

            m_guiScale = Mathf.Max(widthScale, heightScale);

            m_slider = transform.Find("Panel/Slider").GetComponent<Slider>();
            m_tipText = transform.Find("Panel/Text").GetComponent<TextMeshProUGUI>();
            m_tipText.text = "Update Resource";
            m_slider.value = 0;
            m_slider.gameObject.SetActive(true);
        }

        public void OnDownloadProgress(float percentage, double downloadKBSize, double downloadSpeed, float remainingTime)
        {
            m_slider.value = percentage;
            string downloadSize = downloadKBSize > 1024 ? $"{(downloadKBSize / 1024.0).ToString("f2")}MB" : $"{(int)downloadKBSize}KB";

            double cureentDownloadKBSize = downloadKBSize * percentage;
            string currentDownloadSize = cureentDownloadKBSize > 1024 ? $"{(cureentDownloadKBSize / 1024.0).ToString("f2")}MB" : $"{(int)cureentDownloadKBSize}KB";



            m_downloading = $"正在下载[{currentDownloadSize}/{downloadSize}] 速度:{downloadSpeed.ToString("f2")}KB/s 剩余时间:{(int)remainingTime}s";

            m_tipText.text = m_downloading;
        }

        public void OnDownloadError(string s1, string s2)
        {
            m_error = s1+ " " + s2;

            m_tipText.text = $"<color=red>Error {s1} {s2}</color>";
        }

   
        //private void OnGUI()
        //{
        //    Matrix4x4 lastMatrix = GUI.matrix;
        //    GUI.matrix = Matrix4x4.Scale(new Vector3(m_guiScale, m_guiScale, 1f));
        //    GUILayout.Label("Download...."+ m_error);
        //    GUILayout.Label(m_downloading);

        //    GUI.matrix = lastMatrix;

        //}


    }
}