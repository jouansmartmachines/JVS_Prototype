using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Theme
{
    public class SwapEnableBehaviour : MonoBehaviour
    {
        [SerializeField] bool swapOnAwake = true;
        [SerializeField] bool swapOnStart = false;
        [SerializeField] List<GameTheme> _gameThemes;

        public void Awake()
        {
            if (_gameThemes.Count <= 0)
            {
                Destroy(this);
                return;
            }
            _gameThemes.First().ThemeManager.OnGameThemeSelected += Swap;
            if (swapOnAwake)
            {
                Swap(_gameThemes.First().ThemeManager.CurrentGameTheme);
            }
        }

        public void Start()
        {
            if (_gameThemes.Count <= 0)
            {
                Destroy(this);
                return;
            }
            if (swapOnStart)
            {
                Swap(_gameThemes.First().ThemeManager.CurrentGameTheme);
            }
        }

        public void OnDestroy()
        {
            if (_gameThemes.Count > 0) _gameThemes.First().ThemeManager.OnGameThemeSelected -= Swap;
        }

        protected void Swap(GameTheme theme)
        {
            this.gameObject.SetActive(_gameThemes.Contains(theme));
        }
    }
}