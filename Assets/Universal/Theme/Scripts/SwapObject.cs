using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Theme
{
    [CreateAssetMenu(fileName = "SwapObject", menuName = "Game/Theme/SwapObject")]
    public class SwapObject : ScriptableObject
    {
        public ThemeManager ThemeManager;

        public List<SwapEntity> Swaps => _swaps;
        [SerializeField] protected List<SwapEntity> _swaps = new();

        public SwapEntity GetSwapEntity(GameTheme theme) 
        {
            Debug.Log($"[GetSwapEntity] Recherche de l'entité avec le thème : {theme}");

            SwapEntity entity = _swaps.Find(x => x.Theme == theme);
            if(entity == null)
            {
                Debug.LogWarning($"[GetSwapEntity] Aucun SwapEntity trouvé pour le thème {theme}. Tentative avec le thème par défaut : {ThemeManager.DefaultGameTheme}");
                
                entity = _swaps.Find(x => x.Theme == ThemeManager.DefaultGameTheme); 
                if (entity == null)
                {
                    Debug.LogError($"[GetSwapEntity] Entity name {entity.name}ERREUR : Aucun SwapEntity du type {typeof(SwapEntity)} trouvé dans {name} (ni pour {theme}, ni pour le thème par défaut) !");
                    return default;
                }
            }

            Debug.Log($"[GetSwapEntity] Entité trouvée avec succès pour le thème : {entity.name}");
            return entity;
        }

        public SwapEntity GetSwapEntity() 
        {
            Debug.Log($"[GetSwapEntity] Appel sans paramètre. Utilisation du thème actuel : {ThemeManager.CurrentGameTheme}");
            return GetSwapEntity(ThemeManager.CurrentGameTheme);
        }

        public T GetSwapEntity<T>(GameTheme theme) where T : SwapEntity
        {
            Debug.Log($"[GetSwapEntity<{typeof(T)}>] Recherche du type spécifique avec le thème : {theme}");

            var entity = _swaps.Find(x => x.Theme == theme && x is T) as T;
            if (entity == null)
            {
                Debug.LogWarning($"[GetSwapEntity<{typeof(T)}>] Aucun type spécifié trouvé pour {theme}. Tentative avec le thème par défaut : {ThemeManager.DefaultGameTheme}");
                
                entity = _swaps.Find(x => x.Theme == ThemeManager.DefaultGameTheme && x is T) as T;
                if(entity == null)
                {
                    Debug.LogError($"[GetSwapEntity<{typeof(T)}>] ERREUR : Aucun SwapEntity de type {typeof(T)} trouvé dans {name} !");
                    return default;
                }
            }

            Debug.Log($"[GetSwapEntity<{typeof(T)}>] Entité type spécifique trouvée avec succès ! (Thème : {entity.name})");
            return entity;
        }
        public T GetSwapEntity<T>() where T : SwapEntity => GetSwapEntity<T>(ThemeManager.CurrentGameTheme);

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

            if (ThemeManager != null)
            {
                if (!ThemeManager.SwapObjects.Contains(this))
                {
                    ThemeManager.SwapObjects.Add(this);
                }
                for (int i = 0; i < _swaps.Count; i++)
                {
                    if (_swaps[i] == null) _swaps.Remove(_swaps[i]);
                }
                ThemeManager.OnValidate();
                EditorUtility.SetDirty(this);
            }
#endif
        }

        [EditorCools.Button, ContextMenu("OnValidate")]
        public void PressAfterAnyModif()
        {
            OnValidate();
        }
    }
}