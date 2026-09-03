using UnityEngine;

namespace Theme
{
    public abstract class SwapObjectBehaviour : MonoBehaviour
    {
        [SerializeField] bool swapOnAwake = true;
        [SerializeField] bool swapOnStart = false;
        [SerializeField] protected SwapObject _swapObject;

        // On ajoute un flag pour éviter que le clone ne relance le processus
        private static bool _isSwapping = false;

        public void Awake()
        {
            if (_swapObject == null)
            {
                Destroy(this);
                return;
            }

            // IMPORTANT : On s'abonne à l'évènement
            _swapObject.ThemeManager.OnGameThemeSelected += Swap;

            if (swapOnAwake)
            {
                // Si on est déjà en train de swapper, on n'autorise pas 
                // le nouvel objet (le clone) à relancer un Swap
                if (_isSwapping) return;

                _isSwapping = true;
                Swap(_swapObject.ThemeManager.CurrentGameTheme);
                _isSwapping = false;
            }
        }

        public void Start()
        {
            if (_swapObject == null) return;

            if (swapOnStart)
            {
                // Même protection pour le Start
                if (_isSwapping) return;

                _isSwapping = true;
                Swap(_swapObject.ThemeManager.CurrentGameTheme);
                _isSwapping = false;
            }
        }

        public void OnDestroy()
        {
            if (_swapObject != null && _swapObject.ThemeManager != null)
            {
                _swapObject.ThemeManager.OnGameThemeSelected -= Swap;
            }
        }

        protected abstract void Swap(GameTheme theme);
    }
}