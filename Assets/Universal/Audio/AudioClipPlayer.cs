using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioClipPlayer : MonoBehaviour
{
    [SerializeField] private ScriptableAudioClip _clip;
    [SerializeField] private AudioClip _clipAudioSub;
    [SerializeField] private AudioSource _MyAudioSource;
    [SerializeField] private bool _start;
    [SerializeField] private bool _loopStart;

    // Start is called before the first frame updat
    private void Start()
    {
        if (!_start)
            return;
        if (_loopStart == true)
        {
            PlayLoop();
            return;
        }
        PlaySound();

    }
    public void PlaySound()
    {
        if (_clip == null) 
        {
            _MyAudioSource.PlayOneShot(_clipAudioSub);
            return;
        }
        _MyAudioSource.PlayOneShot(_clip.value);
    }



    public void PlayLoop()
    {
        _MyAudioSource.loop = true;
        if (_clip == null)
        {
            _MyAudioSource.clip = _clipAudioSub;
        }
        else
            _MyAudioSource.clip = _clip.value;
        _MyAudioSource.Play();
    }
    public void Stop()
    {
        _MyAudioSource.Stop();

        _MyAudioSource.loop = false;
        _MyAudioSource.clip = null;

    }
}
