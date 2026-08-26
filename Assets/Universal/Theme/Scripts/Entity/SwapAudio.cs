using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Theme
{
    [CreateAssetMenu(fileName = "SwapAudio", menuName = "Game/Theme/Entity/SwapAudio")]
    public class SwapAudio : SwapEntity
    {
        public List<AudioClip> AudioClips;
    }
}