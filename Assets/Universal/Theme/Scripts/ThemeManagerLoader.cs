using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Theme;

#if UNITY_EDITOR
using UnityEditor;
#endif

//[CreateAssetMenu(fileName = "ThemeManagerLoader", menuName = "Game/Theme/Loader")]
public class ThemeManagerLoader : ScriptableObject
{
    [SerializeField] List<ThemeManager> _managers = new();

#if UNITY_EDITOR
    [EditorCools.Button]
    public void GetAllThemeManagers()
    {
        _managers.Clear();
        try
        {
            var paths = AssetDatabase.FindAssets("t:ThemeManager");
            Debug.Log($"Theme Loader : {paths.Length}");
            foreach (var path in paths)
            {
                ThemeManager theme = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(path), typeof(ThemeManager)) as ThemeManager;
                _managers.Add(theme);
                Debug.Log("Theme : " + theme.name);
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Theme Error : {ex}");
        }
        EditorUtility.SetDirty(this);
    }

    public void OnValidate()
    {
        GetAllThemeManagers();
    }
#endif

    public void LoadAllThemeManagers()
    {
        foreach (var theme in _managers)
        {
            theme.ResetCurrentGameTheme();
        }
    }
}
