using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class VideoPlayerScript : MonoBehaviour
{
    [SerializeField] private GameObject videoPlayer;
    [SerializeField] private GameObject bgPanel;
    [SerializeField] private float timerValue = 0;
    [SerializeField] private float timerMax;
    private bool playingVideo;

    private void OnEnable()
    {
        videoPlayer.GetComponent<VideoPlayer>().targetCamera = Camera.main;
    }

    private void Update()
    {
        if (timerValue < timerMax)
        {
            timerValue += Time.deltaTime;
        }
        else if (!playingVideo)
        {
            StartCoroutine(playVideo());
            playingVideo = true;
        }
    }

    private IEnumerator playVideo()
    {
        videoPlayer.SetActive(true);
        bgPanel.SetActive(false);
        yield return new WaitForSeconds(97);
        bgPanel.SetActive(true);
        videoPlayer.SetActive(false);
        playingVideo = false;
        timerValue = 0;
    }

    private void OnDisable()
    {
        StopCoroutine(playVideo());
        bgPanel.SetActive(true);
        videoPlayer.SetActive(false);
        timerValue = 0;
        playingVideo = false;
    }
}
