using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Theme
{
    //[CreateAssetMenu(fileName = "SwapEntity", menuName = "Game/Theme/SwapEntity")]
    public abstract class SwapEntity : ScriptableObject
    {
        public GameTheme Theme;

        public void OnValidate()
        {
#if UNITY_EDITOR
            if(Theme == null)
            {
                var nameSplit = this.name.Split('_');
                string themeName = $"{nameSplit.First()}_Theme_{nameSplit.ElementAt(2)}";
                if (!string.IsNullOrEmpty(themeName))
                {
                    string[] guids = AssetDatabase.FindAssets($"{themeName} t:GameTheme");

                    if (guids.Length > 0)
                    {
                        string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                        Theme = AssetDatabase.LoadAssetAtPath<GameTheme>(path);
                    }
                    else
                    {
                        Debug.LogWarning($"Aucun ScriptableObject nommÅE{themeName} trouvÅE");
                    }
                }
            }

            if (Theme != null)
            {
                if (Theme.ThemeManager != null)
                {
                    foreach (var theme in Theme.ThemeManager.Themes)
                    {
                        if (theme.SwapEntities.Contains(this))
                        {
                            theme.SwapEntities.Remove(this);
                        }
                    }
                }

                if (!Theme.SwapEntities.Contains(this))
                {
                    Theme.SwapEntities.Add(this);
                }

                Theme.OnValidate();
            }

            EditorUtility.SetDirty(this);
#endif
        }

        [EditorCools.Button]
        public void PressAfterAnyModif()
        {
            OnValidate();
        }
    }
}