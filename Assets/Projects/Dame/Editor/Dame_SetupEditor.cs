using UnityEngine;
using UnityEditor;
using System.IO;
using UnityEngine.EventSystems;
using TMPro;
using UnityEditor.SceneManagement;
using Theme;

public class Dame_SetupEditor : EditorWindow
{
    static string _basePath = "Assets/Projects/Dame";
    static string _spritePath = _basePath + "/Sprites";
    static string _soundPath = _basePath + "/Sons";

    [MenuItem("Tools/Dame - Tout configurer")]
    static void ConfigurerTout()
    {
        if (EditorApplication.isPlaying)
        {
            Debug.LogError("Arretez le jeu avant de lancer l'outil !");
            return;
        }

        Debug.Log("=== DEBUT configuration Dame ===");

        // 1. Assigner les sprites et sons dans les scenes
        AssignSpritesToGameScene();
        Debug.Log("✓ Sprites assignes a la GameScene");

        // 2. Generer les sons WAV si absents
        MakeWAV("move", 0.1f, 400, 0.3f);
        MakeWAV("capture", 0.2f, 600, 0.5f);
        MakeWAV("crown", 0.3f, 800, 0.4f);
        MakeWAV("win", 0.5f, 200, 0.8f);
        Debug.Log("✓ Sons generes");

        // 3. Creer l'infrastructure de themes
        CreateThemeInfrastructure();
        Debug.Log("✓ Infrastructure de themes creee");

        // 4. Configurer le menu (dropdown theme)
        SetupMenu();
        Debug.Log("✓ Menu configure (theme dropdown)");

        AssetDatabase.Refresh();
        Debug.Log("=== Dame completement configure ===");
        Debug.Log("Lancez la scene GameScene (Scenes/GameScene_Dame.unity)");
    }

    static void AssignSpritesToGameScene()
    {
        string scenePath = _basePath + "/Scenes/GameScene_Dame.unity";
        var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

        var gm = Object.FindFirstObjectByType<Dame.Dame_GameManager>();
        if (gm == null)
        {
            Debug.LogWarning("GameManager pas trouve dans la scene !");
            return;
        }

        // Charger et assigner les sprites
        gm.caseFoncee = AssetDatabase.LoadAssetAtPath<Sprite>(_spritePath + "/case_foncee.png");
        gm.caseClaire = AssetDatabase.LoadAssetAtPath<Sprite>(_spritePath + "/case_claire.png");
        gm.pionBlanc = AssetDatabase.LoadAssetAtPath<Sprite>(_spritePath + "/pion_blanc.png");
        gm.pionNoir = AssetDatabase.LoadAssetAtPath<Sprite>(_spritePath + "/pion_noir.png");
        gm.dameBlanche = AssetDatabase.LoadAssetAtPath<Sprite>(_spritePath + "/dame_blanche.png");
        gm.dameNoire = AssetDatabase.LoadAssetAtPath<Sprite>(_spritePath + "/dame_noire.png");

        // Charger et assigner les sons
        gm.moveSound = AssetDatabase.LoadAssetAtPath<AudioClip>(_soundPath + "/move.wav");
        gm.captureSound = AssetDatabase.LoadAssetAtPath<AudioClip>(_soundPath + "/capture.wav");
        gm.crownSound = AssetDatabase.LoadAssetAtPath<AudioClip>(_soundPath + "/crown.wav");
        gm.winSound = AssetDatabase.LoadAssetAtPath<AudioClip>(_soundPath + "/win.wav");

        EditorSceneManager.SaveScene(scene);
    }

    static void MakeWAV(string name, float dur, float freq, float vol)
    {
        string path = _soundPath + "/" + name + ".wav";
        if (File.Exists(path)) return;
        int sr = 44100; int samples = (int)(sr * dur);
        var audio = new float[samples];
        for (int i = 0; i < samples; i++)
            audio[i] = (float)(System.Math.Sin(2 * System.Math.PI * freq * i / sr) * vol * (1 - i / (float)samples));

        using (var bw = new BinaryWriter(File.Open(path, FileMode.Create)))
        {
            bw.Write(new char[] { 'R', 'I', 'F', 'F' });
            bw.Write(36 + samples * 2);
            bw.Write(new char[] { 'W', 'A', 'V', 'E', 'f', 'm', 't', ' ' });
            bw.Write(16); bw.Write((short)1); bw.Write((short)1);
            bw.Write(sr); bw.Write(sr * 2);
            bw.Write((short)2); bw.Write((short)16);
            bw.Write(new char[] { 'd', 'a', 't', 'a' });
            bw.Write(samples * 2);
            for (int i = 0; i < samples; i++) bw.Write((short)(audio[i] * 32767));
        }
    }

    static void CreateThemeInfrastructure()
    {
        // Creer le dossier Themes s'il n'existe pas
        string themePath = _basePath + "/Themes";
        Directory.CreateDirectory(themePath + "/Classique");
        Directory.CreateDirectory(themePath + "/Bois");

        // Chaque theme aura ses SwapEntities (creees manuellement dans Unity)
        // ThemeManager cree si pas deja present
        string tmPath = themePath + "/Dame_ThemeManager.asset";
        if (!File.Exists(tmPath))
        {
            var tm = ScriptableObject.CreateInstance<ThemeManager>();
            AssetDatabase.CreateAsset(tm, tmPath);
            AssetDatabase.SaveAssets();
            Debug.Log("✓ ThemeManager cree");
        }
    }

    static void SetupMenu()
    {
        string scenePath = _basePath + "/Scenes/Menu_Dame.unity";
        if (!File.Exists(scenePath)) { Debug.LogError("Menu scene manquante !"); return; }
        var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

        var canvas = Object.FindFirstObjectByType<Canvas>();
        if (canvas == null) { Debug.LogError("Canvas pas trouve dans le menu"); return; }

        var diffGO = GameObject.Find("Difficulty");
        if (diffGO == null) { Debug.LogWarning("Difficulty dropdown pas trouve"); return; }

        if (GameObject.Find("ThemeDropdown") != null) return;

        var themeGO = Object.Instantiate(diffGO, canvas.transform);
        themeGO.name = "ThemeDropdown";
        themeGO.GetComponent<RectTransform>().anchoredPosition += Vector2.down * 150f;

        var label = themeGO.transform.Find("Text");
        if (label != null)
        {
            var tmp = label.GetComponent<TextMeshProUGUI>();
            if (tmp != null) tmp.text = "Theme :";
        }

        var ts = themeGO.AddComponent<Theme.ThemeSelector>();
        var tm = AssetDatabase.LoadAssetAtPath<Theme.ThemeManager>(_basePath + "/Themes/Dame_ThemeManager.asset");
        if (tm != null)
        {
            var field = typeof(Theme.ThemeSelector).GetField("_themeManager",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null) field.SetValue(ts, tm);
        }

        EditorSceneManager.SaveScene(scene);
        Debug.Log("✓ Theme dropdown ajoute au menu");
    }
}