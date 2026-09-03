using MenuSelection;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class StopAllBackgroundMusic : MonoBehaviour
{
    [SerializeField] AudioMixerGroup _musicGroup;

    public void Awake()
    {
        MenuSelectionButton.OnButtonActivated += DisableAllBackgrounMusic;
        MenuSelectionButton.OnButtonClick += EnableAllBackgroundMusic;
    }

    private void DisableAllBackgrounMusic()
    {
        _musicGroup.audioMixer.SetFloat("Volume", -80f);
    }

    private void EnableAllBackgroundMusic()
    {
        _musicGroup.audioMixer.SetFloat("Volume", 0f);
    }
}
