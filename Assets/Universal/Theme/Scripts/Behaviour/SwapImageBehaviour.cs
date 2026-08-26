using System.Collections;
using System.Collections.Generic;
using Tool;
using UnityEngine;
using UnityEngine.UI;

namespace Theme
{
    public class SwapImageBehaviour : SwapObjectBehaviour
    {
        [SerializeField] Image _image;

        protected override void Swap(GameTheme theme)
        {
            var entity = _swapObject.GetSwapEntity(theme) as SwapSprite;
            if (entity.Sprites.Count > 0) _image.sprite = entity.Sprites.RandomElement();
            if (entity.UseColor) _image.color = entity.Color;
            //Debug.Log(_image.color);
            if (entity.SetNativeSize) _image.SetNativeSize();
        }
    }
}
