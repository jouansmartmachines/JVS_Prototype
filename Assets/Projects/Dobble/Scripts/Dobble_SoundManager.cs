using System;
using System.Collections.Generic;
using UnityEngine;

namespace Dobble
{
    public class Dobble_SoundManager : MonoBehaviour
    {
        [Serializable]
        public class Melody
        {
            public List<AudioClip> clips;
            [Range(0f, 1f)] public float volume = 1f;
        }

        [Header("Audio")]
        [SerializeField] private AudioSource audioSource;

        [Header("Master")]
        [Range(0f, 1f)] public float masterVolume = 1f;

        [Serializable]
        public class OneShotClip
        {
            public AudioClip clip;
            [Range(0f, 1f)] public float volume = 1f;
        }

        [Header("Clips")]
        public List<OneShotClip> oneShots = new();
        public List<Melody> melodies = new();

        Melody currentMelody;
        int currentIndex = 0;

        [HideInInspector] public event Action<int> OnMelodyClipChanged;

        public void SetMasterVolume(float v)
        {
            masterVolume = Mathf.Clamp01(v);
            audioSource.volume = currentMelody.volume * masterVolume;
        }

        public void PlayOneShot(string clipName)
        {
            var oneShot = oneShots.Find(c => c.clip && c.clip.name == clipName);
            audioSource.PlayOneShot(oneShot.clip, oneShot.volume * masterVolume);
        }

        public void PlayMelody(int melodyIndex)
        {
            if (melodyIndex < 0 || melodyIndex >= melodies.Count) return;
            currentMelody = melodies[melodyIndex];
            currentIndex = 0;
            PlayAt(currentIndex);
        }

        public void OnNext(int idx)
        {
            /*
            currentIndex = idx % currentMelody.clips.Count;
            PlayAt(currentIndex);
            */
            PlayAt(idx);
    
        }

        void PlayAt(int index)
        {

            if (index < 0 || index >= currentMelody.clips.Count)
            {
             
                StopMelody();
                return;
            }    

            var clip = currentMelody.clips[index];
            if (!clip) return;

            audioSource.clip = clip;
            audioSource.loop = true;
            audioSource.volume = currentMelody.volume * masterVolume;
            audioSource.Play();

            OnMelodyClipChanged?.Invoke(index);
        }
        
        public void StopMelody()
        {
            if (audioSource.clip != null)
            {
                audioSource.clip = null;   
                audioSource.loop = false;  

            }

            currentMelody = null;
            currentIndex = 0;
        }

    }
}
