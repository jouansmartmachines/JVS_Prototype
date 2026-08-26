using UnityEngine;


namespace Theme
{
    public class ThemeAppSelector : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private ThemeManager _themeManager;
        [HideInInspector] public static string LastStaticName;
        void Awake()
        {
            ApplyTheme();
        }

        private void ApplyTheme()
        {
            string lastColorName = LastStaticName;

            if (string.IsNullOrEmpty(lastColorName))
            {
                Debug.LogWarning("ThemeSelector.LastStaticName est vide ou nul !");
                return;
            }

            string firstLetter = lastColorName.Substring(0, 1);
            Debug.Log("Recherche d'un thème commençant par : " + firstLetter);
            var targetTheme = _themeManager.Themes.Find(x => 
                x.Name.StartsWith(firstLetter, System.StringComparison.OrdinalIgnoreCase));
            Debug.Log("Theme trouvé" + targetTheme);
            _themeManager.ChangeThemeFromInterface(targetTheme);

        }
    }
}