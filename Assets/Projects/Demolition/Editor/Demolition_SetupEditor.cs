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
    [MenuItem("Tools/Demolition - Generer les prefabs")]
    static void GenerateAll()
    {
        string basePath = "Assets/Projects/Demolition";
        string resourcesPath = basePath + "/Resources";
        string prefabPath = resourcesPath + "/Prefabs";
        string texPath = resourcesPath + "/Textures";
        string soundPath = resourcesPath + "/Sounds";
        string scenePf = basePath + "/ScenePrefabs";
        string demPrefabs = basePath + "/Demolition_Prefabs";

        // Creer les dossiers avant tout
        Directory.CreateDirectory(prefabPath);
        Directory.CreateDirectory(texPath);
        Directory.CreateDirectory(soundPath);

        // 1. Textures gameplay
        MakePNG(texPath, "bois", 64, 32, new Color(0.545f, 0.353f, 0.169f));
        MakePNG(texPath, "verre", 64, 32, new Color(0.678f, 0.847f, 0.902f, 0.7f));
        MakePNG(texPath, "pierre", 64, 32, new Color(0.5f, 0.5f, 0.5f));
        MakePNG(texPath, "oiseau", 32, 32, new Color(0.863f, 0.196f, 0.196f));
        MakePNG(texPath, "impact", 64, 64, new Color(1f, 0.647f, 0f));
        MakePNG(texPath, "debris_bois", 16, 8, new Color(0.545f, 0.353f, 0.169f));
        MakePNG(texPath, "debris_verre", 8, 8, new Color(0.678f, 0.847f, 0.902f));
        MakePNG(texPath, "debris_pierre", 12, 12, new Color(0.5f, 0.5f, 0.5f));

        AssetDatabase.Refresh();
        foreach (var tex in new[] { "bois", "verre", "pierre", "oiseau", "impact", "debris_bois", "debris_verre", "debris_pierre" })
            SetSpriteMode(texPath + "/" + tex + ".png");
        AssetDatabase.Refresh();

        // 2. Sons
        MakeWAV(soundPath + "/impact.wav", 440, 0.15f, 0.8f);
        MakeWAV(soundPath + "/destruction.wav", 220, 0.3f, 0.7f);
        MakeWAV(soundPath + "/gameover.wav", 180, 0.5f, 0.6f);
        AssetDatabase.Refresh();

        // 3. Charger sprites gameplay
        Sprite sBois = LoadSprite(texPath + "/bois.png");
        Sprite sVerre = LoadSprite(texPath + "/verre.png");
        Sprite sPierre = LoadSprite(texPath + "/pierre.png");
        Sprite sOiseau = LoadSprite(texPath + "/oiseau.png");
        Sprite sImpact = LoadSprite(texPath + "/impact.png");
        Sprite sDBois = LoadSprite(texPath + "/debris_bois.png");
        Sprite sDVerre = LoadSprite(texPath + "/debris_verre.png");
        Sprite sDPierre = LoadSprite(texPath + "/debris_pierre.png");

        if (sBois == null) { Debug.LogError("ERREUR: sprites non charges"); return; }

        // 4. Blocs + debris + oiseau
        var bBois = CreateBloc(prefabPath, "Bloc_Bois", sBois, Demolition_Block.MaterialType.Bois, 2, 50);
        var bVerre = CreateBloc(prefabPath, "Bloc_Verre", sVerre, Demolition_Block.MaterialType.Verre, 1, 100);
        var bPierre = CreateBloc(prefabPath, "Bloc_Pierre", sPierre, Demolition_Block.MaterialType.Pierre, 4, 150);
        var dBois = CreateDebris(prefabPath, "Debris_Bois", sDBois);
        var dVerre = CreateDebris(prefabPath, "Debris_Verre", sDVerre);
        var dPierre = CreateDebris(prefabPath, "Debris_Pierre", sDPierre);
        LinkDebris(bBois, dBois); LinkDebris(bVerre, dVerre); LinkDebris(bPierre, dPierre);
        CreateOiseau(prefabPath, sOiseau, sImpact);
        MakeStruct(prefabPath, "Structure_Exemple", new[] { "Bloc_Bois", "Bloc_Bois", "Bloc_Bois" }, new Vector3[] { new(0,0,0), new(0.7f,0.35f,0), new(1.4f,0.7f,0) });
        MakeStruct(prefabPath, "Tableau_1", new[] { "Bloc_Bois", "Bloc_Verre", "Bloc_Bois", "Bloc_Pierre" }, new Vector3[] { new(0,0,0), new(0.7f,0.35f,0), new(1.4f,0,0), new(2.1f,0.35f,0) });
        MakeStruct(prefabPath, "Tableau_2", new[] { "Bloc_Pierre", "Bloc_Bois", "Bloc_Verre", "Bloc_Verre" }, new Vector3[] { new(0,0,0), new(0.7f,0.7f,0), new(1.4f,0,0), new(2.1f,0,0) });
        MakeStruct(prefabPath, "Tableau_3", new[] { "Bloc_Verre", "Bloc_Bois", "Bloc_Pierre", "Bloc_Bois" }, new Vector3[] { new(0,0,0), new(0.7f,0,0), new(1.4f,0.7f,0), new(0.7f,0.7f,0) });

        // 5. Setup GameScene (doit etre fait AVANT de charger les prefabs scenes pour les refs)
        SetupGameScene(prefabPath, soundPath, demPrefabs);

        // 6. Changer les backgrounds dans les ScenePrefabs (apres la scene pour avoir les textures)
        ReplaceAllBackgrounds(scenePf, texPath);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("OK - Tout genere ! Prefabs gameplay, GameScene, backgrounds mis a jour.");
    }

    static void ReplaceAllBackgrounds(string scenePrefabPath, string texPath)
    {
        // Generer les backgrounds textures
        MakePNG(texPath, "bg_accueil", 1920, 1080, new Color(0.08f, 0.08f, 0.12f));
        MakePNG(texPath, "bg_menu", 1920, 1080, new Color(0.08f, 0.08f, 0.12f));
        MakePNG(texPath, "bg_score", 1920, 1080, new Color(0.08f, 0.08f, 0.12f));
        AssetDatabase.Refresh();
        foreach (var bgt in new[] { "bg_accueil", "bg_menu", "bg_score" })
            SetSpriteMode(texPath + "/" + bgt + ".png");
        AssetDatabase.Refresh();

        // Mapping: prefab -> background texture
        var mapping = new[] {
            ("Accueil_Demolition.prefab", "bg_accueil"),
            ("Menu_Demolition.prefab", "bg_menu"),
            ("Score_Demolition.prefab", "bg_score"),
        };

        foreach (var (prefabName, texName) in mapping)
        {
            string fullPath = scenePrefabPath + "/" + prefabName;
            if (!File.Exists(fullPath))
            {
                Debug.LogWarning("Prefab introuvable: " + fullPath);
                continue;
            }

            Sprite newBg = LoadSprite(texPath + "/" + texName + ".png");
            if (newBg == null) { Debug.LogWarning("Background texture not found: " + texName); continue; }

            GameObject go = PrefabUtility.LoadPrefabContents(fullPath);
            if (go == null) continue;

            int count = 0;
            // Parcourir TOUS les Image et SpriteRenderer, pas juste ceux nommes "Background"
            var images = go.GetComponentsInChildren<Image>(true);
            foreach (var img in images)
            {
                img.sprite = newBg;
                count++;
                break; // Seulement le premier Image (le fond)
            }
            // Fallback: SpriteRenderer
            if (count == 0)
            {
                var srs = go.GetComponentsInChildren<SpriteRenderer>(true);
                foreach (var sr in srs)
                {
                    sr.sprite = newBg;
                    count++;
                    break;
                }
            }

            if (count > 0)
            {
                PrefabUtility.SaveAsPrefabAsset(go, fullPath);
                Debug.Log("  Background mis a jour dans " + prefabName);
            }
            else
                Debug.LogWarning("  Aucun Image/SpriteRenderer trouve dans " + prefabName);

            PrefabUtility.UnloadPrefabContents(go);
        }
    }

    static void SetupGameScene(string prefabPath, string soundPath, string demPrefabs)
    {
        string scenePath = "Assets/Projects/Demolition/Demolition_Scenes/GameScene_Demolition.unity";
        var scene = EditorSceneManager.OpenScene(scenePath);

        // Nettoyer anciens objets
        foreach (var name in new[] { "Demolition_GameManager", "Background", "StructuresParent", "GeneralVariable", "EventSystem" })
        {
            var old = GameObject.Find(name);
            if (old != null) DestroyImmediate(old);
        }

        // Camera si absente
        if (Camera.main == null)
        {
            new GameObject("Main Camera", typeof(Camera), typeof(AudioListener)) { tag = "MainCamera" };
            Camera.main.orthographic = true;
            Camera.main.orthographicSize = 5;
            Camera.main.backgroundColor = new Color(0.05f, 0.05f, 0.08f);
            Camera.main.transform.position = new Vector3(0, 0, -10);
        }

        // EventSystem
        var es = new GameObject("EventSystem", typeof(EventSystem));
        es.AddComponent<StandaloneInputModule>();

        // Background
        var bg = new GameObject("Background", typeof(SpriteRenderer));
        bg.transform.position = new Vector3(0, 0, 5);
        bg.transform.localScale = new Vector3(50, 30, 1);
        var sr = bg.GetComponent<SpriteRenderer>();
        sr.sortingOrder = -10;

        // StructuresParent
        var sp = new GameObject("StructuresParent");
        sp.transform.position = new Vector3(0, -2, 0);

        // GeneralVariable
        GameObject gvPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(demPrefabs + "/GeneralVariable.prefab");
        if (gvPrefab != null)
        {
            var gv = (GameObject)PrefabUtility.InstantiatePrefab(gvPrefab);
            gv.name = "GeneralVariable";
            gv.GetComponent<Demolition_GeneralVariables>().gameName = "Demolition";
        }

        // GameManager
        var gmGO = new GameObject("Demolition_GameManager", typeof(Demolition_GameManager), typeof(AudioSource));
        var gm = gmGO.GetComponent<Demolition_GameManager>();
        gm.structuresParent = sp.transform;
        gm.impactSound = AssetDatabase.LoadAssetAtPath<AudioClip>(soundPath + "/impact.wav");
        gm.destructionSound = AssetDatabase.LoadAssetAtPath<AudioClip>(soundPath + "/destruction.wav");
        gm.gameOverSound = AssetDatabase.LoadAssetAtPath<AudioClip>(soundPath + "/gameover.wav");
        gm.oiseauPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath + "/Oiseau.prefab");
        gm.impactEffectPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath + "/ImpactExplosion.prefab");

        EditorSceneManager.SaveScene(scene);
        Debug.Log("GameScene configuree: Camera, EventSystem, Background, StructuresParent, GeneralVariable, GameManager");
    }

    // --- Methodes de generation ---
    static void MakePNG(string folder, string name, int w, int h, Color color)
    {
        Directory.CreateDirectory(folder);
        string path = folder + "/" + name + ".png";
        if (File.Exists(path)) return;
        Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        Color[] px = new Color[w * h];
        for (int i = 0; i < px.Length; i++) px[i] = color;
        tex.SetPixels(px); tex.Apply();
        File.WriteAllBytes(path, tex.EncodeToPNG());
        DestroyImmediate(tex);
    }

    static void SetSpriteMode(string path)
    {
        if (!File.Exists(path)) return;
        TextureImporter imp = AssetImporter.GetAtPath(path) as TextureImporter;
        if (imp != null) { imp.textureType = TextureImporterType.Sprite; imp.spriteImportMode = SpriteImportMode.Single; imp.SaveAndReimport(); }
    }

    static Sprite LoadSprite(string path) { return AssetDatabase.LoadAssetAtPath<Sprite>(path); }

    static void MakeWAV(string path, float freq, float dur, float vol)
    {
        if (File.Exists(path)) return;
        Directory.CreateDirectory(Path.GetDirectoryName(path));
        int sr = 44100; int samples = (int)(sr * dur); float[] data = new float[samples];
        for (int i = 0; i < samples; i++) { float t = (float)i / sr; float env = 1f - (t / dur); data[i] = Mathf.Sin(2 * Mathf.PI * freq * t) * vol * env * env; }
        using (var fs = new FileStream(path, FileMode.Create)) using (var bw = new BinaryWriter(fs))
        {
            bw.Write("RIFF".ToCharArray()); bw.Write(36 + samples * 2);
            bw.Write("WAVEfmt ".ToCharArray()); bw.Write(16); bw.Write((short)1); bw.Write((short)1); bw.Write(sr); bw.Write(sr * 2); bw.Write((short)2); bw.Write((short)16);
            bw.Write("data".ToCharArray()); bw.Write(samples * 2);
            foreach (float s in data) bw.Write((short)(Mathf.Clamp(s, -1f, 1f) * 32767));
        }
    }

    static GameObject CreateBloc(string path, string name, Sprite sprite, Demolition_Block.MaterialType mat, int hp, int pts)
    {
        GameObject go = new GameObject(name, typeof(SpriteRenderer), typeof(BoxCollider2D), typeof(Rigidbody2D), typeof(Demolition_Block), typeof(AudioSource));
        var sr = go.GetComponent<SpriteRenderer>(); sr.sprite = sprite; sr.sortingOrder = 1;
        go.GetComponent<Rigidbody2D>().gravityScale = 1; go.GetComponent<Rigidbody2D>().mass = 1; go.GetComponent<Rigidbody2D>().linearDamping = 0.5f;
        var b = go.GetComponent<Demolition_Block>(); b.hp = hp; b.points = pts; b.materialType = mat; b.spriteRenderer = sr; b.damageSprites = new Sprite[] { sprite, sprite, sprite }; b.debrisForce = 200f;
        string fp = path + "/" + name + ".prefab"; PrefabUtility.SaveAsPrefabAsset(go, fp); DestroyImmediate(go);
        return AssetDatabase.LoadAssetAtPath<GameObject>(fp);
    }

    static GameObject CreateDebris(string path, string name, Sprite sprite)
    {
        GameObject go = new GameObject(name, typeof(SpriteRenderer), typeof(Rigidbody2D), typeof(BoxCollider2D));
        go.GetComponent<SpriteRenderer>().sprite = sprite; go.GetComponent<SpriteRenderer>().sortingOrder = 2;
        go.GetComponent<Rigidbody2D>().gravityScale = 1; go.GetComponent<Rigidbody2D>().mass = 0.2f;
        string fp = path + "/" + name + ".prefab"; PrefabUtility.SaveAsPrefabAsset(go, fp); DestroyImmediate(go);
        return AssetDatabase.LoadAssetAtPath<GameObject>(fp);
    }

    static void LinkDebris(GameObject block, GameObject debris) { block.GetComponent<Demolition_Block>().debrisPrefab = debris; PrefabUtility.SavePrefabAsset(block); }

    static void CreateOiseau(string path, Sprite oiSprite, Sprite imSprite)
    {
        var imp = new GameObject("ImpactExplosion", typeof(SpriteRenderer));
        imp.GetComponent<SpriteRenderer>().sprite = imSprite; imp.GetComponent<SpriteRenderer>().sortingOrder = 4;
        string ip = path + "/ImpactExplosion.prefab"; PrefabUtility.SaveAsPrefabAsset(imp, ip); DestroyImmediate(imp);
        var go = new GameObject("Oiseau", typeof(SpriteRenderer), typeof(Demolition_Projectile));
        go.GetComponent<SpriteRenderer>().sprite = oiSprite; go.GetComponent<SpriteRenderer>().sortingOrder = 3;
        var p = go.GetComponent<Demolition_Projectile>(); p.oiseauDos = oiSprite; p.spriteRenderer = go.GetComponent<SpriteRenderer>();
        p.vitesseDepart = 5; p.acceleration = 2; p.scaleMin = 0.1f; p.scaleMax = 1; p.forceExplosion = 500; p.radiusExplosion = 2;
        p.explosionPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(ip);
        PrefabUtility.SaveAsPrefabAsset(go, path + "/Oiseau.prefab"); DestroyImmediate(go);
    }

    static void MakeStruct(string path, string name, string[] blocks, Vector3[] pos)
    {
        var go = new GameObject(name, typeof(Demolition_Structure));
        var s = go.GetComponent<Demolition_Structure>(); s.blocs = new Demolition_Block[blocks.Length];
        for (int i = 0; i < blocks.Length; i++)
        {
            var bp = AssetDatabase.LoadAssetAtPath<GameObject>(path + "/" + blocks[i] + ".prefab");
            if (bp == null) continue;
            var b = (GameObject)PrefabUtility.InstantiatePrefab(bp, go.transform);
            b.transform.localPosition = pos[i]; s.blocs[i] = b.GetComponent<Demolition_Block>();
        }
        PrefabUtility.SaveAsPrefabAsset(go, path + "/" + name + ".prefab"); DestroyImmediate(go);
    }

    [MenuItem("Tools/Demolition - Ajouter Toggle & Slider")]
    static void AddMenuControls()
    {
        string scenePath = "Assets/Projects/Demolition/Demolition_Scenes/Menu_Demolition.unity";
        var scene = EditorSceneManager.OpenScene(scenePath);

        // Trouver le canvas
        var canvas = Object.FindFirstObjectByType<Canvas>();
        if (canvas == null) { Debug.LogError("Canvas pas trouve!"); return; }

        // --- Toggle ModeOiseau ---
        GameObject toggleGO = new GameObject("ModeOiseau", typeof(RectTransform));
        toggleGO.transform.SetParent(canvas.transform);
        toggleGO.AddComponent<Image>();
        Toggle toggle = toggleGO.AddComponent<Toggle>();
        toggle.isOn = PlayerPrefs.GetInt(Demolition_GeneralVariables.ModeOiseauKey, 1) == 1;

        // Checkmark
        GameObject checkGO = new GameObject("Checkmark", typeof(RectTransform));
        checkGO.transform.SetParent(toggleGO.transform);
        Image checkImage = checkGO.AddComponent<Image>();
        checkGO.AddComponent<CanvasRenderer>();

        // Relier checkmark au toggle
        toggle.graphic = checkImage;
        toggle.targetGraphic = checkImage;

        // Label "Oiseau / Impact"
        GameObject labelGO = new GameObject("Label", typeof(RectTransform));
        labelGO.transform.SetParent(toggleGO.transform);
        var labelText = labelGO.AddComponent<TextMeshProUGUI>();
        labelText.text = "Mode Oiseau";
        labelText.fontSize = 24;
        labelText.alignment = TextAlignmentOptions.Left;

        // Position toggle
        RectTransform rt = toggleGO.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = new Vector2(-200, 100);
        rt.sizeDelta = new Vector2(200, 50);

        Debug.Log("Toggle ModeOiseau ajoute");

        // --- Slider ScrollSpeed ---
        GameObject sliderGO = new GameObject("ScrollSpeed", typeof(RectTransform));
        sliderGO.transform.SetParent(canvas.transform);
        Slider slider = sliderGO.AddComponent<Slider>();
        sliderGO.AddComponent<Image>();

        // Background du slider
        GameObject bgGO = new GameObject("Background", typeof(RectTransform));
        bgGO.transform.SetParent(sliderGO.transform);
        var bgImage = bgGO.AddComponent<Image>();
        bgImage.color = Color.gray;

        // Fill
        GameObject fillGO = new GameObject("Fill", typeof(RectTransform));
        fillGO.transform.SetParent(sliderGO.transform);
        var fillImage = fillGO.AddComponent<Image>();
        fillImage.color = Color.white;

        // Handle
        GameObject handleGO = new GameObject("Handle", typeof(RectTransform));
        handleGO.transform.SetParent(sliderGO.transform);
        var handleImage = handleGO.AddComponent<Image>();
        handleImage.color = Color.white;

        // Relier slider
        slider.fillRect = fillGO.GetComponent<RectTransform>();
        slider.handleRect = handleGO.GetComponent<RectTransform>();
        slider.targetGraphic = handleImage;
        slider.minValue = 1f;
        slider.maxValue = 5f;
        slider.value = PlayerPrefs.GetFloat(Demolition_GeneralVariables.ScrollSpeedKey, 2f);
        slider.wholeNumbers = true;

        // Label "Vitesse"
        GameObject speedLabel = new GameObject("Label", typeof(RectTransform));
        speedLabel.transform.SetParent(canvas.transform);
        var speedText = speedLabel.AddComponent<TextMeshProUGUI>();
        speedText.text = "Vitesse";
        speedText.fontSize = 24;
        speedText.alignment = TextAlignmentOptions.Left;

        // Position slider
        RectTransform srt = sliderGO.GetComponent<RectTransform>();
        srt.anchorMin = new Vector2(0.5f, 0.5f);
        srt.anchorMax = new Vector2(0.5f, 0.5f);
        srt.anchoredPosition = new Vector2(-200, 0);
        srt.sizeDelta = new Vector2(300, 50);

        Debug.Log("Slider ScrollSpeed ajoute");

        EditorSceneManager.SaveScene(scene);
        Debug.Log("Menu mis a jour: Toggle ModeOiseau + Slider ScrollSpeed ajoutes!");
    }
}