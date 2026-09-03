using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Theme
{
    [CreateAssetMenu(fileName = "GameTheme", menuName = "Game/Theme/GameTheme")]
    public class GameTheme : ScriptableObject
    {
        public string Name;
        public ThemeManager ThemeManager;
        public List<SwapEntity> SwapEntities => _swapEntities;
        [SerializeField] List<SwapEntity> _swapEntities = new();

        public void OnValidate()
        {
#if UNITY_EDITOR
            if(ThemeManager == null)
            {
                var nameSplit = this.name.Split('_');
                string themeManagerName = $"{nameSplit.First()}_ThemeManager";
                if (!string.IsNullOrEmpty(themeManagerName))
                {
                    string[] guids = AssetDatabase.FindAssets($"{themeManagerName} t:ThemeManager");

                    if (guids.Length > 0)
                    {
                        string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                        ThemeManager = AssetDatabase.LoadAssetAtPath<ThemeManager>(path);
                    }
                    else
                    {
                        Debug.LogWarning($"Aucun ScriptableObject nomm�E{themeManagerName} trouv�E");
                    }
                }
            }

            if (ThemeManager != null && !ThemeManager.Themes.Contains(this))
            {
                ThemeManager.Themes.Add(this);
            }

            foreach (var entity in _swapEntities)
            {
                if (entity == null) continue;
                entity.Theme = this;
                var split = entity.name.Split('_');
                var search = $"{split.First()}_{split.Last()}";
                //Debug.Log(search);
                if (ThemeManager?.SwapObjects == null) continue;

                // Ajout de "x != null" dans la recherche
                var swapObject = ThemeManager.SwapObjects.Find(x => x != null && x.name == search);
                if (swapObject != null && !swapObject.Swaps.Contains(entity))
                {
                    swapObject.Swaps.Add(entity);
                }
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
