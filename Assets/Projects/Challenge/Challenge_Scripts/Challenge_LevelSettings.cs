
using UnityEngine.UI;
using UnityEngine;

namespace Challenge
{
    public enum PositionMode
    {
        Nearby = 50,
        Close = 66,
        Opposite = 100

    }

    [CreateAssetMenu(fileName = "LevelSettings", menuName = "Challenge/LevelSettings", order = 1)]
    public class Challenge_LevelSettings : ScriptableObject
    {
        public int level;
        public int points;
        public PositionMode[] positionMode;
        public TargetCategoryValue[] targetCategories;

        public bool mask;

        public Color color ;
        public Sprite levelImage;


        [Header("Parameters")]
        public float Offset = 50f;

        public float EphemereTime;

        public int scoreToReach;

        public float targetScale = 1f;
        
    }
}
