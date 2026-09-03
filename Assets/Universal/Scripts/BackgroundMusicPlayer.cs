using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Tool;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class BackgroundMusicPlayer : MonoBehaviour
{
    [SerializeField]
    private AudioClip[] _backgroundMusic;

    [SerializeField]
    private bool _playAtStart = true;

    [SerializeField]
    private bool _loopAudio = true;

    AudioSource _source;
    Queue<AudioClip> _clipQueue;

    public bool IsPlaying => _isPlaying;
    private bool _isPlaying;

    public void Start()
    {
        _source = GetComponent<AudioSource>();
        var list = _backgroundMusic.ToList().ReturnShuffle();
        _clipQueue = new Queue<AudioClip>(list);

        if (_clipQueue != null && _playAtStart)
            PlayMusic();
    }

    public void StopMusic()
    {
        _source.Stop();
        _source.clip = null;
        _isPlaying = false;
    }

    public void PlayMusic()
    {
        IEnumerator WaitEnd()
        {
            yield return new WaitUntil(() => !_source.isPlaying || !_isPlaying);

            if (_isPlaying)
                PlayMusic();
        }

        AudioClip clip = _clipQueue.Dequeue();
        _source.clip = clip;
        _source.loop = _loopAudio;
        _isPlaying = true;
        _source.Play();
        _clipQueue.Enqueue(clip);
        if(!_loopAudio)
            StartCoroutine(WaitEnd());
    }
}
