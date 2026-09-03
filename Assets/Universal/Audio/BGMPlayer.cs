using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BGMPlayer :MonoBehaviour
{
    [SerializeField] private AudioClip _clip;
    [SerializeField] private AudioSource _source;
    // Start is called before the first frame update
    void Update()
    {
        _source.clip = _clip;
        _source.Play();
        _source.loop = true;
        Destroy(this);
    }
}
