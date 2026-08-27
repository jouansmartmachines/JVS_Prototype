using UnityEngine;
using UnityEditor;
using System.IO;

public class Demolition_Setup : EditorWindow
{
    [MenuItem("Tools/Demolition - Créer les prefabs et scènes")]
    static void Setup()
    {
        string basePath = "Assets/Projects/Demolition";
        string prefabPath = basePath + "/Demolition_Prefabs";
        string scenePath = basePath + "/Demolition_Scenes";
        string spritePath = basePath + "/Resources/Sprites";

        Directory.CreateDirectory(prefabPath);
        Directory.CreateDirectory(scenePath);

        // Charger les sprites
        Sprite boisSprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath + "/bois.png");
        Sprite verreSprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath + "/verre.png");
        Sprite pierreSprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath + "/pierre.png");
        Sprite oiseauSprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath + "/oiseau.png");
        Sprite impactSprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath + "/impact.png");
        Sprite debrisBois = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath + "/debris_bois.png");
        Sprite debrisVerre = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath + "/debris_verre.png");
        Sprite debrisPierre = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath + "/debris_pierre.png");
        Sprite particuleSprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath + "/particule.png");

        // --- 1. Créer les prefabs de blocs ---
        string[] matTypes = { "Bois", "Verre", "Pierre" };
        Sprite[] matSprites = { boisSprite, verreSprite, pierreSprite };
        Sprite[] matDebris = { debrisBois, debrisVerre, debrisPierre };
        int[] matHp = { 2, 1, 4 };
        string[] matColors = { "C8A87C", "ADD8E6", "808080" };

        for (int i = 0; i < matTypes.Length; i++)
        {
            GameObject block = new GameObject("Bloc_" + matTypes[i], typeof(SpriteRenderer), typeof(BoxCollider2D), typeof(Rigidbody2D), typeof(Demolition_Block), typeof(AudioSource));
            var sr = block.GetComponent<SpriteRenderer>();
            sr.sprite = matSprites[i];
            sr.sortingOrder = 1;
            var rb = block.GetComponent<Rigidbody2D>();
            rb.gravityScale = 1;
            rb.mass = 1;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            var col = block.GetComponent<BoxCollider2D>();
            col.size = new Vector2(0.64f, 0.32f);
            var demo = block.GetComponent<Demolition_Block>();
            demo.hp = matHp[i];
            demo.points = 50 * (i + 1);
            demo.materialType = (Demolition_Block.MaterialType)i;
            demo.spriteRenderer = sr;
            // Create damage sprites array (same sprite for now)
            demo.damageSprites = new Sprite[] { matSprites[i], matSprites[i], matSprites[i] };
            // Create debris prefab
            GameObject debris = new GameObject("Debris_" + matTypes[i], typeof(SpriteRenderer));
            var dsr = debris.GetComponent<SpriteRenderer>();
            dsr.sprite = matDebris[i];
            dsr.sortingOrder = 2;
            debris.AddComponent<Rigidbody2D>();
            debris.AddComponent<BoxCollider2D>();
            PrefabUtility.SaveAsPrefabAsset(debris, prefabPath + "/Debris_" + matTypes[i] + ".prefab");
            Object.DestroyImmediate(debris);
            demo.debrisPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath + "/Debris_" + matTypes[i] + ".prefab");
            demo.debrisForce = 200f;
            
            string path = prefabPath + "/Bloc_" + matTypes[i] + ".prefab";
            PrefabUtility.SaveAsPrefabAsset(block, path);
            Object.DestroyImmediate(block);
            Debug.Log("Créé: " + path);
        }

        // --- 2. Créer le prefab Oiseau ---
        GameObject oiseau = new GameObject("Oiseau", typeof(SpriteRenderer), typeof(Demolition_Projectile));
        var osr = oiseau.GetComponent<SpriteRenderer>();
        osr.sprite = oiseauSprite;
        osr.sortingOrder = 3;
        var proj = oiseau.GetComponent<Demolition_Projectile>();
        proj.spriteRenderer = osr;
        proj.oiseauDos = oiseauSprite;
        proj.vitesseDepart = 5f;
        proj.acceleration = 2f;
        proj.scaleMin = 0.1f;
        proj.scaleMax = 1f;
        // Impact prefab
        GameObject impact = new GameObject("ImpactExplosion", typeof(SpriteRenderer));
        var isr = impact.GetComponent<SpriteRenderer>();
        isr.sprite = impactSprite;
        isr.sortingOrder = 4;
        var pathImpact = prefabPath + "/ImpactExplosion.prefab";
        PrefabUtility.SaveAsPrefabAsset(impact, pathImpact);
        Object.DestroyImmediate(impact);
        proj.explosionPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(pathImpact);
        proj.forceExplosion = 500f;
        proj.radiusExplosion = 2f;
        
        var pathOiseau = prefabPath + "/Oiseau.prefab";
        PrefabUtility.SaveAsPrefabAsset(oiseau, pathOiseau);
        Object.DestroyImmediate(oiseau);
        Debug.Log("Créé: " + pathOiseau);

        // --- 3. Créer le prefab Structure (assemblage de blocs) ---
        GameObject structure = new GameObject("Structure_Exemple", typeof(Demolition_Structure));
        // Add 3 blocks as children
        for (int i = 0; i < 3; i++)
        {
            var blockPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath + "/Bloc_Bois.prefab");
            GameObject b = (GameObject)PrefabUtility.InstantiatePrefab(blockPrefab, structure.transform);
            b.transform.localPosition = new Vector3(i * 0.7f, i * 0.35f, 0);
        }
        var structScript = structure.GetComponent<Demolition_Structure>();
        structScript.blocs = structure.GetComponentsInChildren<Demolition_Block>();
        var pathStruct = prefabPath + "/Structure_Exemple.prefab";
        PrefabUtility.SaveAsPrefabAsset(structure, pathStruct);
        Object.DestroyImmediate(structure);
        Debug.Log("Créé: " + pathStruct);

        // --- 4. Créer les tableaux de structures ---
        for (int t = 0; t < 3; t++)
        {
            GameObject tableau = new GameObject("Tableau_" + (t + 1), typeof(Demolition_Structure));
            string mat = matTypes[t % 3];
            var blockPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath + "/Bloc_" + mat + ".prefab");
            for (int i = 0; i < 4; i++)
            {
                GameObject b = (GameObject)PrefabUtility.InstantiatePrefab(blockPrefab, tableau.transform);
                b.transform.localPosition = new Vector3(i * 0.7f - 1.05f, Mathf.Sin(i * 1.5f) * 0.5f + 0.5f, 0);
            }
            var ts = tableau.GetComponent<Demolition_Structure>();
            ts.blocs = tableau.GetComponentsInChildren<Demolition_Block>();
            var pathTab = prefabPath + "/Tableau_" + (t + 1) + ".prefab";
            PrefabUtility.SaveAsPrefabAsset(tableau, pathTab);
            Object.DestroyImmediate(tableau);
            Debug.Log("Créé: " + pathTab);
        }

        // --- 5. Créer les 4 scènes ---
        CreateScene(scenePath + "/Accueil_Demolition.unity", "Accueil", oiseauSprite, "Play");
        CreateScene(scenePath + "/Menu_Demolition.unity", "Menu", null, "Menu");
        CreateScene(scenePath + "/GameScene_Demolition.unity", "GameScene", null, "Game");
        CreateScene(scenePath + "/Score_Demolition.unity", "Score", null, "Score");

        // Build settings
        AddSceneToBuildSettings(scenePath + "/Accueil_Demolition.unity");
        AddSceneToBuildSettings(scenePath + "/Menu_Demolition.unity");
        AddSceneToBuildSettings(scenePath + "/GameScene_Demolition.unity");
        AddSceneToBuildSettings(scenePath + "/Score_Demolition.unity");

        AssetDatabase.Refresh();
        Debug.Log("✅ Setup Demolition terminé ! Ouvre Tools/Demolition - Créer les prefabs et scènes si besoin.");
    }

    static void CreateScene(string path, string type, Sprite logo, string label)
    {
        var scene = UnityEditor.SceneManagement.EditorSceneManager.NewScene(UnityEditor.SceneManagement.NewSceneSetup.EmptyScene, UnityEditor.SceneManagement.NewSceneMode.Single);
        
        // Camera
        Camera cam = Camera.main;
        if (cam == null)
        {
            GameObject camGo = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
            cam = camGo.GetComponent<Camera>();
            camGo.tag = "MainCamera";
        }
        cam.orthographic = true;
        cam.orthographicSize = 5;
        cam.backgroundColor = new Color(0.1f, 0.1f, 0.15f);
        cam.transform.position = new Vector3(0, 0, -10);

        // EventSystem
        GameObject es = new GameObject("EventSystem", typeof(UnityEngine.EventSystems.EventSystem), typeof(UnityEngine.EventSystems.StandaloneInputModule));

        // Canvas
        GameObject canvasGo = new GameObject("Canvas", typeof(Canvas), typeof(UnityEngine.UI.CanvasScaler), typeof(UnityEngine.UI.GraphicRaycaster));
        Canvas canvas = canvasGo.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasGo.GetComponent<UnityEngine.UI.CanvasScaler>();
        scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        // OSC_Manager
        GameObject oscGo = new GameObject("OSC_Manager");
        var oscManager = oscGo.AddComponent<OSC_Manager>();
        // Ensure OSC_Manager exists
        var oscPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Installedpackages/OSCManager_Scripts/Prefabs/OSC_Manager.prefab");
        if (oscPrefab != null)
        {
            GameObject.DestroyImmediate(oscGo);
            oscGo = (GameObject)PrefabUtility.InstantiatePrefab(oscPrefab);
            oscGo.name = "OSC_Manager";
        }

        // GeneralVariable
        GameObject gvGo = new GameObject("Demolition_GeneralVariables", typeof(Demolition_GeneralVariables));
        var gv = gvGo.GetComponent<Demolition_GeneralVariables>();
        gv.gameName = "Demolition";

        // Specific setup per scene type
        if (type == "GameScene")
        {
            cam.orthographic = true;
            cam.orthographicSize = 5;
            cam.transform.position = new Vector3(0, 0, -10);

            // GameManager
            GameObject gmGo = new GameObject("Demolition_GameManager", typeof(Demolition_GameManager));
            var gm = gmGo.GetComponent<Demolition_GameManager>();
            gm.oiseauPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Projects/Demolition/Demolition_Prefabs/Oiseau.prefab");
            gm.impactEffectPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Projects/Demolition/Demolition_Prefabs/ImpactExplosion.prefab");
            gm.tableauPrefabs = new GameObject[] {
                AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Projects/Demolition/Demolition_Prefabs/Tableau_1.prefab"),
                AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Projects/Demolition/Demolition_Prefabs/Tableau_2.prefab"),
                AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Projects/Demolition/Demolition_Prefabs/Tableau_3.prefab"),
            };
            var structuresParent = new GameObject("StructuresParent");
            structuresParent.transform.position = new Vector3(0, -2, 0);
            gm.structuresParent = structuresParent.transform;
            gmGo.AddComponent<AudioSource>();

            // ScrollingBackground (layers)
            GameObject bg = new GameObject("Background");
            for (int i = 0; i < 3; i++)
            {
                GameObject layer = new GameObject("Layer_" + i, typeof(SpriteRenderer));
                layer.transform.SetParent(bg.transform);
                layer.transform.position = new Vector3(0, 0, 5 - i);
                layer.GetComponent<SpriteRenderer>().color = new Color(0.1f, 0.1f + i * 0.05f, 0.15f + i * 0.05f);
                // Create a simple white square texture procedurally
                var tex = new Texture2D(1, 1);
                tex.SetPixel(0, 0, Color.white);
                tex.Apply();
                var s = Sprite.Create(tex, new Rect(0, 0, 1, 1), Vector2.one * 0.5f, 100);
                layer.GetComponent<SpriteRenderer>().sprite = s;
                layer.transform.localScale = new Vector3(100, 20, 1);
            }
            var scroll = bg.AddComponent<Demolition_ScrollingBackground>();
            scroll.backgroundLayers = new Transform[bg.transform.childCount];
            for (int i = 0; i < bg.transform.childCount; i++)
                scroll.backgroundLayers[i] = bg.transform.GetChild(i);
            scroll.layerSpeeds = new float[] { 0.1f, 0.3f, 0.6f };
            scroll.tableauPrefabs = gm.tableauPrefabs;
            scroll.tableauSpawnPoint = new GameObject("TableauSpawn").transform;
            scroll.tableauSpawnPoint.position = new Vector3(10, -2, 0);

            // OSCGameScene
            GameObject oscGs = new GameObject("OSCGameScene", typeof(OSCGameScene));
        }
        else if (type == "Accueil")
        {
            // Title text
            GameObject title = new GameObject("Title", typeof(UnityEngine.UI.Text));
            title.transform.SetParent(canvasGo.transform);
            var text = title.GetComponent<UnityEngine.UI.Text>();
            text.text = "DÉMOLITION";
            text.fontSize = 80;
            text.fontStyle = FontStyle.Bold;
            text.color = Color.white;
            text.alignment = TextAnchor.MiddleCenter;
            var rt = title.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.7f);
            rt.anchorMax = new Vector2(0.5f, 0.7f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(600, 120);
            rt.anchoredPosition = Vector2.zero;

            // Play button
            GameObject btnGo = new GameObject("PlayButton", typeof(UnityEngine.UI.Image), typeof(UnityEngine.UI.Button));
            btnGo.transform.SetParent(canvasGo.transform);
            var btn = btnGo.GetComponent<UnityEngine.UI.Button>();
            btnGo.GetComponent<UnityEngine.UI.Image>().color = new Color(0.2f, 0.6f, 0.2f);
            var btnRt = btnGo.GetComponent<RectTransform>();
            btnRt.anchorMin = new Vector2(0.5f, 0.4f);
            btnRt.anchorMax = new Vector2(0.5f, 0.4f);
            btnRt.pivot = new Vector2(0.5f, 0.5f);
            btnRt.sizeDelta = new Vector2(200, 80);
            btnRt.anchoredPosition = Vector2.zero;

            // Play text
            GameObject playText = new GameObject("PlayText", typeof(UnityEngine.UI.Text));
            playText.transform.SetParent(btnGo.transform);
            var pt = playText.GetComponent<UnityEngine.UI.Text>();
            pt.text = "JOUER";
            pt.fontSize = 40;
            pt.fontStyle = FontStyle.Bold;
            pt.color = Color.white;
            pt.alignment = TextAnchor.MiddleCenter;
            var ptRt = playText.GetComponent<RectTransform>();
            ptRt.anchorMin = Vector2.zero;
            ptRt.anchorMax = Vector2.one;
            ptRt.sizeDelta = Vector2.zero;
            ptRt.anchoredPosition = Vector2.zero;
        }
        else if (type == "Menu")
        {
            GameObject title = new GameObject("Title", typeof(UnityEngine.UI.Text));
            title.transform.SetParent(canvasGo.transform);
            var text = title.GetComponent<UnityEngine.UI.Text>();
            text.text = "Menu Démolition";
            text.fontSize = 60;
            text.color = Color.white;
            text.alignment = TextAnchor.MiddleCenter;
            var rt = title.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.85f);
            rt.anchorMax = new Vector2(0.5f, 0.85f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(400, 80);
        }
        else if (type == "Score")
        {
            GameObject title = new GameObject("Title", typeof(UnityEngine.UI.Text));
            title.transform.SetParent(canvasGo.transform);
            var text = title.GetComponent<UnityEngine.UI.Text>();
            text.text = "Score";
            text.fontSize = 60;
            text.color = Color.white;
            text.alignment = TextAnchor.MiddleCenter;
            var rt = title.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.85f);
            rt.anchorMax = new Vector2(0.5f, 0.85f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(400, 80);

            // OSCScore
            GameObject oscScore = new GameObject("OSCScore", typeof(OSCScore));
        }

        UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene, path);
        Debug.Log("Créée: " + path);
    }

    static void AddSceneToBuildSettings(string path)
    {
        var editorBuildSettingsScenes = new System.Collections.Generic.List<UnityEditor.EditorBuildSettingsScene>(
            UnityEditor.EditorBuildSettings.scenes);
        // Check if already added
        bool found = false;
        foreach (var s in editorBuildSettingsScenes)
        {
            if (s.path == path) { found = true; break; }
        }
        if (!found)
        {
            editorBuildSettingsScenes.Add(new UnityEditor.EditorBuildSettingsScene(path, true));
            UnityEditor.EditorBuildSettings.scenes = editorBuildSettingsScenes.ToArray();
            Debug.Log("Ajouté aux Build Settings: " + path);
        }
    }
}