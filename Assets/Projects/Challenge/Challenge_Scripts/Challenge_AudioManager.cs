using System;
using UnityEngine;

namespace Challenge
{

    public enum SoundType 
    {
        Acceuil,
        Debut,
        Demarrage,
        Fin,
        Type,
        Goal,
        Ephemere,
        Evolutif,
        Level,
        Background

        
    }

    [System.Serializable]
    public class SoundEntry
    {
        public SoundType  type;
        public AudioClip clip;

        public int index;
        [Range(0f, 1f)] public float volume;
        public bool loop;
    }

    public class Challenge_AudioManager : MonoBehaviour
    {
        public static Challenge_AudioManager i;

        [Header("Global Volumes")]
        [Range(0f, 1f)] public float globalMusicVolume = 1f;
        [Range(0f, 1f)] public float globalSfxVolume = 1f;

        [Header("Sounds")]
        public SoundEntry[] sounds;

        private const int MAX_VARIANTS = 10; 

        private AudioClip[,] clipArray;
        private float[,] volumeArray;
        private bool[,] loopArray;

        public AudioSource audioSource;

        private void Awake()
        {

            i = this;

            int soundTypeCount = System.Enum.GetValues(typeof(SoundType)).Length;

            clipArray = new AudioClip[soundTypeCount, MAX_VARIANTS];
            volumeArray = new float[soundTypeCount, MAX_VARIANTS];
            loopArray = new bool[soundTypeCount, MAX_VARIANTS];

            foreach (var s in sounds)
            {
                int typeIndex = (int)s.type;

                if (s.index < 0 || s.index >= MAX_VARIANTS)
                {
                    Debug.LogWarning($"Index {s.index} out of range for {s.type}");
                    continue;
                }

                clipArray[typeIndex, s.index] = s.clip;
                volumeArray[typeIndex, s.index] = s.volume;
                loopArray[typeIndex, s.index] = s.loop;
            }
        }

        void Start()
        {
            PlayOneShot(SoundType.Acceuil);
        }

        // 🔊 SON SIMPLE (index 0)
        public void PlayOneShot(SoundType type)
        {
            PlayOneShot(type, 0);
        }

        // 🔊 SON AVEC INDEX
        public void PlayOneShot(SoundType type, int index)
        {
            int t = (int)type;

            if (clipArray[t, index] == null)
            {
                Debug.LogWarning($"Sound missing: {type} index {index}");
                return;
            }

            audioSource.PlayOneShot(
                clipArray[t, index],
                volumeArray[t, index] * globalSfxVolume
            );
        }

        public AudioSource CreateSource(GameObject owner, SoundType type, int index = 0)
        {
            int t = (int)type;

            if (clipArray[t, index] == null)
            {
                return null;
            }
                
            AudioSource source = owner.AddComponent<AudioSource>();
            source.clip = clipArray[t, index];
            source.loop = loopArray[t, index];
            source.volume = volumeArray[t, index] * globalSfxVolume;
            source.playOnAwake = false;



            return source;
        }

        // 🎵 MUSIQUE
        public void PlayMusic(AudioSource musicSource, SoundType type, int index = 0)
        {
            int t = (int)type;

            if (clipArray[t, index] == null)
                return;

            musicSource.clip = clipArray[t, index];
            musicSource.loop = true;
            musicSource.volume = volumeArray[t, index] * globalMusicVolume;
            musicSource.Play();
        }

        public void StopMusic(AudioSource musicSource)
        {
            musicSource.Stop();
        }
    }
 
}
