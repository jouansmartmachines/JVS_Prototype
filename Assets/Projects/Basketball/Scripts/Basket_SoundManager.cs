using System.Collections;
using System.Collections.Generic;
using Tool;
using UnityEngine;

namespace Basket
{
    public class Basket_SoundManager : MonoBehaviour
    {
        public static Basket_SoundManager i;
        private void Awake()
        {
            if (i != null)
            {
                Destroy(gameObject);
                return;
            }

            i = this;
        }

        public AudioSource _source;
        public AudioSource _source2;
        [SerializeField] AudioSource[] _netSource;

        public AudioClip[] _ambiences;
        public AudioClip[] _musics;
        public AudioClip[] _hits;
        public AudioClip[] _bounce;
        public AudioClip[] _end;
        public AudioClip _net;
        public AudioClip _movementCamera;

        public void Start()
        {
            IEnumerator Ambiance()
            {
                Queue<AudioClip> audios = new Queue<AudioClip>(_ambiences.ReturnShuffle());

                while (true)
                {
                    var clip = audios.Dequeue();
                    _source.clip = clip;
                    _source.Play();
                    audios.Enqueue(clip);
                    yield return new WaitUntil(() => !_source.isPlaying);
                }
            }

            IEnumerator Music()
            {
                Queue<AudioClip> audios = new Queue<AudioClip>(_musics.ReturnShuffle());

                _source2.clip = _musics[0];
                _source2.Play();
                yield return new WaitUntil(() => !_source2.isPlaying);

                while (true)
                {
                    var clip = audios.Dequeue();
                    _source2.clip = clip;
                    _source2.Play();
                    audios.Enqueue(clip);
                    yield return new WaitUntil(() => !_source2.isPlaying);
                }

            }

            StartCoroutine(Ambiance());
            StartCoroutine(Music());
        }

        public void PlaySound(AudioClip clip)
        {
            _source.clip = clip;
            _source.Play();
        }

        public void PlaySound(AudioClip clip, Vector3 pos)
        {
            AudioSource.PlayClipAtPoint(clip, pos);
        }

        public void PlayNet(bool IsP1)
        {
            _netSource[IsP1 ? 1 : 0].clip = _net;
            _netSource[IsP1 ? 1 : 0].Play();
        }
    }
}