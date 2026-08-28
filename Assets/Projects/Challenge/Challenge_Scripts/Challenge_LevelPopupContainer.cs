using UnityEngine;
using System;

namespace Challenge
{
    [Serializable]
    public class LevelPopupInfo
    {
        public string title;
    }

    public class Challenge_LevelPopupContainer : MonoBehaviour
    {
        [Header("Level Messages Configuration")]
        public LevelPopupInfo[] popupMessages;
    }
}