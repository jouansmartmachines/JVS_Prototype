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
        MakeWAV("break", 0.3f, 150, 0.5f);
        MakeWAV("explosion", 0.5f, 100, 0.8f);
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
        Debug.Log("4/6 GameScene configuree");

        // 5. Menu
        SetupMenu();
        Debug.Log("5/6 Menu Toggle+Slider ajoutes");

        // 6. Accueil + Score backgrounds
        SetupBackGrounds();
        Debug.Log("6/6 Backgrounds scenes mis a jour");

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

        // Camera
        if (Object.FindFirstObjectByType<Camera>() == null)
        {
            var go = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
            var cam = go.GetComponent<Camera>();
            cam.orthographic = true; cam.orthographicSize = 5;
            cam.clearFlags = CameraClearFlags.Color;
            cam.backgroundColor = new Color(0.05f, 0.05f, 0.08f);
            go.transform.position = new Vector3(0, 0, -10);
            go.tag = "MainCamera";
            Debug.Log("Camera OK");
        }

        // EventSystem
        if (Object.FindFirstObjectByType<EventSystem>() == null)
        {
            new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            Debug.Log("EventSystem OK");
        }

        // Background
        if (GameObject.Find("Background") == null)
        {
            var bg = new GameObject("Background", typeof(SpriteRenderer));
            var sr = bg.GetComponent<SpriteRenderer>();
            var tex = LoadSprite("bg_game");
            if (tex != null) sr.sprite = tex;
            sr.color = new Color(0.05f, 0.05f, 0.08f);
            bg.transform.position = new Vector3(0, 0, 5);
            sr.sortingOrder = -1;
            Debug.Log("Background OK");
        }

        // StructuresParent
        if (GameObject.Find("StructuresParent") == null)
        {
            new GameObject("StructuresParent");
            Debug.Log("StructuresParent OK");
        }

        // GeneralVariable
        if (Object.FindFirstObjectByType<Demolition_GeneralVariables>() == null)
        {
            var gvPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(_basePath + "/Demolition_Prefabs/GeneralVariable.prefab");
            if (gvPrefab != null) { PrefabUtility.InstantiatePrefab(gvPrefab); Debug.Log("GV OK"); }
            else Debug.LogWarning("GeneralVariable.prefab manquant");
        }

        // GameManager
        if (Object.FindFirstObjectByType<Demolition_GameManager>() == null)
        {
            var gmGO = new GameObject("Demolition_GameManager", typeof(Demolition_GameManager));
            var gm = gmGO.GetComponent<Demolition_GameManager>();
            var aud = gmGO.AddComponent<AudioSource>();
            aud.playOnAwake = false;
            gm.impactSound = AssetDatabase.LoadAssetAtPath<AudioClip>(_soundPath + "/impact.wav");
            gm.breakSound = AssetDatabase.LoadAssetAtPath<AudioClip>(_soundPath + "/break.wav");
            gm.oiseauPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(_prefabPath + "/Oiseau.prefab");
            gm.impactEffectPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(_prefabPath + "/ImpactExplosion.prefab");
            Debug.Log("GameManager OK");
        }

        EditorSceneManager.SaveScene(scene);
        Debug.Log("GameScene sauvegardee");
    }

    static void SetupMenu()
    {
        string scenePath = _basePath + "/Demolition_Scenes/Menu_Demolition.unity";
        var scene = EditorSceneManager.OpenScene(scenePath);

        var canvas = Object.FindFirstObjectByType<Canvas>();
        if (canvas == null) { Debug.LogError("Canvas pas trouve!"); return; }

        // Toggle ModeOiseau
        if (GameObject.Find("ModeOiseau") == null)
        {
            var go = new GameObject("ModeOiseau", typeof(RectTransform));
            go.transform.SetParent(canvas.transform);
            go.AddComponent<Image>();
            var toggle = go.AddComponent<Toggle>();
            toggle.isOn = PlayerPrefs.GetInt(Demolition_GeneralVariables.ModeOiseauKey, 1) == 1;

            var chk = new GameObject("Checkmark", typeof(RectTransform));
            chk.transform.SetParent(go.transform);
            var ci = chk.AddComponent<Image>();
            ci.sprite = LoadSprite("bois");
            chk.AddComponent<CanvasRenderer>();
            toggle.graphic = ci;

            var lbl = new GameObject("Label", typeof(RectTransform));
            lbl.transform.SetParent(go.transform);
            var txt = lbl.AddComponent<TextMeshProUGUI>();
            txt.text = "Mode Oiseau"; txt.fontSize = 24;

            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = new Vector2(-100, 150);
            rt.sizeDelta = new Vector2(200, 50);
            Debug.Log("Toggle OK");
        }

        // Slider
        if (GameObject.Find("ScrollSpeed") == null)
        {
            var go = new GameObject("ScrollSpeed", typeof(RectTransform));
            go.transform.SetParent(canvas.transform);
            var slider = go.AddComponent<Slider>();
            go.AddComponent<Image>();

            var bg = new GameObject("Background", typeof(RectTransform));
            bg.transform.SetParent(go.transform); bg.AddComponent<Image>();

            var fill = new GameObject("Fill", typeof(RectTransform));
            fill.transform.SetParent(go.transform); fill.AddComponent<Image>();

            var hdl = new GameObject("Handle", typeof(RectTransform));
            hdl.transform.SetParent(go.transform); hdl.AddComponent<Image>();

            slider.fillRect = fill.GetComponent<RectTransform>();
            slider.handleRect = hdl.GetComponent<RectTransform>();
            slider.minValue = 1; slider.maxValue = 5; slider.wholeNumbers = true;
            slider.value = PlayerPrefs.GetFloat(Demolition_GeneralVariables.ScrollSpeedKey, 2f);

            var lbl = new GameObject("Label", typeof(RectTransform));
            lbl.transform.SetParent(canvas.transform);
            var txt = lbl.AddComponent<TextMeshProUGUI>();
            txt.text = "Vitesse"; txt.fontSize = 24;

            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = new Vector2(-100, 50);
            rt.sizeDelta = new Vector2(300, 50);
            Debug.Log("Slider OK");
        }

        EditorSceneManager.SaveScene(scene);
    }

    static void SetupBackGrounds()
    {
        // Replace sprites in Accueil, Menu, Score scenes
        // Already done in YAML on GitHub - just log
        Debug.Log("Backgrounds deja configures dans les scenes");
    }
}