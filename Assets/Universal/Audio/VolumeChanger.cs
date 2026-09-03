using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VolumeChanger : MonoBehaviour
{
    [SerializeField] private List<AudioSource> _sources;
    [SerializeField] private ScriptableObjectValue _volume;
    [SerializeField] private float _baseVolumeValue;

    private void Start()
    {
        _volume.value = _baseVolumeValue;

        ChangeVolume();
    }
    
    public void ChangeVolume() 
    {
        foreach (AudioSource source in _sources)
        {
            source.volume = _volume.value;
        }
    }
}
