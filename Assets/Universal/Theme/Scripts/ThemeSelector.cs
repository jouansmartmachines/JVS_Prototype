using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Theme
{
    [RequireComponent(typeof(TMP_Dropdown))]
    public class ThemeSelector : MonoBehaviour
    {

        private TMP_Dropdown _dropdown;
        [SerializeField] ThemeManager _themeManager;

        public void Start()
        {
            _dropdown = GetComponent<TMP_Dropdown>();
            var list = new List<string>();
            foreach (var theme in _themeManager.Themes)
            {
                list.Add(theme.Name);
            }
            _dropdown.AddOptions(list);
            _dropdown.value = _dropdown.options.IndexOf(_dropdown.options.Find(x => x.text == _themeManager.CurrentGameTheme.Name));
            _dropdown.onValueChanged.AddListener(ChangeTheme);
        }

        private void ChangeTheme(int value)
        {
            _themeManager.ChangeTheme(_themeManager.Themes.Find(x => x.Name == _dropdown.options[value].text));
        }
    }
}