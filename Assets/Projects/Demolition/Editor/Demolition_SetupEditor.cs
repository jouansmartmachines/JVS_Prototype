using UnityEngine;
using UnityEditor;
using System.IO;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using Demolition;

public class Demolition_SetupEditor : EditorWindow
{
    private static string _basePath = "Assets/Projects/Demolition";
    private static string _prefabPath = "Assets/Projects/Demolition/Resources/Prefabs";
    private static string _texPath = "Assets/Projects/Demolition/Resources/Textures";
    private static string _soundPath = "Assets/Projects/Demolition/Resources/Sounds";

    [MenuItem("Tools/Demolition - Panneau Configuration Editeur")]
    public static void ShowWindow()
    {
        GetWindow<Demolition_SetupEditor>("Demolition Editor Tool");
    }

    void OnGUI()
    {
        GUILayout.Space(10);
        GUILayout.Label("Outils de Configuration Hors Play - Demolition", EditorStyles.boldLabel);
        GUILayout.Space(5);

        EditorGUILayout.HelpBox("Cliquez sur les boutons ci-dessous pour configurer proprement vos scènes HORS Play.\nL'outil nettoie tous les doublons et place le Background en plein écran dans le Canvas.", MessageType.Info);
        GUILayout.Space(10);

        if (GUILayout.Button("1. Configurer Background & Sol dans GameScene (Hors Play)", GUILayout.Height(35)))
        {
            PlacerBackgroundEtSolGameScene();
        }

        GUILayout.Space(5);
        if (GUILayout.Button("2. Configurer Background dans Scene Accueil (Hors Play)", GUILayout.Height(30)))
        {
            PlacerBackgroundSceneAccueil();
        }

        GUILayout.Space(5);
        if (GUILayout.Button("3. Configurer Background dans Scene Menu (Hors Play)", GUILayout.Height(30)))
        {
            PlacerBackgroundSceneMenu();
        }

        GUILayout.Space(5);
        if (GUILayout.Button("4. Configurer Background dans Scene Score (Hors Play)", GUILayout.Height(30)))
        {
            PlacerBackgroundSceneScore();
        }

        GUILayout.Space(15);
        if (GUILayout.Button("★ TOUT CONFIGURER (Prefabs, Sons, Scènes, UI)", GUILayout.Height(40)))
        {
            ConfigurerTout();
        }
    }

    [MenuItem("Tools/Demolition/1. Configurer Background et Sol dans GameScene (Hors Play)")]
    public static void PlacerBackgroundEtSolGameScene()
    {
        if (EditorApplication.isPlaying)
        {
            Debug.LogError("Arrêtez le mode Play avant d'utiliser cet outil !");
            return;
        }

        InitPaths();
        string scenePath = _basePath + "/Demolition_Scenes/GameScene_Demolition.unity";
        var scene = EditorSceneManager.OpenScene(scenePath);

        // 1. Camera
        EnsureCamera();

        // 2. Nettoyage des orphelins hors Canvas
        CleanAllOrphanBackgrounds();

        // 3. UI Canvas & Background plein écran
        SetupGameSceneCanvas();
        SetupCanvasBackground("bg_game", false); // raycastTarget = false pour laisser passer les tirs

        // 4. Sol (Ground) avec sprite sol.png, BoxCollider2D et GroundScroll
        SetupGameSceneGround();

        // 5. StructuresParent
        if (GameObject.Find("StructuresParent") == null)
        {
            new GameObject("StructuresParent");
        }

        // 6. Demolition_GameManager
        if (Object.FindFirstObjectByType<Demolition_GameManager>() == null)
        {
            var gmGO = new GameObject("Demolition_GameManager", typeof(Demolition_GameManager));
            var gm = gmGO.GetComponent<Demolition_GameManager>();
            var aud = gmGO.AddComponent<AudioSource>();
            aud.playOnAwake = false;
            gm.impactSound = AssetDatabase.LoadAssetAtPath<AudioClip>(_soundPath + "/impact.wav");
            gm.destructionSound = AssetDatabase.LoadAssetAtPath<AudioClip>(_soundPath + "/destruction.wav");
            gm.oiseauPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(_prefabPath + "/Oiseau.prefab");
            gm.impactEffectPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(_prefabPath + "/ImpactExplosion.prefab");
        }

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("[Demolition] GameScene_Demolition configurée avec succès (Background Canvas plein écran + Sol avec sprite sol.png) !");
    }

    [MenuItem("Tools/Demolition/2. Configurer Background dans Scene Accueil (Hors Play)")]
    public static void PlacerBackgroundSceneAccueil()
    {
        if (EditorApplication.isPlaying)
        {
            Debug.LogError("Arrêtez le mode Play avant d'utiliser cet outil !");
            return;
        }

        InitPaths();
        string scenePath = _basePath + "/Demolition_Scenes/Accueil_Demolition.unity";
        var scene = EditorSceneManager.OpenScene(scenePath);

        EnsureCamera();
        CleanAllOrphanBackgrounds();
        SetupCanvasBackground("bg_accueil", true);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("[Demolition] Accueil_Demolition configurée avec succès avec son Background dans le Canvas !");
    }

    [MenuItem("Tools/Demolition/3. Configurer Background dans Scene Menu (Hors Play)")]
    public static void PlacerBackgroundSceneMenu()
    {
        if (EditorApplication.isPlaying)
        {
            Debug.LogError("Arrêtez le mode Play avant d'utiliser cet outil !");
            return;
        }

        InitPaths();
        string scenePath = _basePath + "/Demolition_Scenes/Menu_Demolition.unity";
        var scene = EditorSceneManager.OpenScene(scenePath);

        EnsureCamera();
        CleanAllOrphanBackgrounds();
        SetupCanvasBackground("bg_menu", true);
        SetupMenu();

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("[Demolition] Menu_Demolition configurée avec succès avec son Background dans le Canvas !");
    }

    [MenuItem("Tools/Demolition/4. Configurer Background dans Scene Score (Hors Play)")]
    public static void PlacerBackgroundSceneScore()
    {
        if (EditorApplication.isPlaying)
        {
            Debug.LogError("Arrêtez le mode Play avant d'utiliser cet outil !");
            return;
        }

        InitPaths();
        string scenePath = _basePath + "/Demolition_Scenes/Score_Demolition.unity";
        var scene = EditorSceneManager.OpenScene(scenePath);

        EnsureCamera();
        CleanAllOrphanBackgrounds();
        SetupCanvasBackground("bg_score", true);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("[Demolition] Score_Demolition configurée avec succès avec son Background au bon endroit dans le Canvas !");
    }

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

    /// <summary>
    /// Supprime tous les GameObjects Background orphelins à la racine de la scène.
    /// </summary>
    private static void CleanAllOrphanBackgrounds()
    {
        var rootGOs = SceneManager.GetActiveScene().GetRootGameObjects();
        foreach (var go in rootGOs)
        {
            if (go != null && (go.name == "Background" || go.name == "bg_game" || go.name == "bg_accueil" || go.name == "bg_menu" || go.name == "bg_score"))
            {
                if (go.GetComponent<Canvas>() == null)
                {
                    Undo.DestroyObjectImmediate(go);
                }
            }
        }
    }

    /// <summary>
    /// Place et configure un Background plein écran comme premier enfant du Canvas UI.
    /// </summary>
    private static void SetupCanvasBackground(string texName, bool raycastTarget)
    {
        var sprite = LoadSprite(texName);
        if (sprite == null) sprite = LoadSprite("bg_accueil");

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

        // Trouver ou créer le Background enfant du Canvas
        Transform bgTransform = null;
        for (int i = 0; i < canvas.transform.childCount; i++)
        {
            var child = canvas.transform.GetChild(i);
            if (child.name == "Background")
            {
                if (bgTransform == null)
                {
                    bgTransform = child;
                }
                else
                {
                    // Doublon supplémentaire dans le Canvas -> détruire
                    Undo.DestroyObjectImmediate(child.gameObject);
                }
            }
        }

        if (bgTransform == null)
        {
            var bgGO = new GameObject("Background", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            bgGO.transform.SetParent(canvas.transform, false);
            bgTransform = bgGO.transform;
        }

        // Toujours en premier enfant (arrière-plan sous les boutons/textes)
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

    /// <summary>
    /// Configure le Sol physique dans GameScene avec le sprite sol.png.
    /// </summary>
    private static void SetupGameSceneGround()
    {
        var ground = GameObject.Find("Ground");
        if (ground == null)
        {
            ground = new GameObject("Ground", typeof(BoxCollider2D), typeof(SpriteRenderer), typeof(Demolition_GroundScroll));
        }

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

        var solSprite = LoadSprite("sol");
        if (solSprite != null)
        {
            groundSr.sprite = solSprite;
        }

        if (ground.GetComponent<Demolition_GroundScroll>() == null)
        {
            ground.AddComponent<Demolition_GroundScroll>();
        }
    }

    [MenuItem("Tools/Demolition/5. Tout configurer (Scenes + Prefabs + Assets)")]
    public static void ConfigurerTout()
    {
        if (EditorApplication.isPlaying)
        {
            Debug.LogError("Arrêtez le jeu avant de lancer l'outil !");
            return;
        }
        InitPaths();

        Directory.CreateDirectory(_prefabPath);
        Directory.CreateDirectory(_texPath);
        Directory.CreateDirectory(_soundPath);

        Debug.Log("=== DEBUT configuration Demolition ===");

        // 1. Textures
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

        // 2. Sons
        MakeWAV("impact", 0.15f, 200, 0.3f);
        MakeWAV("destruction", 0.3f, 150, 0.5f);
        MakeWAV("gameover", 0.5f, 100, 0.8f);
        MakeWAV("pig_hit", 0.2f, 300, 0.4f);

        AssetDatabase.Refresh();

        // 3. Prefabs gameplay
        var t_bois = LoadSprite("bois");
        var t_verre = LoadSprite("verre");
        var t_pierre = LoadSprite("pierre");
        var t_f1 = LoadSprite("fissure1");
        var t_f2 = LoadSprite("fissure2");
        var t_oiseau = LoadSprite("oiseau_dos");
        var t_impact = LoadSprite("impact");
        var t_db = LoadSprite("debris_bois");
        var t_dv = LoadSprite("debris_verre");
        var t_dp = LoadSprite("debris_pierre");
        var t_dc = LoadSprite("debris_cochon");

        if (t_bois != null)
        {
            CreateBloc("Bloc_Bois", t_bois, Demolition_Block.MaterialType.Bois, 4, 50, t_f1, t_f2);
            CreateBloc("Bloc_Verre", t_verre, Demolition_Block.MaterialType.Verre, 2, 80, t_f1, t_f2);
            CreateBloc("Bloc_Pierre", t_pierre, Demolition_Block.MaterialType.Pierre, 8, 40, t_f1, t_f2);
            CreateDebris("Debris_Bois", t_db);
            CreateDebris("Debris_Verre", t_dv);
            CreateDebris("Debris_Pierre", t_dp);
            CreateDebris("Debris_Cochon", t_dc);
            CreateOiseau(t_oiseau, t_impact);
            CreateCochonPrefabs();
            CreatePopupTextPrefab();
        }

        // 4. Configuration propre des Scènes (nettoyage + remplacement enfants Canvas)
        PlacerBackgroundEtSolGameScene();
        PlacerBackgroundSceneAccueil();
        PlacerBackgroundSceneMenu();
        PlacerBackgroundSceneScore();

        // 5. Star images
        CreateStarImages();

        AssetDatabase.Refresh();
        Debug.Log("=== FINI: Demolition complètement configuré sans doublons ! ===");
    }

    private static void InitPaths()
    {
        _basePath = "Assets/Projects/Demolition";
        string resPath = _basePath + "/Resources";
        _prefabPath = resPath + "/Prefabs";
        _texPath = resPath + "/Textures";
        _soundPath = resPath + "/Sounds";
    }

    static void MakePNG(string name, int w, int h, Color c)
    {
        string path = _texPath + "/" + name + ".png";
        if (File.Exists(path)) return;
        var tex = new Texture2D(w, h);
        for (int x = 0; x < w; x++) for (int y = 0; y < h; y++) tex.SetPixel(x, y, c);
        tex.Apply();
        File.WriteAllBytes(path, tex.EncodeToPNG());
        Object.DestroyImmediate(tex);
    }

    static Sprite LoadSprite(string name)
    {
        AssetDatabase.Refresh();
        return AssetDatabase.LoadAssetAtPath<Sprite>(_texPath + "/" + name + ".png");
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

    static void CreateBloc(string name, Sprite sprite, Demolition_Block.MaterialType mat, int hp, int pts, Sprite f1, Sprite f2)
    {
        var go = new GameObject(name, typeof(SpriteRenderer), typeof(BoxCollider2D), typeof(Rigidbody2D), typeof(AudioSource), typeof(Demolition_Block));
        var sr = go.GetComponent<SpriteRenderer>(); sr.sprite = sprite; sr.sortingOrder = 3;
        var blk = go.GetComponent<Demolition_Block>();
        blk.hp = hp; blk.points = pts; blk.materialType = mat; blk.spriteRenderer = sr;
        blk.damageSprites = new Sprite[] { f1, f2 };
        PrefabUtility.SaveAsPrefabAsset(go, _prefabPath + "/" + name + ".prefab");
        Object.DestroyImmediate(go);
    }

    static void CreateDebris(string name, Sprite sprite)
    {
        var go = new GameObject(name, typeof(SpriteRenderer));
        go.GetComponent<SpriteRenderer>().sprite = sprite;
        PrefabUtility.SaveAsPrefabAsset(go, _prefabPath + "/" + name + ".prefab");
        Object.DestroyImmediate(go);
    }

    static void CreateOiseau(Sprite oiSprite, Sprite imSprite)
    {
        var imp = new GameObject("ImpactExplosion", typeof(SpriteRenderer));
        imp.GetComponent<SpriteRenderer>().sprite = imSprite;
        imp.GetComponent<SpriteRenderer>().sortingOrder = 5;
        PrefabUtility.SaveAsPrefabAsset(imp, _prefabPath + "/ImpactExplosion.prefab");
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
        PrefabUtility.SaveAsPrefabAsset(go, _prefabPath + "/Oiseau.prefab");
        Object.DestroyImmediate(go);
    }

    static void CreateCochonPrefabs()
    {
        var t_cochon = LoadSprite("cochon");
        if (t_cochon == null) { Debug.LogWarning("cochon.png pas trouve"); return; }
        CreateCochonBloc("Cochon", t_cochon, 3, 500, 1);
        var t_cv = LoadSprite("cochon_vert");
        if (t_cv != null) CreateCochonBloc("Cochon_Vert", t_cv, 4, 1000, 2);
        var t_cb = LoadSprite("cochon_bleu");
        if (t_cb != null) CreateCochonBloc("Cochon_Bleu", t_cb, 6, 2000, 3);
    }

    static void CreateCochonBloc(string name, Sprite sprite, int hp, int pts, int starVal)
    {
        var go = new GameObject(name, typeof(SpriteRenderer), typeof(BoxCollider2D), typeof(Rigidbody2D), typeof(AudioSource), typeof(Demolition_Block), typeof(Demolition_PigBehavior));
        var sr = go.GetComponent<SpriteRenderer>(); sr.sprite = sprite; sr.sortingOrder = 3;
        var blk = go.GetComponent<Demolition_Block>();
        blk.hp = hp; blk.points = pts; blk.materialType = Demolition_Block.MaterialType.Cochon; blk.spriteRenderer = sr;
        blk.isTarget = true; blk.starValue = starVal;
        PrefabUtility.SaveAsPrefabAsset(go, _prefabPath + "/" + name + ".prefab");
        Object.DestroyImmediate(go);
    }

    static void CreatePopupTextPrefab()
    {
        var go = new GameObject("PopupText", typeof(TextMeshPro), typeof(Demolition_PopupText));
        var tmp = go.GetComponent<TextMeshPro>();
        tmp.fontSize = 4f;
        tmp.color = Color.yellow;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontStyle = FontStyles.Bold;
        tmp.text = "+50";
        PrefabUtility.SaveAsPrefabAsset(go, _prefabPath + "/PopupText.prefab");
        Object.DestroyImmediate(go);
    }

    static void CreateStarImages()
    {
        MakePNG("star_1", 32, 32, new Color(1, 0.8f, 0));
        MakePNG("star_2", 32, 32, new Color(1, 0.9f, 0.2f));
        MakePNG("star_3", 32, 32, new Color(1, 1, 0.4f));
    }

    static void SetupGameSceneCanvas()
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

    static void SetupMenu()
    {
        string scenePath = _basePath + "/Demolition_Scenes/Menu_Demolition.unity";
        var scene = EditorSceneManager.OpenScene(scenePath);

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
            checkImg.sprite = LoadSprite("bois");
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

        EditorSceneManager.SaveScene(scene);
    }
}
