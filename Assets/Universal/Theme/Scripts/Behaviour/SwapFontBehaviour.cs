using System.Collections;
using System.Collections.Generic;
using TMPro;
using Tool;
using UnityEngine;

namespace Theme
{
    public class SwapFontBehaviour : SwapObjectBehaviour
    {
        [SerializeField] TextMeshProUGUI _text;

        protected override void Swap(GameTheme theme)
        {
            var entity = _swapObject.GetSwapEntity(theme) as SwapFont;
            if(entity.Fonts.Count > 0) _text.font = entity.Fonts.RandomElement();
            if (entity.UseColor) _text.color = entity.Color;
        }
    }
}