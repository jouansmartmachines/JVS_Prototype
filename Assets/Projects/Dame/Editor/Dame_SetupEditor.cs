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
    static string _fontPath = _basePath + "/Font";

    [MenuItem("Tools/Dame - Tout configurer")]
    static void ConfigurerTout()
    {
        if (EditorApplication.isPlaying)
        {
            Debug.LogError("Arretez le jeu avant de lancer l'outil !");
            return;
        }

        Debug.Log("=== DEBUT configuration Dame ===");

        // 1. Copier la police LiberationSans SDF si absente
        CopyDefaultFont();
        Debug.Log("✓ Police verifiee");

        // 2. Assigner les sprites et sons dans les scenes
        AssignSpritesToGameScene();
        AssignSpritesToAccueil();
        AssignSpritesToScore();
        Debug.Log("✓ Sprites assignes a toutes les scenes");

        // 3. Generer les sons WAV si absents
        MakeWAV("move", 0.1f, 400, 0.3f);
        MakeWAV("capture", 0.2f, 600, 0.5f);
        MakeWAV("crown", 0.3f, 800, 0.4f);
        MakeWAV("win", 0.5f, 200, 0.8f);
        Debug.Log("✓ Sons generes");

        // 4. Creer l'infrastructure de themes
        CreateThemeInfrastructure();
        Debug.Log("✓ Infrastructure de themes creee");

        // 5. Configurer le menu (theme dropdown + 2 joueurs)
        SetupMenu();
        Debug.Log("✓ Menu configure");

        // 6. Ajouter SwapImageBehaviour sur les backgrounds
        AddSwapImageToScenes();
        Debug.Log("✓ SwapImage ajoute aux backgrounds");

        AssetDatabase.Refresh();
        Debug.Log("=== Dame completement configure ===");
        Debug.Log("Lancez la scene Accueil (Scenes/Accueil_Dame.unity)");
    }

    static void CopyDefaultFont()
    {
        // LiberationSans SDF est dans TMP Essentials, disponible partout
        // On cree juste un alias dans Font/ pour que Unity le trouve
        string fontDest = _fontPath + "/Dame_Font.asset";
        if (!File.Exists(fontDest))
        {
            var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset");
            if (font != null)
            {
                AssetDatabase.CopyAsset("Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset", fontDest);
                Debug.Log("Police LiberationSans copiee dans Font/");
            }
        }
    }

    static void AssignSpritesToGameScene()
    {
        string scenePath = _basePath + "/Scenes/GameScene_Dame.unity";
        var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

        var gm = Object.FindFirstObjectByType<Dame.Dame_GameManager>();
        if (gm == null) { Debug.LogWarning("GameManager pas trouve dans GameScene !"); return; }

        // Sprites du plateau
        gm.caseFoncee = AssetDatabase.LoadAssetAtPath<Sprite>(_spritePath + "/case_foncee.png");
        gm.caseClaire = AssetDatabase.LoadAssetAtPath<Sprite>(_spritePath + "/case_claire.png");
        gm.pionBlanc = AssetDatabase.LoadAssetAtPath<Sprite>(_spritePath + "/pion_blanc.png");
        gm.pionNoir = AssetDatabase.LoadAssetAtPath<Sprite>(_spritePath + "/pion_noir.png");
        gm.dameBlanche = AssetDatabase.LoadAssetAtPath<Sprite>(_spritePath + "/dame_blanche.png");
        gm.dameNoire = AssetDatabase.LoadAssetAtPath<Sprite>(_spritePath + "/dame_noire.png");

        // Sons
        gm.moveSound = AssetDatabase.LoadAssetAtPath<AudioClip>(_soundPath + "/move.wav");
        gm.captureSound = AssetDatabase.LoadAssetAtPath<AudioClip>(_soundPath + "/capture.wav");
        gm.crownSound = AssetDatabase.LoadAssetAtPath<AudioClip>(_soundPath + "/crown.wav");
        gm.winSound = AssetDatabase.LoadAssetAtPath<AudioClip>(_soundPath + "/win.wav");

        // Assigner le bg au Background
        var bg = GameObject.Find("Background");
        if (bg != null)
        {
            var img = bg.GetComponent<UnityEngine.UI.Image>();
            if (img != null) img.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(_spritePath + "/bg_game.png");
            else
            {
                var sr = bg.GetComponent<SpriteRenderer>();
                if (sr != null) sr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(_spritePath + "/bg_game.png");
            }
        }

        EditorSceneManager.SaveScene(scene);
    }

    static void AssignSpritesToAccueil()
    {
        string scenePath = _basePath + "/Scenes/Accueil_Dame.unity";
        var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

        var bg = GameObject.Find("BackGround");
        if (bg != null)
        {
            var img = bg.GetComponent<UnityEngine.UI.Image>();
            if (img != null) img.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(_spritePath + "/bg_accueil.png");
        }

        EditorSceneManager.SaveScene(scene);
    }

    static void AssignSpritesToScore()
    {
        string scenePath = _basePath + "/Scenes/Score_Dame.unity";
        var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

        var bg = GameObject.Find("BackGround");
        if (bg != null)
        {
            var img = bg.GetComponent<UnityEngine.UI.Image>();
            if (img != null) img.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(_spritePath + "/bg_score.png");
        }

        var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(_fontPath + "/Dame_Font.asset");
        if (font != null)
        {
            // Assigner la police a tous les TextMeshPro de la scene
            var texts = Object.FindObjectsByType<TextMeshProUGUI>(FindObjectsSortMode.None);
            foreach (var t in texts)
                t.font = font;
        }

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
        string themePath = _basePath + "/Themes";
        Directory.CreateDirectory(themePath + "/Classique");
        Directory.CreateDirectory(themePath + "/Bois");

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

        var bg = GameObject.Find("Background");
        if (bg == null) { Debug.LogError("Background pas trouve dans le menu"); return; }

        // Assigner le bg du menu
        var bgImg = bg.GetComponent<UnityEngine.UI.Image>();
        if (bgImg != null) bgImg.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(_spritePath + "/bg_menu.png");

        // === Theme dropdown ===
        var canvas = Object.FindFirstObjectByType<Canvas>();
        if (canvas == null) return;

        var diffGO = GameObject.Find("Difficulty");
        if (diffGO != null && GameObject.Find("ThemeDropdown") == null)
        {
            var themeGO = Object.Instantiate(diffGO, bg.transform);
            themeGO.name = "ThemeDropdown";
            var rt = themeGO.GetComponent<RectTransform>();
            if (rt != null) rt.anchoredPosition += Vector2.down * 150f;

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
        }

        // === InputField pour 2 joueurs ===
        if (GameObject.Find("PlayerNameInput") == null)
        {
            var pnPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Universal/Prefab/PlayerNameInput Template.prefab");
            if (pnPrefab != null)
            {
                var pnGO = Object.Instantiate(pnPrefab, bg.transform);
                pnGO.name = "PlayerNameInput";
                Debug.Log("✓ PlayersNameInput ajoute au menu");
            }
        }

        EditorSceneManager.SaveScene(scene);
    }

    static void AddSwapImageToScenes()
    {
        var scenes = new string[] { "Accueil_Dame.unity", "Menu_Dame.unity", "GameScene_Dame.unity", "Score_Dame.unity" };
        foreach (var fname in scenes)
        {
            string scenePath = _basePath + "/Scenes/" + fname;
            if (!File.Exists(scenePath)) continue;
            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

            var bg = GameObject.Find("Background") ?? GameObject.Find("BackGround");
            if (bg != null && bg.GetComponent<SwapImageBehaviour>() == null)
            {
                bg.AddComponent<SwapImageBehaviour>();
                Debug.Log("✓ SwapImageBehaviour ajoute sur " + bg.name + " dans " + fname);
            }

            EditorSceneManager.SaveScene(scene);
        }
    }
}