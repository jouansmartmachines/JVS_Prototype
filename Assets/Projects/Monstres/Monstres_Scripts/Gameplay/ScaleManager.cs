using UnityEngine;

namespace Monstres
{
    public class ScaleManager : MonoBehaviour
    {
        private Monstres_GameManager pathmanager;
        
        [Header("Rock Scale Settings")]
        public bool IsThemeScaleActive = true;
        
        [Tooltip("La taille minimale que le rocher peut atteindre (quand scaleRatio est proche de 0)")]
        public float scaleResizer = 0.25f; 
        
        [Tooltip("La taille maximale autorisée (le Cap)")]
        public float scalelimit = 0.45f;
        void Awake()
        {
            pathmanager = GetComponentInParent<Monstres_GameManager>();

            pathmanager.IsThemeScaleActive = IsThemeScaleActive;
            pathmanager.scaleResizer = scaleResizer;
            pathmanager.scalelimit = scalelimit;

        }

    }
}