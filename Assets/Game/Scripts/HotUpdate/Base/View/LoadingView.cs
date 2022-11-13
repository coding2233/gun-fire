using strange.extensions.mediation.impl;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Wanderer
{
    public class LoadingView : View
    {
        private TextMeshProUGUI m_text;

        public void Setup(AsyncOperation handle)
        {
            m_text = transform.Find("Panel/Text").GetComponent<TextMeshProUGUI>();
            StartCoroutine(ShowLoadingPercent(handle));
        }


        private IEnumerator ShowLoadingPercent(AsyncOperation handle)
        {
            while (!handle.isDone)
            {
                m_text.text = $"Scene Loading {(int)(handle.progress * 100)}%";
                yield return null;
            }

            GameObject.Destroy(gameObject);
        }

    }
}
