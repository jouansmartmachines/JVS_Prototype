using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine.UI;
using TMPro;
using Demolition;
using Object = UnityEngine.Object;

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
    private enum ProjectTab { Demolition, Dame, Sparks }
    private ProjectTab _activeTab = ProjectTab.Demolition;
    private Vector2 _scrollPos;

    // ── Colors / styling ──────────────────────────────────────────
    private static readonly Color ColorTabActive = new Color(0.3f, 0.5f, 0.9f);
    private static readonly Color ColorTabInactive = new Color(0.25f, 0.25f, 0.25f);
    private static readonly Color ColorHeader = new Color(0.2f, 0.25f, 0.3f);
    private static readonly Color ColorDivider = new Color(0.15f, 0.15f, 0.15f);

    // ── Setup step definitions ────────────────────────────────────
    private class SetupStep
    {
        public string label;
        public string scenePath;
        public Func<bool> isDone;   // scoped to target scene only
        public Action action;       // opens + saves target scene
        public bool isLast;
    }

    private Dictionary<ProjectTab, List<SetupStep>> _steps = new Dictionary<ProjectTab, List<SetupStep>>();

    // ── Init ──────────────────────────────────────────────────────
    private void OnEnable()
    {
        InitializeSteps();
    }

    // ════════════════════════════════════════════════════════════════════
    //  SAFE SCENE CHECK — OUVRE LA SCENE, VERIFIE, REFERME
    //  Évite les faux positifs quand l'utilisateur est sur une autre scène
    // ════════════════════════════════════════════════════════════════════

    private static bool SceneCheck(string scenePath, Predicate<Scene> checker)
    {
        if (!File.Exists(scenePath)) return false;

        var scene = EditorSceneManager.GetSceneByPath(scenePath);
        bool opened = false;
        if (!scene.IsValid() || !scene.isLoaded)
        {
            scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);
            opened = true;
        }

        bool result = scene.IsValid() && checker(scene);

        if (opened && scene.IsValid())
            EditorSceneManager.CloseScene(scene, true);

        return result;
    }

    private static bool HasObjInScene(Scene scene, string objName)
    {
        foreach (var root in scene.GetRootGameObjects())
        {
            if (FindInChildren(root.transform, objName) != null) return true;
        }
        return false;
    }

    private static Transform FindInChildren(Transform parent, string name)
    {
        if (parent.name == name) return parent;
        for (int i = 0; i < parent.childCount; i++)
        {
            var found = FindInChildren(parent.GetChild(i), name);
            if (found != null) return found;
        }
        return null;
    }

    private static bool HasComponentInScene<T>(Scene scene) where T : Component
    {
        foreach (var root in scene.GetRootGameObjects())
        {
            if (root.GetComponentInChildren<T>(true) != null) return true;
        }
        return false;
    }

    // ── Demolition checks (tout fait) ──────────────────────────────
    private static bool DemolitionGameSceneReady() => true;
    private static bool DemolitionAccueilReady() => true;
    private static bool DemolitionMenuReady() => true;
    private static bool DemolitionScoreReady() => true;

    // ── Dame checks (tout fait) ────────────────────────────────────
    private static bool DameGameSceneReady() => true;
    private static bool DameAccueilReady() => true;
    private static bool DameMenuReady() => true;
    private static bool DameScoreReady() => true;

    // ── Sparks checks ──────────────────────────────────────────────

    private static bool SparksAccueilReady()
    {
        return SceneCheck("Assets/Projects/Sparks/Scenes/Accueil_Sparks.unity", scene =>
            HasObjInScene(scene, "Background") && HasComponentInScene<Canvas>(scene));
    }

    private static bool SparksMenuReady()
    {
        return SceneCheck("Assets/Projects/Sparks/Scenes/Menu_Sparks.unity", scene =>
            HasObjInScene(scene, "Background") && HasComponentInScene<Canvas>(scene));
    }

    private static bool SparksScoreReady()
    {
        return SceneCheck("Assets/Projects/Sparks/Scenes/Score_Sparks.unity", scene =>
            HasObjInScene(scene, "Background") && HasComponentInScene<Canvas>(scene));
    }

    // ── Init steps ────────────────────────────────────────────────
    private void InitializeSteps()
    {
        _steps[ProjectTab.Demolition] = new List<SetupStep>
        {
            new SetupStep
            {
                label = "1. GameScene — Background, Sol, Canvas UI",
                scenePath = "Assets/Projects/Demolition/Demolition_Scenes/GameScene_Demolition.unity",
                isDone = DemolitionGameSceneReady,
                action = Demolition_SetupGameScene,
            },
            new SetupStep
            {
                label = "2. Accueil — Background",
                scenePath = "Assets/Projects/Demolition/Demolition_Scenes/Accueil_Demolition.unity",
                isDone = DemolitionAccueilReady,
                action = Demolition_SetupAccueil,
            },
            new SetupStep
            {
                label = "3. Menu — Background + UI options",
                scenePath = "Assets/Projects/Demolition/Demolition_Scenes/Menu_Demolition.unity",
                isDone = DemolitionMenuReady,
                action = Demolition_SetupMenu,
            },
            new SetupStep
            {
                label = "4. Score — Background",
                scenePath = "Assets/Projects/Demolition/Demolition_Scenes/Score_Demolition.unity",
                isDone = DemolitionScoreReady,
                action = Demolition_SetupScore,
                isLast = true,
            },
            new SetupStep
            {
                label = "★ TOUT CONFIGURER — Assets + 4 scènes",
                scenePath = null,
                isDone = () => DemolitionGameSceneReady() && DemolitionAccueilReady() && DemolitionMenuReady() && DemolitionScoreReady(),
                action = Demolition_ConfigTout,
            },
        };

        _steps[ProjectTab.Dame] = new List<SetupStep>
        {
            new SetupStep
            {
                label = "1. GameScene — Sprites + Sons",
                scenePath = "Assets/Projects/Dame/Scenes/GameScene_Dame.unity",
                isDone = DameGameSceneReady,
                action = Dame_SetupGameScene,
            },
            new SetupStep
            {
                label = "2. Accueil — Background",
                scenePath = "Assets/Projects/Dame/Scenes/Accueil_Dame.unity",
                isDone = DameAccueilReady,
                action = Dame_SetupAccueil,
            },
            new SetupStep
            {
                label = "3. Menu — Background + UI options",
                scenePath = "Assets/Projects/Dame/Scenes/Menu_Dame.unity",
                isDone = DameMenuReady,
                action = Dame_SetupMenu,
            },
            new SetupStep
            {
                label = "4. Score — Background + Fontes",
                scenePath = "Assets/Projects/Dame/Scenes/Score_Dame.unity",
                isDone = DameScoreReady,
                action = Dame_SetupScore,
                isLast = true,
            },
            new SetupStep
            {
                label = "★ TOUT CONFIGURER — Assets + 4 scènes",
                scenePath = null,
                isDone = () => DameGameSceneReady() && DameAccueilReady() && DameMenuReady() && DameScoreReady(),
                action = Dame_ConfigTout,
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
            var icon = tab == ProjectTab.Demolition ? "💥" : tab == ProjectTab.Dame ? "👑" : "✨";

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

        for (int i = 0; i < steps.Count; i++)
        {
            var step = steps[i];
            if (step.isDone())
            {
                doneCount++;
                continue;
            }

            DrawStepButton(step);
        }

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

        EditorGUILayout.Space(8);
        float progress = steps.Count > 0 ? (float)doneCount / steps.Count : 0;
        var progressRect = EditorGUILayout.BeginVertical();
        EditorGUI.ProgressBar(progressRect, progress, $"{doneCount}/{steps.Count} étapes faites");
        GUILayout.Space(20);
        EditorGUILayout.EndVertical();
    }

    private void DrawStepButton(SetupStep step)
    {
        bool sceneOpen = false;
        if (!string.IsNullOrEmpty(step.scenePath))
        {
            var scene = EditorSceneManager.GetSceneByPath(step.scenePath);
            sceneOpen = scene.IsValid() && scene.isLoaded;
        }

        bool inPlayMode = EditorApplication.isPlaying;
        bool disabled = sceneOpen || inPlayMode;

        if (disabled) GUI.enabled = false;

        string lockHint = "";
        if (inPlayMode) lockHint = "Arrêtez le mode Play d'abord";
        else if (sceneOpen) lockHint = "Fermez la scène avant de cliquer";

        string btnLabel = step.label;
        if (!string.IsNullOrEmpty(lockHint))
            btnLabel = $"🔒  {step.label}  —  {lockHint}";

        if (disabled)
            GUI.backgroundColor = new Color(0.8f, 0.3f, 0.1f);
        else
            GUI.backgroundColor = new Color(0.25f, 0.45f, 0.7f);

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        if (GUILayout.Button($"  {btnLabel}", GUILayout.Height(40)))
        {
            if (!disabled)
            {
                step.action();
                Repaint();
                EditorApplication.delayCall += Repaint;
            }
        }
        EditorGUILayout.EndVertical();

        GUI.backgroundColor = Color.white;
        GUI.enabled = true;
        EditorGUILayout.Space(3);
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
    //  ACTIONS VIDES — tout est déjà configuré
    // ════════════════════════════════════════════════════════════════

    private static void Demolition_SetupGameScene() { }
    private static void Demolition_SetupAccueil() { }
    private static void Demolition_SetupMenu() { }
    private static void Demolition_SetupScore() { }
    private static void Demolition_ConfigTout() { }
    private static void Dame_SetupGameScene() { }
    private static void Dame_SetupAccueil() { }
    private static void Dame_SetupMenu() { }
    private static void Dame_SetupScore() { }
    private static void Dame_ConfigTout() { }

    private static void Sparks_SetupGameScene() { }
    private static void Sparks_SetupAccueil() { }
    private static void Sparks_SetupMenu() { }
    private static void Sparks_SetupScore() { }
    private static void Sparks_ConfigTout() { }

    // ════════════════════════════════════════════════════════════════
    //  SHARED HELPERS — utilitaires réutilisables
    // ════════════════════════════════════════════════════════════════

    private static string _demoBase = "Assets/Projects/Demolition";
    private static string _demoPrefab => _demoBase + "/Resources/Prefabs";
    private static string _demoTex => _demoBase + "/Resources/Textures";
    private static string _demoSound => _demoBase + "/Resources/Sounds";

    private static string _dameBase = "Assets/Projects/Dame";
    private static string _dameSprite => _dameBase + "/Sprites";
    private static string _dameSound => _dameBase + "/Sons";
    private static string _dameFont => _dameBase + "/Font";

    private static string _sparksBase = "Assets/Projects/Sparks";
    private static string _sparksPrefab => _sparksBase + "/Resources/Prefabs";
    private static string _sparksTex => _sparksBase + "/Resources/Textures";
    private static string _sparksSound => _sparksBase + "/Resources/Sounds";

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
                    Object.DestroyImmediate(go);
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
                else Object.DestroyImmediate(child.gameObject);
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

    private static Sprite LoadSprite(string basePath, string name)
    {
        AssetDatabase.Refresh();
        return AssetDatabase.LoadAssetAtPath<Sprite>(basePath + "/" + name + ".png");
    }

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