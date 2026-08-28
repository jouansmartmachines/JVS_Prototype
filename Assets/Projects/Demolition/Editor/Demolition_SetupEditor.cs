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
    static string _basePath, _prefabPath, _texPath, _soundPath;

    [MenuItem("Tools/Demolition - Tout configurer")]
    static void ConfigurerTout()
    {
        _basePath = "Assets/Projects/Demolition";
        string resPath = _basePath + "/Resources";
        _prefabPath = resPath + "/Prefabs";
        _texPath = resPath + "/Textures";
        _soundPath = resPath + "/Sounds";

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
        MakePNG("fissure1", 16, 16, new Color(0.3f, 0.3f, 0.3f));
        MakePNG("fissure2", 16, 16, new Color(0.2f, 0.2f, 0.2f));
        Debug.Log("1/6 Textures creees");

        // 2. Sons
        MakeWAV("impact", 0.15f, 200, 0.3f);
        MakeWAV("destruction", 0.3f, 150, 0.5f);
        MakeWAV("gameover", 0.5f, 100, 0.8f);
        Debug.Log("2/6 Sons crees");

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

        if (t_bois == null) { Debug.LogError("Textures non trouvees"); return; }

        CreateBloc("Bloc_Bois", t_bois, Demolition_Block.MaterialType.Bois, 2, 50, t_f1, t_f2);
        CreateBloc("Bloc_Verre", t_verre, Demolition_Block.MaterialType.Verre, 1, 100, t_f1, t_f2);
        CreateBloc("Bloc_Pierre", t_pierre, Demolition_Block.MaterialType.Pierre, 4, 20, t_f1, t_f2);
        CreateDebris("Debris_Bois", t_db);
        CreateDebris("Debris_Verre", t_dv);
        CreateDebris("Debris_Pierre", t_dp);
        CreateOiseau(t_oiseau, t_impact);
        Debug.Log("3/6 Prefabs gameplay crees");

        // 4. GameScene
        SetupGameScene();
        Debug.Log("4/6 GameScene + Canvas U configuree");

        // 5. Menu Toggle (ModeOiseau) + Slider (ScrollSpeed) - style SpotTheDif
        SetupMenu();
        Debug.Log("5/6 Menu Toggle+Slider ajoutes");

        AssetDatabase.Refresh();
        Debug.Log("=== FINI: Demolition completement configure ===");
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
        var sr = go.GetComponent<SpriteRenderer>(); sr.sprite = sprite; sr.sortingOrder = 2;
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
        imp.GetComponent<SpriteRenderer>().sortingOrder = 4;
        PrefabUtility.SaveAsPrefabAsset(imp, _prefabPath + "/ImpactExplosion.prefab");
        Object.DestroyImmediate(imp);

        var go = new GameObject("Oiseau", typeof(SpriteRenderer), typeof(Demolition_Projectile));
        go.GetComponent<SpriteRenderer>().sprite = oiSprite;
        go.GetComponent<SpriteRenderer>().sortingOrder = 3;
        var p = go.GetComponent<Demolition_Projectile>();
        p.oiseauDos = oiSprite;
        p.spriteRenderer = go.GetComponent<SpriteRenderer>();
        p.vitesseDepart = 5; p.acceleration = 2;
        p.scaleMin = 0.1f; p.scaleMax = 1;
        p.forceExplosion = 500; p.radiusExplosion = 2;
        p.explosionPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(_prefabPath + "/ImpactExplosion.prefab");
        PrefabUtility.SaveAsPrefabAsset(go, _prefabPath + "/Oiseau.prefab");
        Object.DestroyImmediate(go);
    }

    static void SetupGameScene()
    {
        string scenePath = _basePath + "/Demolition_Scenes/GameScene_Demolition.unity";
        var scene = EditorSceneManager.OpenScene(scenePath);

        if (Object.FindFirstObjectByType<Camera>() == null)
        {
            var go = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
            var cam = go.GetComponent<Camera>();
            cam.orthographic = true; cam.orthographicSize = 5;
            cam.clearFlags = CameraClearFlags.Color;
            cam.backgroundColor = new Color(0.05f, 0.05f, 0.08f);
            go.transform.position = new Vector3(0, 0, -10);
            go.tag = "MainCamera";
        }

        if (Object.FindFirstObjectByType<Light>() == null)
        {
            var lightGO = new GameObject("Directional Light", typeof(Light));
            var light = lightGO.GetComponent<Light>();
            light.type = LightType.Directional;
            light.color = new Color(1, 0.95686275f, 0.8392157f);
            light.intensity = 1;
            lightGO.transform.position = new Vector3(0, 3, 0);
            lightGO.transform.rotation = Quaternion.Euler(50, -30, 0);
        }

        if (Object.FindFirstObjectByType<EventSystem>() == null)
            new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));

        if (GameObject.Find("Background") == null)
        {
            var bg = new GameObject("Background", typeof(SpriteRenderer));
            var sr = bg.GetComponent<SpriteRenderer>();
            var tex = LoadSprite("bg_game");
            if (tex != null) sr.sprite = tex;
            sr.color = new Color(0.05f, 0.05f, 0.08f);
            bg.transform.position = new Vector3(0, 0, 5);
            sr.sortingOrder = -1;
        }

        if (GameObject.Find("StructuresParent") == null)
            new GameObject("StructuresParent");

        if (Object.FindFirstObjectByType<Demolition_GeneralVariables>() == null)
        {
            var gvPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(_basePath + "/Demolition_Prefabs/GeneralVariable.prefab");
            if (gvPrefab != null) { PrefabUtility.InstantiatePrefab(gvPrefab); }
            else Debug.LogWarning("GeneralVariable.prefab manquant");
        }

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

            // Tableaux (structures de blocs)
            var t1 = Resources.Load<GameObject>("Prefabs/Tableau_1");
            var t2 = Resources.Load<GameObject>("Prefabs/Tableau_2");
            var t3 = Resources.Load<GameObject>("Prefabs/Tableau_3");
            if (t1 != null && t2 != null && t3 != null)
                gm.tableauPrefabs = new GameObject[] { t1, t2, t3 };
            else
                Debug.LogWarning("Tableau prefabs non trouves dans Resources/Prefabs/");
        }

        // Canvas avec Score + Timer pour le gameplay
        SetupGameSceneCanvas();

        EditorSceneManager.SaveScene(scene);
    }

    static void SetupGameSceneCanvas()
    {
        var canvas = Object.FindFirstObjectByType<Canvas>();
        if (canvas != null)
        {
            // Canvas deja present, verifier les textes
            if (GameObject.Find("ScoreText") != null && GameObject.Find("TimerText") != null)
                return;
        }

        // Creer le Canvas
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

        // Score en haut a gauche
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
            Debug.Log("ScoreText cree dans GameScene");
        }

        // Timer en haut a droite
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
            Debug.Log("TimerText cree dans GameScene");
        }
    }

    static void SetupMenu()
    {
        string scenePath = _basePath + "/Demolition_Scenes/Menu_Demolition.unity";
        var scene = EditorSceneManager.OpenScene(scenePath);

        var canvas = Object.FindFirstObjectByType<Canvas>();
        if (canvas == null) { Debug.LogError("Canvas pas trouve!"); return; }

        // === Toggle ModeOiseau (style SpotTheDif Instructions) ===
        if (GameObject.Find("ModeOiseau") == null)
        {
            // Container (like Instructions)
            var container = new GameObject("ModeOiseau", typeof(RectTransform));
            container.transform.SetParent(canvas.transform);
            var crt = container.GetComponent<RectTransform>();
            crt.anchorMin = Vector2.one * 0.5f;
            crt.anchorMax = Vector2.one * 0.5f;
            crt.anchoredPosition = new Vector2(386, -153);
            crt.sizeDelta = new Vector2(1221, 150);
            crt.localScale = Vector3.one * 0.5f;

            // Label "Mode Oiseau"
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

            // Toggle
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

            // Checkmark
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

            Debug.Log("Toggle ModeOiseau cree avec positions SpotTheDif");
        }

        // === Slider ScrollSpeed (style SpotTheDif ScenesNumber) ===
        if (GameObject.Find("ScrollSpeed") == null)
        {
            // Container (like ScenesNumber)
            var container = new GameObject("ScrollSpeed", typeof(RectTransform));
            container.transform.SetParent(canvas.transform);
            var crt = container.GetComponent<RectTransform>();
            crt.anchorMin = Vector2.one * 0.5f;
            crt.anchorMax = Vector2.one * 0.5f;
            crt.anchoredPosition = new Vector2(-691, -73);
            crt.sizeDelta = new Vector2(500, 400);
            crt.localScale = Vector3.one * 0.8f;

            // Label "Vitesse"
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

            // Slider
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

            // Fill
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

            // Handle
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

            // Background of slider
            var bg = new GameObject("Background", typeof(RectTransform));
            bg.transform.SetParent(sliderGO.transform);
            var brt = bg.GetComponent<RectTransform>();
            brt.anchorMin = new Vector2(0, 0.25f);
            brt.anchorMax = new Vector2(1, 0.75f);
            brt.sizeDelta = Vector2.zero;
            bg.AddComponent<Image>();
            bg.GetComponent<Image>().color = new Color(0.5f, 0.5f, 0.5f);

            Debug.Log("Slider ScrollSpeed cree avec positions SpotTheDif");
        }

        EditorSceneManager.SaveScene(scene);
    }
}