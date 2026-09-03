using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Theme
{
    [CreateAssetMenu(fileName = "ThemeManager", menuName = "Game/Theme/ThemeManager")]
    public class ThemeManager : ScriptableObject
    {
        public List<GameTheme> Themes => _themes;
        [SerializeField] List<GameTheme> _themes = new();
        public GameTheme CurrentGameTheme
        {
            get 
            {
                if (PlayerPrefs.HasKey(this.name))
                {
                    var theme = _themes.Find(x => x.Name.Equals(PlayerPrefs.GetString(this.name)));
                    //var theme = _themes[PlayerPrefs.GetInt(this.name)];
                    if (theme != null)
                    {
                        return theme;
                    }
                    //Debug.LogError("No Theme find with Name : " + PlayerPrefs.GetString(this.name));
                    Debug.LogError("No Theme find with Name : " + PlayerPrefs.GetInt(this.name));
                    return _currentGameTheme;
                }
                Debug.LogError($"No PlayerPrefs Key Find with name = {this.name}");
                return _currentGameTheme;
            }
        }
        [SerializeField] GameTheme _currentGameTheme;
        public GameTheme DefaultGameTheme => _defaultGameTheme;
        [SerializeField] GameTheme _defaultGameTheme;

        public Action<GameTheme> OnGameThemeSelected;

        public List<SwapObject> SwapObjects => _swapObjects;
        [SerializeField] List<SwapObject> _swapObjects = new();

        public void OnValidate()
        {
#if UNITY_EDITOR
            OnGameThemeSelected?.Invoke(_currentGameTheme);

            foreach (var theme in _themes)
            {
                if (theme == null) continue;
                theme.ThemeManager = this;
                theme.OnValidate();
            }

            foreach (var swap in _swapObjects)
            {
                if (swap == null) continue;
                swap.ThemeManager = this;
            }

            EditorUtility.SetDirty(this);
#endif
        }

        public void Awake()
        {
#if !UNITY_EDITOR
            //if(DefaultGameTheme != null) ChangeTheme(DefaultGameTheme);
#endif
        }

        public void ChangeTheme(GameTheme theme)
        {
            Debug.Log("Change Theme activated theme.ThemeManager.Equals(this)" +  theme.ThemeManager.Equals(this) + theme.Name);
            if (!theme.ThemeManager.Equals(this)) return;

            PlayerPrefs.SetString(this.name, theme.Name);
            //PlayerPrefs.SetInt(this.name, _themes.IndexOf(theme));
            PlayerPrefs.Save();
            Debug.Log($"From : {_currentGameTheme.Name} | To : {theme.Name} | Key : {this.name} = {PlayerPrefs.GetString(this.name)}");
            //Debug.Log($"From : {_currentGameTheme.Name} | To : {theme.Name} | Key : {this.name} = {PlayerPrefs.GetInt(this.name)}");
            _currentGameTheme = theme;
            OnGameThemeSelected?.Invoke(_currentGameTheme);
        }

        public void ChangeThemeFromInterface(GameTheme theme)
        {
            Debug.Log("Change Theme from interface activated theme.ThemeManager.Equals(this)" +  theme.ThemeManager.Equals(this) + theme.Name);

            PlayerPrefs.SetString(this.name, theme.Name);
            //PlayerPrefs.SetInt(this.name, _themes.IndexOf(theme));
            PlayerPrefs.Save();
            Debug.Log($"From : {_currentGameTheme.Name} | To : {theme.Name} | Key : {this.name} = {PlayerPrefs.GetString(this.name)}");
            //Debug.Log($"From : {_currentGameTheme.Name} | To : {theme.Name} | Key : {this.name} = {PlayerPrefs.GetInt(this.name)}");
            _currentGameTheme = theme;
            OnGameThemeSelected?.Invoke(_currentGameTheme);
        }

    
        public void ResetCurrentGameTheme()
        {
            Debug.Log("ResetThemeActivated");
            string savedThemeName = PlayerPrefs.GetString(this.name);
            if (PlayerPrefs.HasKey(this.name))
            {
                var theme = _themes.Find(x => x.Name.Equals(PlayerPrefs.GetString(this.name)));
                //var theme = _themes[PlayerPrefs.GetInt(this.name)];
                if (theme != null)
                {
                    Debug.Log($"From : {_currentGameTheme.Name} | To : {theme.Name} | Key : {this.name} = {PlayerPrefs.GetString(this.name)}");
                    //Debug.Log($"From : {_currentGameTheme.Name} | To : {theme.Name} | Key : {this.name} = {PlayerPrefs.GetInt(this.name)}");
                    ChangeTheme(theme);
                }
                else
                {
                    Debug.LogError($"No GameTheme Find with name = {PlayerPrefs.GetString(this.name)}");
                    //Debug.LogError($"No GameTheme Find with name = {PlayerPrefs.GetInt(this.name)}");
                }
            }
            else
            {
                Debug.LogWarning($"No PlayerPrefs Key Find with name = {this.name}");
            }
        }

        
        [EditorCools.Button]
        public void PressAfterAnyModif()
        {
            OnValidate();
        }
    }
}