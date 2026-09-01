using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using Demolition;
using Theme;

public class JVS_SetupEditor : EditorWindow
{
    [MenuItem("Tools/JVS - Éditeur de Configuration", priority = 100)]
    public static void ShowWindow()
    {
        var win = GetWindow<JVS_SetupEditor>("JVS Setup");
        win.minSize = new Vector2(420, 500);
        win.maxSize = new Vector2(700, 800);
    }

    // ── Tab management ────────────────────────────────────────────
    private enum ProjectTab { Demolition, Dame }
    private ProjectTab _activeTab = ProjectTab.Demolition;
    private Vector2 _scrollPos;

    // ── Setup step definitions ────────────────────────────────────
    private class SetupStep
    {
        public string label;
        public string scenePath;
        public string sceneName;
        public string key;               // unique ID for done-key in EditorPrefs
        public Func<bool> isDone;        // returns true when scene is fully configured
        public Action action;            // runs the configuration
        public bool isLast;              // if true, draws a separator after
    }

    private Dictionary<ProjectTab, List<SetupStep>> _steps;
    private Dictionary<ProjectTab, string> _basePaths;

    // ── Colors / styling ──────────────────────────────────────────
    private static readonly Color ColorDone = new Color(0.1f, 0.7f, 0.2f);
    private static readonly Color ColorLocked = new Color(0.8f, 0.3f, 0.1f);
    private static readonly Color ColorTabActive = new Color(0.3f, 0.5f, 0.9f);
    private static readonly Color ColorTabInactive = new Color(0.25f, 0.25f, 0.25f);
    private static readonly Color ColorHeader = new Color(0.2f, 0.25f, 0.3f);
    private static readonly Color ColorDivider = new Color(0.15f, 0.15f, 0.15f);

    // ── Init ──────────────────────────────────────────────────────
    private void OnEnable()
    {
        InitializeSteps();
    }

    private void InitializeSteps()
    {
        _basePaths = new Dictionary<ProjectTab, string>
        {
            [ProjectTab.Demolition] = "Assets/Projects/Demolition",
            [ProjectTab.Dame] = "Assets/Projects/Dame",
        };

        _steps = new Dictionary<ProjectTab, List<SetupStep>>
        {
            [ProjectTab.Demolition] = new List<SetupStep>
            {
                new SetupStep
                {
                    label = "1. GameScene — Background, Sol, Canvas UI",
                    sceneName = "GameScene_Demolition",
                    scenePath = "Assets/Projects/Demolition/Demolition_Scenes/GameScene_Demolition.unity",
                    key = "JVS_Demo_GameScene",
                    isDone = () => DemolitionGameSceneReady(),
                    action = () => Demolition_SetupGameScene(),
                },
                new SetupStep
                {
                    label = "2. Accueil — Background",
                    sceneName = "Accueil_Demolition",
                    scenePath = "Assets/Projects/Demolition/Demolition_Scenes/Accueil_Demolition.unity",
                    key = "JVS_Demo_Accueil",
                    isDone = () => DemolitionAccueilReady(),
                    action = () => Demolition_SetupAccueil(),
                },
                new SetupStep
                {
                    label = "3. Menu — Background + UI options",
                    sceneName = "Menu_Demolition",
                    scenePath = "Assets/Projects/Demolition/Demolition_Scenes/Menu_Demolition.unity",
                    key = "JVS_Demo_Menu",
                    isDone = () => DemolitionMenuReady(),
                    action = () => Demolition_SetupMenuOld(),
                },
                new SetupStep
                {
                    label = "4. Score — Background",
                    sceneName = "Score_Demolition",
                    scenePath = "Assets/Projects/Demolition/Demolition_Scenes/Score_Demolition.unity",
                    key = "JVS_Demo_Score",
                    isDone = () => DemolitionScoreReady(),
                    action = () => Demolition_SetupScore(),
                    isLast = true,
                },
                new SetupStep
                {
                    label = "★ TOUT CONFIGURER — Assets + 4 scènes",
                    sceneName = null,
                    scenePath = null,
                    key = "JVS_Demo_All",
                    isDone = () => DemolitionGameSceneReady() && DemolitionAccueilReady() && DemolitionMenuReady() && DemolitionScoreReady(),
                    action = () => Demolition_ConfigTout(),
                },
            },

            [ProjectTab.Dame] = new List<SetupStep>
            {
                new SetupStep
                {
                    label = "1. GameScene — Sprites + Sons",
                    sceneName = "GameScene_Dame",
                    scenePath = "Assets/Projects/Dame/Scenes/GameScene_Dame.unity",
                    key = "JVS_Dame_GameScene",
                    isDone = () => DameGameSceneReady(),
                    action = () => Dame_SetupGameScene(),
                },
                new SetupStep
                {
                    label = "2. Accueil — Background",
                    sceneName = "Accueil_Dame",
                    scenePath = "Assets/Projects/Dame/Scenes/Accueil_Dame.unity",
                    key = "JVS_Dame_Accueil",
                    isDone = () => DameAccueilReady(),
                    action = () => Dame_SetupAccueil(),
                },
                new SetupStep
                {
                    label = "3. Menu — Background + UI options",
                    sceneName = "Menu_Dame",
                    scenePath = "Assets/Projects/Dame/Scenes/Menu_Dame.unity",
                    key = "JVS_Dame_Menu",
                    isDone = () => DameMenuReady(),
                    action = () => Dame_SetupMenu(),
                },
                new SetupStep
                {
                    label = "4. Score — Background + Fontes",
                    sceneName = "Score_Dame",
                    scenePath = "Assets/Projects/Dame/Scenes/Score_Dame.unity",
                    key = "JVS_Dame_Score",
                    isDone = () => DameScoreReady(),
                    action = () => Dame_SetupScore(),
                    isLast = true,
                },
                new SetupStep
                {
                    label = "★ TOUT CONFIGURER — Assets + 4 scènes",
                    sceneName = null,
                    scenePath = null,
                    key = "JVS_Dame_All",
                    isDone = () => DameGameSceneReady() && DameAccueilReady() && DameMenuReady() && DameScoreReady(),
                    action = () => Dame_ConfigTout(),
                },
            },
        };
    }

    // ── GUI ────────────────────────────────────────────────────────
    private void OnGUI()
    {
        DrawHeader();
        DrawTabs();
        _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);
        DrawStepsForTab(_activeTab);
        EditorGUILayout.EndScrollView();
        DrawFooter();
    }

    private void DrawHeader()
    {
        var headerRect = EditorGUILayout.BeginVertical();
        EditorGUI.DrawRect(headerRect, ColorHeader);
        GUILayout.Space(12);
        EditorGUILayout.LabelField("⚙ JVS Setup Editor", EditorStyles.boldLabel, GUILayout.Height(24));
        EditorGUILayout.LabelField("Configurez vos projets en un clic — les étapes faites disparaissent.", EditorStyles.miniLabel);
        GUILayout.Space(8);
        EditorGUILayout.EndVertical();
        EditorGUILayout.Space(4);
    }

    private void DrawTabs()
    {
        EditorGUILayout.BeginHorizontal();
        foreach (ProjectTab tab in Enum.GetValues(typeof(ProjectTab)))
        {
            var isActive = _activeTab == tab;
            var bgColor = isActive ? ColorTabActive : ColorTabInactive;
            var icon = tab == ProjectTab.Demolition ? "💥" : "👑";

            GUI.backgroundColor = bgColor;
            if (GUILayout.Button($"{icon}  {tab}", GUILayout.Height(32), GUILayout.MinWidth(120)))
            {
                _activeTab = tab;
                Repaint();
            }
            GUI.backgroundColor = Color.white;
        }
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();

        // divider
        var dividerRect = EditorGUILayout.BeginHorizontal();
        EditorGUI.DrawRect(dividerRect, ColorDivider);
        GUILayout.Space(1);
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space(6);
    }

    private void DrawStepsForTab(ProjectTab tab)
    {
        var steps = _steps[tab];
        int doneCount = 0;
        bool showLast = false;

        // Draw all non-done steps
        for (int i = 0; i < steps.Count; i++)
        {
            var step = steps[i];
            if (step.isDone())
            {
                doneCount++;
                if (step.isLast) { showLast = false; }
                continue;
            }

            DrawStepButton(tab, step);

            if (step.isLast)
            {
                showLast = true;
            }
        }

        // If everything is done, show a completion message
        if (doneCount >= steps.Count)
        {
            EditorGUILayout.Space(10);
            var msgRect = EditorGUILayout.BeginVertical();
            EditorGUI.DrawRect(msgRect, new Color(0.08f, 0.35f, 0.12f));
            GUILayout.Space(16);
            EditorGUILayout.LabelField($"✓  {tab} — Tout est configuré !", EditorStyles.boldLabel, GUILayout.Height(24));
            GUILayout.Space(4);
            EditorGUILayout.LabelField("Aucune action nécessaire.", EditorStyles.miniLabel);
            GUILayout.Space(16);
            EditorGUILayout.EndVertical();
        }

        // Progress bar
        EditorGUILayout.Space(8);
        float progress = steps.Count > 0 ? (float)doneCount / steps.Count : 0;
        var progressRect = EditorGUILayout.BeginVertical();
        EditorGUI.ProgressBar(progressRect, progress, $"{doneCount}/{steps.Count} étapes faites");
        GUILayout.Space(20);
        EditorGUILayout.EndVertical();
    }

    private void DrawStepButton(ProjectTab tab, SetupStep step)
    {
        // Scene-open guard
        bool sceneOpen = false;
        if (!string.IsNullOrEmpty(step.scenePath))
        {
            var scene = EditorSceneManager.GetSceneByPath(step.scenePath);
            sceneOpen = scene.IsValid() && scene.isLoaded;
        }

        // Check if we're in play mode (always block)
        bool inPlayMode = EditorApplication.isPlaying;

        bool disabled = sceneOpen || inPlayMode;

        if (disabled)
        {
            GUI.enabled = false;
        }

        string lockIcon = "";
        string lockHint = "";
        if (inPlayMode)
        {
            lockIcon = " ⚠";
            lockHint = "Arrêtez le mode Play d'abord";
        }
        else if (sceneOpen)
        {
            lockIcon = " 🔒";
            lockHint = "Fermez la scène avant de cliquer";
        }

        // Styling
        var defaultBg = GUI.backgroundColor;
        if (disabled)
        {
            GUI.backgroundColor = ColorLocked;
        }
        else
        {
            GUI.backgroundColor = new Color(0.25f, 0.45f, 0.7f);
        }

        var btnLabel = step.label;
        if (!string.IsNullOrEmpty(lockHint))
        {
            btnLabel = $"🔒  {step.label}  —  {lockHint}";
        }

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        if (GUILayout.Button($"  {btnLabel}", GUILayout.Height(40)))
        {
            if (!disabled)
            {
                RunStep(step);
            }
        }
        EditorGUILayout.EndVertical();

        GUI.backgroundColor = defaultBg;
        GUI.enabled = true;

        EditorGUILayout.Space(3);
    }

    private void RunStep(SetupStep step)
    {
        step.action();
        Repaint();
        // Force Unity to repaint the window after the action completes
        EditorApplication.delayCall += Repaint;
    }

    private void DrawFooter()
    {
        EditorGUILayout.Space(4);
        var footerRect = EditorGUILayout.BeginVertical();
        EditorGUI.DrawRect(footerRect, new Color(0.12f, 0.12f, 0.14f));
        GUILayout.Space(6);
        EditorGUILayout.LabelField("JVS Framework — Les boutons disparaissent quand une étape est faite.", EditorStyles.centeredGreyMiniLabel);
        GUILayout.Space(4);
        EditorGUILayout.EndVertical();
    }

    // ════════════════════════════════════════════════════════════════
    //  VERIFICATION HELPERS
    // ════════════════════════════════════════════════════════════════

    // ── Demolition ────────────────────────────────────────────────
    private static bool DemolitionGameSceneReady()
    {
        string path = "Assets/Projects/Demolition/Demolition_Scenes/GameScene_Demolition.unity";
        if (!File.Exists(path)) return false;

        var scene = EditorSceneManager.GetSceneByPath(path);
        bool opened = false;
        if (!scene.IsValid() || !scene.isLoaded)
        {
            scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
            opened = true;
        }

        bool ready =
            Object.FindFirstObjectByType<Demolition_GameManager>() != null &&
            GameObject.Find("Ground") != null &&
            GameObject.Find("Background") != null &&
            GameObject.Find("Canvas") != null;

        if (opened && scene.IsValid())
            EditorSceneManager.CloseScene(scene, true);

        return ready;
    }

    private static bool DemolitionAccueilReady()
    {
        string path = "Assets/Projects/Demolition/Demolition_Scenes/Accueil_Demolition.unity";
        if (!File.Exists(path)) return false;

        var scene = EditorSceneManager.GetSceneByPath(path);
        bool opened = false;
        if (!scene.IsValid() || !scene.isLoaded)
        { scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Additive); opened = true; }

        bool ready = HasCanvasBackground();

        if (opened && scene.IsValid()) EditorSceneManager.CloseScene(scene, true);
        return ready;
    }

    private static bool DemolitionMenuReady()
    {
        string path = "Assets/Projects/Demolition/Demolition_Scenes/Menu_Demolition.unity";
        if (!File.Exists(path)) return false;

        var scene = EditorSceneManager.GetSceneByPath(path);
        bool opened = false;
        if (!scene.IsValid() || !scene.isLoaded)
        { scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Additive); opened = true; }

        bool ready = HasCanvasBackground() && GameObject.Find("ModeOiseau") != null && GameObject.Find("ScrollSpeed") != null;

        if (opened && scene.IsValid()) EditorSceneManager.CloseScene(scene, true);
        return ready;
    }

    private static bool DemolitionScoreReady()
    {
        string path = "Assets/Projects/Demolition/Demolition_Scenes/Score_Demolition.unity";
        if (!File.Exists(path)) return false;

        var scene = EditorSceneManager.GetSceneByPath(path);
        bool opened = false;
        if (!scene.IsValid() || !scene.isLoaded)
        { scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Additive); opened = true; }

        bool ready = HasCanvasBackground();

        if (opened && scene.IsValid()) EditorSceneManager.CloseScene(scene, true);
        return ready;
    }

    // ── Dame ──────────────────────────────────────────────────────
    private static bool DameGameSceneReady()
    {
        string path = "Assets/Projects/Dame/Scenes/GameScene_Dame.unity";
        if (!File.Exists(path)) return false;

        var scene = EditorSceneManager.GetSceneByPath(path);
        bool opened = false;
        if (!scene.IsValid() || !scene.isLoaded)
        { scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Additive); opened = true; }

        bool ready = false;
        var gm = Object.FindFirstObjectByType<Dame.Dame_GameManager>();
        ready = gm != null && gm.caseFoncee != null;

        if (opened && scene.IsValid()) EditorSceneManager.CloseScene(scene, true);
        return ready;
    }

    private static bool DameAccueilReady()
    {
        string path = "Assets/Projects/Dame/Scenes/Accueil_Dame.unity";
        if (!File.Exists(path)) return false;

        var scene = EditorSceneManager.GetSceneByPath(path);
        bool opened = false;
        if (!scene.IsValid() || !scene.isLoaded)
        { scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Additive); opened = true; }

        bool ready = HasCanvasBackground();

        if (opened && scene.IsValid()) EditorSceneManager.CloseScene(scene, true);
        return ready;
    }

    private static bool DameMenuReady()
    {
        string path = "Assets/Projects/Dame/Scenes/Menu_Dame.unity";
        if (!File.Exists(path)) return false;

        var scene = EditorSceneManager.GetSceneByPath(path);
        bool opened = false;
        if (!scene.IsValid() || !scene.isLoaded)
        { scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Additive); opened = true; }

        bool ready = HasCanvasBackground() &&
                     GameObject.Find("ThemeDropdown") != null &&
                     GameObject.Find("PlayerNameInput") != null;

        if (opened && scene.IsValid()) EditorSceneManager.CloseScene(scene, true);
        return ready;
    }

    private static bool DameScoreReady()
    {
        string path = "Assets/Projects/Dame/Scenes/Score_Dame.unity";
        if (!File.Exists(path)) return false;

        var scene = EditorSceneManager.GetSceneByPath(path);
        bool opened = false;
        if (!scene.IsValid() || !scene.isLoaded)
        { scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Additive); opened = true; }

        bool ready = HasCanvasBackground();

        if (opened && scene.IsValid()) EditorSceneManager.CloseScene(scene, true);
        return ready;
    }

    // ── Shared helpers ────────────────────────────────────────────
    private static bool HasCanvasBackground()
    {
        var canvas = Object.FindFirstObjectByType<Canvas>();
        if (canvas == null) return false;
        for (int i = 0; i < canvas.transform.childCount; i++)
        {
            if (canvas.transform.GetChild(i).name == "Background")
                return true;
        }
        return false;
    }

    // ════════════════════════════════════════════════════════════════
    //  DEMOLITION — SETUP ACTIONS (translated from existing tool)
    // ════════════════════════════════════════════════════════════════
    private static string _demoBase = "Assets/Projects/Demolition";
    private static string _demoPrefab => _demoBase + "/Resources/Prefabs";
    private static string _demoTex => _demoBase + "/Resources/Textures";
    private static string _demoSound => _demoBase + "/Resources/Sounds";

    private static void Demolition_SetupGameScene()
    {
        var scene = EditorSceneManager.OpenScene("Assets/Projects/Demolition/Demolition_Scenes/GameScene_Demolition.unity");
        EnsureCamera();
        CleanOrphanBackgrounds();
        SetupGameSceneCanvas();
        SetupCanvasBackground("bg_game", false);
        SetupGameSceneGround();
        if (GameObject.Find("StructuresParent") == null) new GameObject("StructuresParent");
        if (Object.FindFirstObjectByType<Demolition_GameManager>() == null)
        {
            var gmGO = new GameObject("Demolition_GameManager", typeof(Demolition_GameManager));
            var gm = gmGO.GetComponent<Demolition_GameManager>();
            var aud = gmGO.AddComponent<AudioSource>();
            aud.playOnAwake = false;
            gm.impactSound = AssetDatabase.LoadAssetAtPath<AudioClip>(_demoSound + "/impact.wav");
            gm.destructionSound = AssetDatabase.LoadAssetAtPath<AudioClip>(_demoSound + "/destruction.wav");
            gm.oiseauPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(_demoPrefab + "/Oiseau.prefab");
            gm.impactEffectPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(_demoPrefab + "/ImpactExplosion.prefab");
        }
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("[JVS] ✓ GameScene_Demolition configurée");
    }

    private static void Demolition_SetupAccueil()
    {
        var scene = EditorSceneManager.OpenScene("Assets/Projects/Demolition/Demolition_Scenes/Accueil_Demolition.unity");
        EnsureCamera();
        CleanOrphanBackgrounds();
        SetupCanvasBackground("bg_accueil", true);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("[JVS] ✓ Accueil_Demolition configurée");
    }

    private static void Demolition_SetupMenuOld()
    {
        var scene = EditorSceneManager.OpenScene("Assets/Projects/Demolition/Demolition_Scenes/Menu_Demolition.unity");
        EnsureCamera();
        CleanOrphanBackgrounds();
        SetupCanvasBackground("bg_menu", true);
        SetupDemolitionMenuUI();
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("[JVS] ✓ Menu_Demolition configuré");
    }

    private static void Demolition_SetupScore()
    {
        var scene = EditorSceneManager.OpenScene("Assets/Projects/Demolition/Demolition_Scenes/Score_Demolition.unity");
        EnsureCamera();
        CleanOrphanBackgrounds();
        SetupCanvasBackground("bg_score", true);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("[JVS] ✓ Score_Demolition configurée");
    }

    private static void Demolition_ConfigTout()
    {
        Directory.CreateDirectory(_demoPrefab);
        Directory.CreateDirectory(_demoTex);
        Directory.CreateDirectory(_demoSound);

        MakePNG("bois", 64, 32, new Color(0.545f, 0.353f, 0.169f));
        MakePNG("verre", 64, 32, new Color(0.7f, 0.85f, 0.9f));
        MakePNG("pierre", 64, 32, new Color(0.5f, 0.5f, 0.5f));
        MakePNG("oiseau_dos", 32, 32, new Color(1, 0.5f, 0));
        MakePNG("impact", 32, 32, new Color(1, 0.8f, 0));
        MakePNG("debris_bois", 16, 16, new Color(0.6f, 0.4f, 0.2f));
        MakePNG("debris_verre", 16, 16, new Color(0.7f, 0.85f, 0.9f));
        MakePNG("debris_pierre", 16, 16, new Color(0.5f, 0.5f, 0.5f));
        MakePNG("debris_cochon", 16, 16, new Color(0.9f, 0.5f, 0.5f));
        MakePNG("fissure1", 16, 16, new Color(0.3f, 0.3f, 0.3f));
        MakePNG("fissure2", 16, 16, new Color(0.2f, 0.2f, 0.2f));
        MakePNG("sol", 128, 32, new Color(0.4f, 0.3f, 0.2f));
        MakePNG("star_1", 32, 32, new Color(1, 0.8f, 0));
        MakePNG("star_2", 32, 32, new Color(1, 0.9f, 0.2f));
        MakePNG("star_3", 32, 32, new Color(1, 1, 0.4f));

        MakeWAV(_demoSound, "impact", 0.15f, 200, 0.3f);
        MakeWAV(_demoSound, "destruction", 0.3f, 150, 0.5f);
        MakeWAV(_demoSound, "gameover", 0.5f, 100, 0.8f);
        MakeWAV(_demoSound, "pig_hit", 0.2f, 300, 0.4f);

        AssetDatabase.Refresh();

        var t_bois = LoadSprite(_demoTex, "bois");
        var t_verre = LoadSprite(_demoTex, "verre");
        var t_pierre = LoadSprite(_demoTex, "pierre");
        if (t_bois != null)
        {
            var t_f1 = LoadSprite(_demoTex, "fissure1");
            var t_f2 = LoadSprite(_demoTex, "fissure2");
            var t_oiseau = LoadSprite(_demoTex, "oiseau_dos");
            var t_impact = LoadSprite(_demoTex, "impact");
            var t_db = LoadSprite(_demoTex, "debris_bois");
            var t_dv = LoadSprite(_demoTex, "debris_verre");
            var t_dp = LoadSprite(_demoTex, "debris_pierre");
            var t_dc = LoadSprite(_demoTex, "debris_cochon");

            CreateBloc(_demoPrefab, "Bloc_Bois", t_bois, Demolition_Block.MaterialType.Bois, 4, 50, t_f1, t_f2);
            CreateBloc(_demoPrefab, "Bloc_Verre", t_verre, Demolition_Block.MaterialType.Verre, 2, 80, t_f1, t_f2);
            CreateBloc(_demoPrefab, "Bloc_Pierre", t_pierre, Demolition_Block.MaterialType.Pierre, 8, 40, t_f1, t_f2);
            CreateDebris(_demoPrefab, "Debris_Bois", t_db);
            CreateDebris(_demoPrefab, "Debris_Verre", t_dv);
            CreateDebris(_demoPrefab, "Debris_Pierre", t_dp);
            CreateDebris(_demoPrefab, "Debris_Cochon", t_dc);
            CreateOiseau(_demoPrefab, t_oiseau, t_impact);
            CreateCochonPrefabs();
            CreatePopupTextPrefab();
        }

        Demolition_SetupGameScene();
        Demolition_SetupAccueil();
        Demolition_SetupMenuOld();
        Demolition_SetupScore();

        AssetDatabase.Refresh();
        Debug.Log("[JVS] ✓ Démolition complètement configuré !");
    }

    // ════════════════════════════════════════════════════════════════
    //  DAME — SETUP ACTIONS
    // ════════════════════════════════════════════════════════════════
    private static string _dameBase = "Assets/Projects/Dame";
    private static string _dameSprite => _dameBase + "/Sprites";
    private static string _dameSound => _dameBase + "/Sons";
    private static string _dameFont => _dameBase + "/Font";

    private static void Dame_SetupGameScene()
    {
        var scene = EditorSceneManager.OpenScene("Assets/Projects/Dame/Scenes/GameScene_Dame.unity");
        var gm = Object.FindFirstObjectByType<Dame.Dame_GameManager>();
        if (gm == null) { Debug.LogWarning("GameManager pas trouvé dans GameScene !"); return; }
        gm.caseFoncee = AssetDatabase.LoadAssetAtPath<Sprite>(_dameSprite + "/case_foncee.png");
        gm.caseClaire = AssetDatabase.LoadAssetAtPath<Sprite>(_dameSprite + "/case_claire.png");
        gm.pionBlanc = AssetDatabase.LoadAssetAtPath<Sprite>(_dameSprite + "/pion_blanc.png");
        gm.pionNoir = AssetDatabase.LoadAssetAtPath<Sprite>(_dameSprite + "/pion_noir.png");
        gm.dameBlanche = AssetDatabase.LoadAssetAtPath<Sprite>(_dameSprite + "/dame_blanche.png");
        gm.dameNoire = AssetDatabase.LoadAssetAtPath<Sprite>(_dameSprite + "/dame_noire.png");
        gm.moveSound = AssetDatabase.LoadAssetAtPath<AudioClip>(_dameSound + "/move.wav");
        gm.captureSound = AssetDatabase.LoadAssetAtPath<AudioClip>(_dameSound + "/capture.wav");
        gm.crownSound = AssetDatabase.LoadAssetAtPath<AudioClip>(_dameSound + "/crown.wav");
        gm.winSound = AssetDatabase.LoadAssetAtPath<AudioClip>(_dameSound + "/win.wav");
        AssignSpriteToBackground("bg_game.png");
        EditorSceneManager.SaveScene(scene);
        Debug.Log("[JVS] ✓ GameScene_Dame configurée");
    }

    private static void Dame_SetupAccueil()
    {
        var scene = EditorSceneManager.OpenScene("Assets/Projects/Dame/Scenes/Accueil_Dame.unity");
        AssignSpriteToBackground("bg_accueil.png");
        EditorSceneManager.SaveScene(scene);
        Debug.Log("[JVS] ✓ Accueil_Dame configurée");
    }

    private static void Dame_SetupMenu()
    {
        var scene = EditorSceneManager.OpenScene("Assets/Projects/Dame/Scenes/Menu_Dame.unity");
        AssignSpriteToBackground("bg_menu.png");

        var bg = GameObject.Find("Background") ?? GameObject.Find("BackGround");
        if (bg == null) { Debug.LogError("Background pas trouvé dans le menu"); return; }

        // Theme dropdown
        var canvas = Object.FindFirstObjectByType<Canvas>();
        if (canvas != null && GameObject.Find("ThemeDropdown") == null)
        {
            var diffGO = GameObject.Find("Difficulty");
            if (diffGO != null)
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
                var ts = themeGO.AddComponent<ThemeSelector>();
                var tm = AssetDatabase.LoadAssetAtPath<ThemeManager>(_dameBase + "/Themes/Dame_ThemeManager.asset");
                if (tm != null)
                {
                    var field = typeof(ThemeSelector).GetField("_themeManager",
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    if (field != null) field.SetValue(ts, tm);
                }
            }
        }

        // Player name input
        if (GameObject.Find("PlayerNameInput") == null)
        {
            var pnPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Universal/Prefab/PlayerNameInput Template.prefab");
            if (pnPrefab != null)
            {
                var pnGO = Object.Instantiate(pnPrefab, bg.transform);
                pnGO.name = "PlayerNameInput";
            }
        }

        // SwapImage on bg
        if (bg != null && bg.GetComponent<SwapImageBehaviour>() == null)
            bg.AddComponent<SwapImageBehaviour>();

        EditorSceneManager.SaveScene(scene);
        Debug.Log("[JVS] ✓ Menu_Dame configuré");
    }

    private static void Dame_SetupScore()
    {
        var scene = EditorSceneManager.OpenScene("Assets/Projects/Dame/Scenes/Score_Dame.unity");
        AssignSpriteToBackground("bg_score.png");

        var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(_dameFont + "/Dame_Font.asset");
        if (font != null)
        {
            var texts = Object.FindObjectsByType<TextMeshProUGUI>(FindObjectsSortMode.None);
            foreach (var t in texts) t.font = font;
        }

        var bg = GameObject.Find("Background") ?? GameObject.Find("BackGround");
        if (bg != null && bg.GetComponent<SwapImageBehaviour>() == null)
            bg.AddComponent<SwapImageBehaviour>();

        EditorSceneManager.SaveScene(scene);
        Debug.Log("[JVS] ✓ Score_Dame configurée");
    }

    private static void Dame_ConfigTout()
    {
        Dame_SetupGameScene();
        Dame_SetupAccueil();
        Dame_SetupMenu();
        Dame_SetupScore();
        Debug.Log("[JVS] ✓ Dame complètement configuré !");
    }

    // ════════════════════════════════════════════════════════════════
    //  SHARED HELPERS
    // ════════════════════════════════════════════════════════════════

    private static void EnsureCamera()
    {
        var cam = Object.FindFirstObjectByType<Camera>();
        if (cam == null)
        {
            var go = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
            cam = go.GetComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 5;
            cam.clearFlags = CameraClearFlags.Color;
            cam.backgroundColor = new Color(0.05f, 0.05f, 0.08f);
            go.transform.position = new Vector3(0, 0, -10);
            go.tag = "MainCamera";
        }
        else
        {
            cam.orthographic = true;
            cam.orthographicSize = 5;
        }
    }

    private static void CleanOrphanBackgrounds()
    {
        var rootGOs = SceneManager.GetActiveScene().GetRootGameObjects();
        foreach (var go in rootGOs)
        {
            if (go != null && (go.name == "Background" || go.name.StartsWith("bg_")))
            {
                if (go.GetComponent<Canvas>() == null)
                    Undo.DestroyObjectImmediate(go);
            }
        }
    }

    private static void SetupCanvasBackground(string texName, bool raycastTarget)
    {
        var sprite = LoadSprite(_demoTex, texName);
        if (sprite == null) sprite = LoadSprite(_demoTex, "bg_accueil");

        var canvas = Object.FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            var canvasGO = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvas = canvasGO.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGO.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
        }

        Transform bgTransform = null;
        for (int i = 0; i < canvas.transform.childCount; i++)
        {
            var child = canvas.transform.GetChild(i);
            if (child.name == "Background")
            {
                if (bgTransform == null) bgTransform = child;
                else Undo.DestroyObjectImmediate(child.gameObject);
            }
        }

        if (bgTransform == null)
        {
            var bgGO = new GameObject("Background", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            bgGO.transform.SetParent(canvas.transform, false);
            bgTransform = bgGO.transform;
        }

        bgTransform.SetAsFirstSibling();

        var rt = bgTransform.GetComponent<RectTransform>();
        if (rt != null)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.localScale = Vector3.one;
        }

        var img = bgTransform.GetComponent<Image>();
        if (img == null) img = bgTransform.gameObject.AddComponent<Image>();
        if (sprite != null)
        {
            img.sprite = sprite;
            img.color = Color.white;
            img.type = Image.Type.Simple;
            img.preserveAspect = false;
        }
        img.raycastTarget = raycastTarget;
    }

    private static void SetupGameSceneGround()
    {
        var ground = GameObject.Find("Ground");
        if (ground == null)
            ground = new GameObject("Ground", typeof(BoxCollider2D), typeof(SpriteRenderer), typeof(Demolition_GroundScroll));

        ground.transform.position = new Vector3(0, -5.2f, 0);
        ground.transform.localScale = Vector3.one;

        var groundCol = ground.GetComponent<BoxCollider2D>();
        if (groundCol == null) groundCol = ground.AddComponent<BoxCollider2D>();
        groundCol.size = new Vector2(300, 2.4f);
        groundCol.offset = Vector2.zero;

        var groundSr = ground.GetComponent<SpriteRenderer>();
        if (groundSr == null) groundSr = ground.AddComponent<SpriteRenderer>();
        groundSr.sortingOrder = 2;
        groundSr.drawMode = SpriteDrawMode.Tiled;
        groundSr.size = new Vector2(300, 2.4f);
        groundSr.color = Color.white;

        var solSprite = LoadSprite(_demoTex, "sol");
        if (solSprite != null) groundSr.sprite = solSprite;

        if (ground.GetComponent<Demolition_GroundScroll>() == null)
            ground.AddComponent<Demolition_GroundScroll>();
    }

    private static void SetupGameSceneCanvas()
    {
        var canvas = Object.FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            var canvasGO = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvas = canvasGO.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGO.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
        }

        if (GameObject.Find("ScoreText") == null)
        {
            var scoreGO = new GameObject("ScoreText", typeof(RectTransform));
            scoreGO.transform.SetParent(canvas.transform);
            var srt = scoreGO.GetComponent<RectTransform>();
            srt.anchorMin = new Vector2(0, 1);
            srt.anchorMax = new Vector2(0, 1);
            srt.pivot = new Vector2(0, 1);
            srt.anchoredPosition = new Vector2(30, -30);
            srt.sizeDelta = new Vector2(300, 80);
            var scoreTxt = scoreGO.AddComponent<TextMeshProUGUI>();
            scoreTxt.text = "Score: 0";
            scoreTxt.fontSize = 48;
            scoreTxt.color = Color.white;
            scoreTxt.alignment = TextAlignmentOptions.TopLeft;
            scoreGO.AddComponent<CanvasRenderer>();
        }

        if (GameObject.Find("TimerText") == null)
        {
            var timerGO = new GameObject("TimerText", typeof(RectTransform));
            timerGO.transform.SetParent(canvas.transform);
            var trt = timerGO.GetComponent<RectTransform>();
            trt.anchorMin = new Vector2(1, 1);
            trt.anchorMax = new Vector2(1, 1);
            trt.pivot = new Vector2(1, 1);
            trt.anchoredPosition = new Vector2(-30, -30);
            trt.sizeDelta = new Vector2(200, 80);
            var timerTxt = timerGO.AddComponent<TextMeshProUGUI>();
            timerTxt.text = "60";
            timerTxt.fontSize = 48;
            timerTxt.color = Color.white;
            timerTxt.alignment = TextAlignmentOptions.TopRight;
            timerGO.AddComponent<CanvasRenderer>();
        }

        if (GameObject.Find("StarText") == null)
        {
            var starGO = new GameObject("StarText", typeof(RectTransform));
            starGO.transform.SetParent(canvas.transform);
            var strt = starGO.GetComponent<RectTransform>();
            strt.anchorMin = new Vector2(0.5f, 1);
            strt.anchorMax = new Vector2(0.5f, 1);
            strt.pivot = new Vector2(0.5f, 1);
            strt.anchoredPosition = new Vector2(0, -35);
            strt.sizeDelta = new Vector2(400, 60);
            var starTxt = starGO.AddComponent<TextMeshProUGUI>();
            starTxt.text = "★";
            starTxt.fontSize = 42;
            starTxt.color = Color.yellow;
            starTxt.alignment = TextAlignmentOptions.Center;
            starGO.AddComponent<CanvasRenderer>();
        }
    }

    private static void SetupDemolitionMenuUI()
    {
        var canvas = Object.FindFirstObjectByType<Canvas>();
        if (canvas == null) return;

        if (GameObject.Find("ModeOiseau") == null)
        {
            var container = new GameObject("ModeOiseau", typeof(RectTransform));
            container.transform.SetParent(canvas.transform);
            var crt = container.GetComponent<RectTransform>();
            crt.anchorMin = Vector2.one * 0.5f;
            crt.anchorMax = Vector2.one * 0.5f;
            crt.anchoredPosition = new Vector2(386, -153);
            crt.sizeDelta = new Vector2(1221, 150);
            crt.localScale = Vector3.one * 0.5f;

            var label = new GameObject("Label", typeof(RectTransform));
            label.transform.SetParent(container.transform);
            var lrt = label.GetComponent<RectTransform>();
            lrt.anchorMin = Vector2.zero;
            lrt.anchorMax = Vector2.one;
            lrt.offsetMin = Vector2.zero;
            lrt.offsetMax = Vector2.zero;
            var txt = label.AddComponent<TextMeshProUGUI>();
            txt.text = "Mode Oiseau :";
            txt.fontSize = 102;
            txt.color = Color.white;
            txt.alignment = TextAlignmentOptions.MidlineLeft;
            txt.margin = new Vector4(5, 0, 0, 0);
            label.AddComponent<CanvasRenderer>();

            var toggleGO = new GameObject("Toggle", typeof(RectTransform));
            toggleGO.transform.SetParent(container.transform);
            var trt = toggleGO.GetComponent<RectTransform>();
            trt.anchorMin = Vector2.one * 0.5f;
            trt.anchorMax = Vector2.one * 0.5f;
            trt.anchoredPosition = new Vector2(548, -12.5f);
            trt.sizeDelta = new Vector2(125, 125);
            var toggle = toggleGO.AddComponent<Toggle>();
            toggle.isOn = PlayerPrefs.GetInt(Demolition_GeneralVariables.ModeOiseauKey, 1) == 1;
            toggleGO.AddComponent<CanvasRenderer>();
            var bgImg = toggleGO.AddComponent<Image>();
            bgImg.color = new Color(0.96f, 0.64f, 0);

            var check = new GameObject("Checkmark", typeof(RectTransform));
            check.transform.SetParent(toggleGO.transform);
            var crt2 = check.GetComponent<RectTransform>();
            crt2.anchorMin = Vector2.zero;
            crt2.anchorMax = Vector2.one;
            crt2.offsetMin = Vector2.zero;
            crt2.offsetMax = Vector2.zero;
            var checkImg = check.AddComponent<Image>();
            checkImg.sprite = LoadSprite(_demoTex, "bois");
            check.AddComponent<CanvasRenderer>();
            toggle.graphic = checkImg;
            toggle.targetGraphic = bgImg;
        }

        if (GameObject.Find("ScrollSpeed") == null)
        {
            var container = new GameObject("ScrollSpeed", typeof(RectTransform));
            container.transform.SetParent(canvas.transform);
            var crt = container.GetComponent<RectTransform>();
            crt.anchorMin = Vector2.one * 0.5f;
            crt.anchorMax = Vector2.one * 0.5f;
            crt.anchoredPosition = new Vector2(-691, -73);
            crt.sizeDelta = new Vector2(500, 400);
            crt.localScale = Vector3.one * 0.8f;

            var label = new GameObject("Label", typeof(RectTransform));
            label.transform.SetParent(container.transform);
            var lrt = label.GetComponent<RectTransform>();
            lrt.anchorMin = Vector2.one * 0.5f;
            lrt.anchorMax = Vector2.one * 0.5f;
            lrt.anchoredPosition = new Vector2(0, -19.2f);
            lrt.sizeDelta = new Vector2(200, 50);
            var txt = label.AddComponent<TextMeshProUGUI>();
            txt.text = "Vitesse";
            txt.fontSize = 77;
            txt.color = new Color(0.745f, 0.643f, 0.431f);
            txt.alignment = TextAlignmentOptions.Center;
            label.AddComponent<CanvasRenderer>();

            var sliderGO = new GameObject("Slider", typeof(RectTransform));
            sliderGO.transform.SetParent(container.transform);
            var srt = sliderGO.GetComponent<RectTransform>();
            srt.anchorMin = new Vector2(0, 1);
            srt.anchorMax = Vector2.one;
            srt.anchoredPosition = Vector2.zero;
            srt.sizeDelta = new Vector2(0, 200);
            var slider = sliderGO.AddComponent<Slider>();
            sliderGO.AddComponent<CanvasRenderer>();
            var sliderImg = sliderGO.AddComponent<Image>();
            sliderImg.color = Color.white;

            var fill = new GameObject("Fill", typeof(RectTransform));
            fill.transform.SetParent(sliderGO.transform);
            fill.AddComponent<Image>();
            var fillImg = fill.GetComponent<Image>();
            fillImg.color = new Color(0.745f, 0.643f, 0.431f);
            var frt = fill.GetComponent<RectTransform>();
            frt.anchorMin = new Vector2(0, 0.25f);
            frt.anchorMax = new Vector2(1, 0.75f);
            frt.sizeDelta = Vector2.zero;
            slider.fillRect = frt;

            var handle = new GameObject("Handle", typeof(RectTransform));
            handle.transform.SetParent(sliderGO.transform);
            handle.AddComponent<Image>();
            var hrt = handle.GetComponent<RectTransform>();
            hrt.anchorMin = new Vector2(0, 0);
            hrt.anchorMax = Vector2.one;
            hrt.pivot = new Vector2(0.5f, 0.5f);
            hrt.anchoredPosition = Vector2.zero;
            hrt.sizeDelta = new Vector2(20, 20);
            slider.handleRect = hrt;
            slider.targetGraphic = handle.GetComponent<Image>();

            slider.minValue = 1; slider.maxValue = 5; slider.wholeNumbers = true;
            slider.value = PlayerPrefs.GetFloat(Demolition_GeneralVariables.ScrollSpeedKey, 2f);

            var bg = new GameObject("Background", typeof(RectTransform));
            bg.transform.SetParent(sliderGO.transform);
            var brt = bg.GetComponent<RectTransform>();
            brt.anchorMin = new Vector2(0, 0.25f);
            brt.anchorMax = new Vector2(1, 0.75f);
            brt.sizeDelta = Vector2.zero;
            bg.AddComponent<Image>();
            bg.GetComponent<Image>().color = new Color(0.5f, 0.5f, 0.5f);
        }
    }

    private static void AssignSpriteToBackground(string spriteFile)
    {
        string[] bgNames = { "Background", "BackGround" };
        foreach (var name in bgNames)
        {
            var bg = GameObject.Find(name);
            if (bg != null)
            {
                var img = bg.GetComponent<Image>();
                if (img != null)
                {
                    img.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(_dameSprite + "/" + spriteFile);
                    return;
                }
                var sr = bg.GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    sr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(_dameSprite + "/" + spriteFile);
                    return;
                }
            }
        }
    }

    // ── Asset generation ──────────────────────────────────────────
    private static void MakePNG(string name, int w, int h, Color c)
    {
        string path = _demoTex + "/" + name + ".png";
        if (File.Exists(path)) return;
        var tex = new Texture2D(w, h);
        for (int x = 0; x < w; x++) for (int y = 0; y < h; y++) tex.SetPixel(x, y, c);
        tex.Apply();
        File.WriteAllBytes(path, tex.EncodeToPNG());
        Object.DestroyImmediate(tex);
    }

    private static Sprite LoadSprite(string basePath, string name)
    {
        AssetDatabase.Refresh();
        return AssetDatabase.LoadAssetAtPath<Sprite>(basePath + "/" + name + ".png");
    }

    private static void MakeWAV(string dir, string name, float dur, float freq, float vol)
    {
        string path = dir + "/" + name + ".wav";
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

    private static void CreateBloc(string prefabDir, string name, Sprite sprite, Demolition_Block.MaterialType mat, int hp, int pts, Sprite f1, Sprite f2)
    {
        var go = new GameObject(name, typeof(SpriteRenderer), typeof(BoxCollider2D), typeof(Rigidbody2D), typeof(AudioSource), typeof(Demolition_Block));
        var sr = go.GetComponent<SpriteRenderer>(); sr.sprite = sprite; sr.sortingOrder = 3;
        var blk = go.GetComponent<Demolition_Block>();
        blk.hp = hp; blk.points = pts; blk.materialType = mat; blk.spriteRenderer = sr;
        blk.damageSprites = new Sprite[] { f1, f2 };
        PrefabUtility.SaveAsPrefabAsset(go, prefabDir + "/" + name + ".prefab");
        Object.DestroyImmediate(go);
    }

    private static void CreateDebris(string prefabDir, string name, Sprite sprite)
    {
        var go = new GameObject(name, typeof(SpriteRenderer));
        go.GetComponent<SpriteRenderer>().sprite = sprite;
        PrefabUtility.SaveAsPrefabAsset(go, prefabDir + "/" + name + ".prefab");
        Object.DestroyImmediate(go);
    }

    private static void CreateOiseau(string prefabDir, Sprite oiSprite, Sprite imSprite)
    {
        var imp = new GameObject("ImpactExplosion", typeof(SpriteRenderer));
        imp.GetComponent<SpriteRenderer>().sprite = imSprite;
        imp.GetComponent<SpriteRenderer>().sortingOrder = 5;
        PrefabUtility.SaveAsPrefabAsset(imp, prefabDir + "/ImpactExplosion.prefab");
        Object.DestroyImmediate(imp);

        var go = new GameObject("Oiseau", typeof(SpriteRenderer), typeof(Demolition_Projectile));
        go.GetComponent<SpriteRenderer>().sprite = oiSprite;
        go.GetComponent<SpriteRenderer>().sortingOrder = 8;
        var p = go.GetComponent<Demolition_Projectile>();
        p.flightDuration = 0.14f;
        p.scaleStart = 1.4f;
        p.scaleEnd = 0.55f;
        p.hitRadius = 0.35f;
        p.pushForce = 2.2f;
        p.directDamage = 1;
        PrefabUtility.SaveAsPrefabAsset(go, prefabDir + "/Oiseau.prefab");
        Object.DestroyImmediate(go);
    }

    private static void CreateCochonPrefabs()
    {
        var t_cochon = LoadSprite(_demoTex, "cochon");
        if (t_cochon == null) { Debug.LogWarning("cochon.png pas trouvé"); return; }
        CreateCochonBloc("Cochon", t_cochon, 3, 500, 1);
        var t_cv = LoadSprite(_demoTex, "cochon_vert");
        if (t_cv != null) CreateCochonBloc("Cochon_Vert", t_cv, 4, 1000, 2);
        var t_cb = LoadSprite(_demoTex, "cochon_bleu");
        if (t_cb != null) CreateCochonBloc("Cochon_Bleu", t_cb, 6, 2000, 3);
    }

    private static void CreateCochonBloc(string name, Sprite sprite, int hp, int pts, int starVal)
    {
        var go = new GameObject(name, typeof(SpriteRenderer), typeof(BoxCollider2D), typeof(Rigidbody2D), typeof(AudioSource), typeof(Demolition_Block), typeof(Demolition_PigBehavior));
        var sr = go.GetComponent<SpriteRenderer>(); sr.sprite = sprite; sr.sortingOrder = 3;
        var blk = go.GetComponent<Demolition_Block>();
        blk.hp = hp; blk.points = pts; blk.materialType = Demolition_Block.MaterialType.Cochon; blk.spriteRenderer = sr;
        blk.isTarget = true; blk.starValue = starVal;
        PrefabUtility.SaveAsPrefabAsset(go, _demoPrefab + "/" + name + ".prefab");
        Object.DestroyImmediate(go);
    }

    private static void CreatePopupTextPrefab()
    {
        var go = new GameObject("PopupText", typeof(TextMeshPro), typeof(Demolition_PopupText));
        var tmp = go.GetComponent<TextMeshPro>();
        tmp.fontSize = 4f;
        tmp.color = Color.yellow;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontStyle = FontStyles.Bold;
        tmp.text = "+50";
        PrefabUtility.SaveAsPrefabAsset(go, _demoPrefab + "/PopupText.prefab");
        Object.DestroyImmediate(go);
    }
}