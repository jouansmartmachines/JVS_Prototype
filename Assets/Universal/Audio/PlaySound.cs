using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaySound : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] private ScriptableAudioClip _soSound;
    [SerializeField] private AudioClip _audioClip;
    [SerializeField] private GameEvent _event;
    [SerializeField] private bool _bstart;
    void Awake()
    {
        if (_bstart)
            Play();
    }

    // Update is called once per frame
    void Play()
    {
        _soSound.value = _audioClip;
        _event.Raise();
    }
}
