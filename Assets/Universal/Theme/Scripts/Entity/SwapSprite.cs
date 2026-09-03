using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Theme
{
    [CreateAssetMenu(fileName = "SwapSprite", menuName = "Game/Theme/Entity/SwapSprite")]
    public class SwapSprite : SwapEntity
    {
        public bool SetNativeSize => _setNativeSize;
        [SerializeField] bool _setNativeSize;
        public bool UseColor => _useColor;
        [SerializeField] bool _useColor = false;
        public Color Color => _color;
        [SerializeField] Color _color = Color.white;
        public List<Sprite> Sprites;
    }
}