using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Theme
{
    [CreateAssetMenu(fileName = "SwapFont", menuName = "Game/Theme/Entity/SwapFont")]
    public class SwapFont : SwapEntity
    {
        public bool UseColor => _useColor;
        [SerializeField] bool _useColor = false;
        public Color Color => _color;
        [SerializeField] Color _color = Color.white;
        public List<TMP_FontAsset> Fonts;
    }
}