using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class VideoPlayer02 : MonoBehaviour
{
    private VideoPlayer m_videoPlayer;
    private AudioSource m_audioSource;
    private bool m_isPause;
    private bool m_waitForReady=true;
    VideoController m_videoController;
    VideoController m_videoFullController;

    VideoController m_videoSelect= null;

    // Start is called before the first frame update
    void Start()
    {
        //m_rawImage = GetComponent<RawImage>();
        m_videoPlayer = gameObject.AddComponent<VideoPlayer>();
        m_audioSource = gameObject.AddComponent<AudioSource>();
        
        m_videoPlayer.playOnAwake = false;
        m_audioSource.playOnAwake = false;

        m_videoPlayer.isLooping = true;

        m_videoPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;
        m_videoPlayer.controlledAudioTrackCount = 1;
        m_videoPlayer.SetTargetAudioSource(0, m_audioSource);
       
        m_videoPlayer.url = "http://dldir3.qq.com/minigamefile/terminal/MJResTest/Video/test1.mp4";
        //m_videoPlayer.frameReady += M_videoPlayer_frameReady;
        //m_videoPlayer.prepareCompleted += M_videoPlayer_prepareCompleted;
        //http://dldir3.qq.com/minigamefile/terminal/MJResTest/Video/2-skiil_2_content.mp4
        //http://dldir3.qq.com/minigamefile/terminal/MJResTest/Video/test1.mp4

       var canvas = GameObject.Find("Canvas");
        var fullVideoController = canvas.transform.Find("FullVideoPanel").gameObject.AddComponent<VideoController>();
        SetVideoPlayTarget(transform, fullVideoController);

        Prepare();
    }



    //private void OnEnable()
    //{
    //    if (m_rawImage != null)
    //    {
    //        m_rawImage.texture = m_cacheTexture;
    //    }
    //}

    public void SetVideoPlayTarget(Transform target, VideoController videoFullController)
    {
        GameObject videoCtrl = GameObject.Instantiate(Resources.Load<GameObject>("VideoPanel"));
        videoCtrl.transform.SetParent(target);
        videoCtrl.transform.localScale = Vector3.one;
        videoCtrl.transform.localPosition = Vector3.zero;
        m_videoController = videoCtrl.AddComponent<VideoController>();
        m_videoController.GetComponent<RectTransform>().sizeDelta = target.GetComponent<RectTransform>().sizeDelta;

        m_videoFullController = videoFullController;

        ChangeVideoConteroller();
    }


    private void ChangeVideoConteroller()
    {
        bool oldIsFullVideo = m_videoSelect != m_videoController;
        if (m_videoSelect != null)
        {
            m_videoSelect.gameObject.SetActive(false);
        }
        m_videoSelect = oldIsFullVideo ? m_videoController : m_videoFullController;
        m_videoSelect.gameObject.SetActive(true);
        Vector2 targetSize = oldIsFullVideo ? Vector2.one:Vector2.zero;
        bool isPlaying = m_videoPlayer != null && m_videoPlayer.isPlaying;
        double slider = m_videoPlayer != null && m_videoPlayer.frameCount>0? ((m_videoPlayer.frame + 1) / (double)m_videoPlayer.frameCount):0.0f;
        m_videoSelect.Setup((index) => {
            switch (index)
            {
                case 0:
                    Play();
                    break;
                case 1:
                    Pause();
                    break;
                case 2:
                    ChangeVideoConteroller();
                    break;
            }
        }, m_waitForReady, isPlaying, (float)slider,!oldIsFullVideo);

        m_videoSelect.UpdateTexture(m_videoPlayer.texture);
    }

    public void Play()
    {
        m_waitForReady = false;
        m_isPause = false;
        m_videoPlayer.Play();
    }
    public void Pause()
    {
        m_isPause = true;
    }

    public void Prepare()
    {
        m_videoPlayer.Stop();
        m_waitForReady = true;
        m_videoPlayer.Play();
    }

   

    // Update is called once per frame
    void Update()
    {
        if (m_videoPlayer != null)
        {
            if (m_videoPlayer.isPlaying)
            {

                double sliderValue = (m_videoPlayer.frame+1) / (double)m_videoPlayer.frameCount;
                
                if (m_videoSelect != null)
                {
                    m_videoSelect.UpdateTexture(m_videoPlayer.texture);
                    m_videoSelect.SetSlider((float)sliderValue);
                }

                //暂停
                if (m_waitForReady || m_isPause)
                {
                    m_videoPlayer.Pause();
                    return;
                }

                if (sliderValue >= 1.0f)
                {
                    if (!m_videoPlayer.isLooping)
                    {
                        Prepare();
                        m_videoSelect.ResetUI();
                        return;
                    }
                }
            }
        }
    }
}
