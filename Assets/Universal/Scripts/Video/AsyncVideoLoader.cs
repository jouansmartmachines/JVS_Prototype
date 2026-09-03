using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;
using System;

public class AsyncVideoLoader : MonoBehaviour
{
    [SerializeField] private VideoPlayer _videoPlayer;
    [SerializeField] private RawImage _targetUiImage;

    private int _startFrame;

    void Start()
    {
        if (_videoPlayer == null)
            _videoPlayer = GetComponent<VideoPlayer>();

        _videoPlayer.playOnAwake = false;
        
        _videoPlayer.prepareCompleted += OnVideoPrepared;
        _videoPlayer.started += OnVideoStarted;
        
        _startFrame = Time.frameCount;
        
        // Horloge système précise (ex: 10:47:23.010)
        string clockTime = DateTime.Now.ToString("HH:mm:ss.fff");
        Debug.Log($"[VideoPerf] 1. Appel de Prepare() | Frame : {_startFrame} | Horloge : {clockTime}");

        _videoPlayer.Prepare();
    }

    private void OnVideoPrepared(VideoPlayer source)
    {
        int framesElapsed = Time.frameCount - _startFrame;
        string clockTime = DateTime.Now.ToString("HH:mm:ss.fff");
        
        Debug.Log($"[VideoPerf] 2. Vidéo prête en RAM | Frames écoulées : {framesElapsed} | Horloge : {clockTime}");
        
        _videoPlayer.Play();
    }

    private void OnVideoStarted(VideoPlayer source)
    {
        int framesElapsed = Time.frameCount - _startFrame;
        string clockTime = DateTime.Now.ToString("HH:mm:ss.fff");

        Debug.Log($"[VideoPerf] 3. Lecture commencée à l'écran | Total frames : {framesElapsed} | Horloge : {clockTime}");

        if (_targetUiImage != null && _videoPlayer.texture != null)
        {
            _targetUiImage.texture = _videoPlayer.texture;
            _targetUiImage.color = Color.white;

            AspectRatioFitter aspectFitter = _targetUiImage.GetComponent<AspectRatioFitter>();
            if (aspectFitter == null) 
                aspectFitter = _targetUiImage.gameObject.AddComponent<AspectRatioFitter>();
            
            aspectFitter.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
            aspectFitter.aspectRatio = (float)_videoPlayer.width / _videoPlayer.height;
        }
    }

    void OnDestroy()
    {
        if (_videoPlayer != null)
        {
            _videoPlayer.prepareCompleted -= OnVideoPrepared;
            _videoPlayer.started -= OnVideoStarted;
        }
    }
}