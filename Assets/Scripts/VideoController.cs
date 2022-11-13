using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VideoController : MonoBehaviour
{
    private Transform m_btnCenterPlay;
    private Transform m_btnBottom;
    private Transform m_btnPlay;
    private Transform m_btnPause;
    private Transform m_btnZoom;
    private Slider m_slider;
    private RawImage m_rawImage;

    private Action<int> _onCtrlCallback;

    private bool m_isFull;
   


    public void Setup(Action<int> onCtrlCallback,bool waitForReady,bool isPlaying,float sliderValue,bool isFull=false)
    {
        if (m_rawImage == null)
        {
            m_rawImage = transform.Find("RawImage").GetComponent<RawImage>();
            m_btnCenterPlay = transform.Find("VideoController/Play");
            m_btnBottom = transform.Find("VideoController/Bottom");
            m_btnPlay = transform.Find("VideoController/Bottom/Play");
            m_btnPause = transform.Find("VideoController/Bottom/Pause");
            m_btnZoom = transform.Find("VideoController/Bottom/ZoomIn");
            m_slider = transform.Find("VideoController/Bottom/Slider").GetComponent<Slider>();


            m_btnCenterPlay.GetComponent<Button>().onClick.AddListener(() => { OnButtonClick(0); });
            m_btnPlay.GetComponent<Button>().onClick.AddListener(() => { OnButtonClick(0); });
            m_btnPause.GetComponent<Button>().onClick.AddListener(() => { OnButtonClick(1); });
            m_btnZoom.GetComponent<Button>().onClick.AddListener(() => { OnButtonClick(2); });
        }

        m_isFull = isFull;
         

        if (m_isFull)
        {
            m_btnCenterPlay.gameObject.SetActive(false);
            m_btnBottom.gameObject.SetActive(true);
        }
        else
        {
            m_btnCenterPlay.gameObject.SetActive(waitForReady);
            m_btnBottom.gameObject.SetActive(!waitForReady);
        }

        m_btnPlay.gameObject.SetActive(!isPlaying);
        m_btnPause.gameObject.SetActive(isPlaying);
        m_rawImage.rectTransform.sizeDelta = GetComponent<RectTransform>().sizeDelta;
        m_slider.value = sliderValue;

        _onCtrlCallback = onCtrlCallback;
    }

    public void ResetUI()
    {
        m_rawImage.texture = null;
       
        if (m_isFull)
        {
            m_btnCenterPlay.gameObject.SetActive(false);
            m_btnBottom.gameObject.SetActive(true);
        }
        else
        {
            m_btnCenterPlay.gameObject.SetActive(true);
            m_btnBottom.gameObject.SetActive(false);
        }
    }

    public void SetSlider(float value)
    {
        m_slider.value = value;
    }

    private void OnButtonClick(int index)
    {
        switch (index)
        {
            case 0:
                m_btnCenterPlay.gameObject.SetActive(false);

                m_btnPlay.gameObject.SetActive(false);
                m_btnPause.gameObject.SetActive(true);
                m_btnBottom.gameObject.SetActive(true);
                break;
            case 1:
                m_btnPlay.gameObject.SetActive(true);
                m_btnPause.gameObject.SetActive(false);
                break;
            default:
                break;
        }

        _onCtrlCallback?.Invoke(index);
    }


    private void Adpter(int videoWidth, int videoHeight)
    {
        var size = GetComponent<RectTransform>().sizeDelta;
        if ((videoWidth / (float)videoHeight) >= (size.x / size.y))
        {
            float interval = videoHeight / (float)videoWidth;
            float height = interval * size.x;
            m_rawImage.rectTransform.sizeDelta = new Vector2(size.x, height);
        }
        else
        {
            float interval = videoWidth / (float)videoHeight;
            float width = interval * size.y;
            m_rawImage.rectTransform.sizeDelta = new Vector2(width, size.y);
        }
    }


    public void UpdateTexture(Texture texture)
    {
        if (m_rawImage != null)
        {
            if (m_rawImage.texture == null && texture != null)
            {
                Adpter(texture.width, texture.height);
            }
            m_rawImage.texture = texture;
        }
    }


}
