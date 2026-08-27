using UnityEngine;
using UnityEditor;
using System.IO;
using UnityEngine.UI;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using Demolition;

public class Demolition_SetupEditor : EditorWindow
{
    [MenuItem("Tools/Demolition - Generer les prefabs")]
    static void GenerateAll()
    {
        string basePath = "Assets/Projects/Demolition";
        string prefabPath = basePath + "/Resources/Prefabs";
        string texPath = basePath + "/Resources/Textures";
        string soundPath = basePath + "/Resources/Sounds";
        string scenePf = basePath + "/ScenePrefabs";

        // 1. Textures gameplay
        MakePNG(texPath, "bois", 64, 32, new Color(0.545f, 0.353f, 0.169f));
        MakePNG(texPath, "verre", 64, 32, new Color(0.678f, 0.847f, 0.902f, 0.7f));
        MakePNG(texPath, "pierre", 64, 32, new Color(0.5f, 0.5f, 0.5f));
        MakePNG(texPath, "oiseau", 32, 32, new Color(0.863f, 0.196f, 0.196f));
        MakePNG(texPath, "impact", 64, 64, new Color(1f, 0.647f, 0f));
        MakePNG(texPath, "debris_bois", 16, 8, new Color(0.545f, 0.353f, 0.169f));
        MakePNG(texPath, "debris_verre", 8, 8, new Color(0.678f, 0.847f, 0.902f));
        MakePNG(texPath, "debris_pierre", 12, 12, new Color(0.5f, 0.5f, 0.5f));
        // Backgrounds for scenes
        MakePNG(texPath, "bg_accueil", 1920, 1080, new Color(0.08f, 0.08f, 0.12f));
        MakePNG(texPath, "bg_menu", 1920, 1080, new Color(0.08f, 0.08f, 0.12f));
        MakePNG(texPath, "bg_score", 1920, 1080, new Color(0.08f, 0.08f, 0.12f));
        MakePNG(texPath, "bg_game", 1920, 1080, new Color(0.05f, 0.05f, 0.08f));

        AssetDatabase.Refresh();
        foreach (var tex in new[] { "bois", "verre", "pierre", "oiseau", "impact", "debris_bois", "debris_verre", "debris_pierre", "bg_accueil", "bg_menu", "bg_score", "bg_game" })
            SetSpriteMode(texPath + "/" + tex + ".png");
        AssetDatabase.Refresh();

        // 2. Sons
        MakeWAV(soundPath + "/impact.wav", 440, 0.15f, 0.8f);
        MakeWAV(soundPath + "/destruction.wav", 220, 0.3f, 0.7f);
        MakeWAV(soundPath + "/gameover.wav", 180, 0.5f, 0.6f);
        AssetDatabase.Refresh();

        // 3. Charger sprites
        Sprite sBois = LoadSprite(texPath + "/bois.png");
        Sprite sVerre = LoadSprite(texPath + "/verre.png");
        Sprite sPierre = LoadSprite(texPath + "/pierre.png");
        Sprite sOiseau = LoadSprite(texPath + "/oiseau.png");
        Sprite sImpact = LoadSprite(texPath + "/impact.png");
        Sprite sDBois = LoadSprite(texPath + "/debris_bois.png");
        Sprite sDVerre = LoadSprite(texPath + "/debris_verre.png");
        Sprite sDPierre = LoadSprite(texPath + "/debris_pierre.png");
        Sprite bgAccueil = LoadSprite(texPath + "/bg_accueil.png");
        Sprite bgMenu = LoadSprite(texPath + "/bg_menu.png");
        Sprite bgScore = LoadSprite(texPath + "/bg_score.png");
        Sprite bgGame = LoadSprite(texPath + "/bg_game.png");

        if (sBois == null) { Debug.LogError("ERREUR: sprites non charges"); return; }

        // 4. Blocs + debris + oiseau (gameplay)
        var bBois = CreateBloc(prefabPath, "Bloc_Bois", sBois, Demolition_Block.MaterialType.Bois, 2, 50);
        var bVerre = CreateBloc(prefabPath, "Bloc_Verre", sVerre, Demolition_Block.MaterialType.Verre, 1, 100);
        var bPierre = CreateBloc(prefabPath, "Bloc_Pierre", sPierre, Demolition_Block.MaterialType.Pierre, 4, 150);
        var dBois = CreateDebris(prefabPath, "Debris_Bois", sDBois);
        var dVerre = CreateDebris(prefabPath, "Debris_Verre", sDVerre);
        var dPierre = CreateDebris(prefabPath, "Debris_Pierre", sDPierre);
        LinkDebris(bBois, dBois); LinkDebris(bVerre, dVerre); LinkDebris(bPierre, dPierre);
        CreateOiseau(prefabPath, sOiseau, sImpact);
        MakeStruct(prefabPath, "Structure_Exemple", new[] { "Bloc_Bois", "Bloc_Bois", "Bloc_Bois" },
            new Vector3[] { new(0,0,0), new(0.7f,0.35f,0), new(1.4f,0.7f,0) });
        MakeStruct(prefabPath, "Tableau_1", new[] { "Bloc_Bois", "Bloc_Verre", "Bloc_Bois", "Bloc_Pierre" },
            new Vector3[] { new(0,0,0), new(0.7f,0.35f,0), new(1.4f,0,0), new(2.1f,0.35f,0) });
        MakeStruct(prefabPath, "Tableau_2", new[] { "Bloc_Pierre", "Bloc_Bois", "Bloc_Verre", "Bloc_Verre" },
            new Vector3[] { new(0,0,0), new(0.7f,0.7f,0), new(1.4f,0,0), new(2.1f,0,0) });
        MakeStruct(prefabPath, "Tableau_3", new[] { "Bloc_Verre", "Bloc_Bois", "Bloc_Pierre", "Bloc_Bois" },
            new Vector3[] { new(0,0,0), new(0.7f,0,0), new(1.4f,0.7f,0), new(0.7f,0.7f,0) });

        // 5. Remplacer les backgrounds dans les ScenePrefabs
        ReplaceBackground(scenePf + "/Accueil_Demolition.prefab", bgAccueil);
        ReplaceBackground(scenePf + "/Menu_Demolition.prefab", bgMenu);
        ReplaceBackground(scenePf + "/Score_Demolition.prefab", bgScore);

        // 6. Configurer la GameScene
        SetupGameScene(prefabPath, soundPath, bgGame);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("OK - Tout genere ! Accueil, Menu, Score, GameScene prets.");
    }

    static void ReplaceBackground(string prefabPath, Sprite newSprite)
    {
        if (!File.Exists(prefabPath)) { Debug.LogWarning("Prefab introuvable: " + prefabPath); return; }
        GameObject go = PrefabUtility.LoadPrefabContents(prefabPath);
        if (go == null) { Debug.LogWarning("Impossible de charger: " + prefabPath); return; }

        // Trouver tous les Image components avec "BackGround" ou "Background"
        int count = 0;
        var images = go.GetComponentsInChildren<Image>(true);
        foreach (var img in images)
        {
            if (img.name.ToLower().Contains("back") || img.name.ToLower().Contains("ground"))
            {
                img.sprite = newSprite;
                count++;
            }
        }
        // Aussi SpriteRenderer
        var srs = go.GetComponentsInChildren<SpriteRenderer>(true);
        foreach (var sr in srs)
        {
            if (sr.name.ToLower().Contains("back") || sr.name.ToLower().Contains("ground"))
            {
                sr.sprite = newSprite;
                count++;
            }
        }

        if (count > 0)
        {
            PrefabUtility.SaveAsPrefabAsset(go, prefabPath);
            Debug.Log("  Backgrounds mis a jour dans " + Path.GetFileName(prefabPath) + " (" + count + ")");
        }
        else
        {
            Debug.LogWarning("  Aucun Background trouve dans " + Path.GetFileName(prefabPath));
        }
        PrefabUtility.UnloadPrefabContents(go);
    }

    static void SetupGameScene(string prefabPath, string soundPath, Sprite bgGame)
    {
        string scenePath = "Assets/Projects/Demolition/Demolition_Scenes/GameScene_Demolition.unity";
        var scene = EditorSceneManager.OpenScene(scenePath);

        // Nettoyer les anciens objets
        var oldGm = GameObject.Find("Demolition_GameManager");
        if (oldGm != null) DestroyImmediate(oldGm);
        var oldBg = GameObject.Find("Background");
        if (oldBg != null) DestroyImmediate(oldBg);
        var oldSp = GameObject.Find("StructuresParent");
        if (oldSp != null) DestroyImmediate(oldSp);
        var oldEv = GameObject.Find("EventSystem");
        if (oldEv != null) DestroyImmediate(oldEv);
        var oldGv = GameObject.Find("GeneralVariable");
        if (oldGv != null) DestroyImmediate(oldGv);

        // Camera - si pas de camera, la creer
        if (Camera.main == null)
        {
            var camGO = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
            camGO.tag = "MainCamera";
            camGO.transform.position = new Vector3(0, 0, -10);
            var cam = camGO.GetComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 5;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.05f, 0.05f, 0.08f);
        }

        // Background
        var bgGO = new GameObject("Background", typeof(SpriteRenderer));
        bgGO.transform.position = new Vector3(0, 0, 5);
        var sr = bgGO.GetComponent<SpriteRenderer>();
        sr.sprite = bgGame;
        sr.sortingOrder = -10;
        bgGO.transform.localScale = new Vector3(50, 30, 1);

        // StructuresParent
        var spGO = new GameObject("StructuresParent");
        spGO.transform.position = new Vector3(0, -2, 0);

        // EventSystem
        var evGO = new GameObject("EventSystem", typeof(EventSystem));
        evGO.AddComponent<StandaloneInputModule>();

        // GameManager
        var gmGO = new GameObject("Demolition_GameManager", typeof(Demolition_GameManager), typeof(AudioSource));
        var gm = gmGO.GetComponent<Demolition_GameManager>();
        gm.structuresParent = spGO.transform;
        gm.impactSound = AssetDatabase.LoadAssetAtPath<AudioClip>(soundPath + "/impact.wav");
        gm.destructionSound = AssetDatabase.LoadAssetAtPath<AudioClip>(soundPath + "/destruction.wav");
        gm.gameOverSound = AssetDatabase.LoadAssetAtPath<AudioClip>(soundPath + "/gameover.wav");
        gm.oiseauPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath + "/Oiseau.prefab");
        gm.impactEffectPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath + "/ImpactExplosion.prefab");

        // GeneralVariable
        var gvPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Projects/Demolition/Demolition_Prefabs/GeneralVariable.prefab");
        if (gvPrefab != null)
        {
            var gv = (GameObject)PrefabUtility.InstantiatePrefab(gvPrefab);
            gv.name = "GeneralVariable";
            gv.GetComponent<Demolition_GeneralVariables>().gameName = "Demolition";
        }

        EditorSceneManager.SaveScene(scene);
        Debug.Log("GameScene configuree !");
    }

    // --- Methodes existantes (MakePNG, SetSpriteMode, LoadSprite, MakeWAV, CreateBloc, etc.) ---
    static void MakePNG(string folder, string name, int w, int h, Color color)
    {
        Directory.CreateDirectory(folder);
        string path = folder + "/" + name + ".png";
        if (File.Exists(path)) File.Delete(path);
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
        using (var fs = new FileStream(path, FileMode.Create))
        using (var bw = new BinaryWriter(fs))
        {
            bw.Write(new char[] { 'R','I','F','F' }); bw.Write(36 + samples * 2);
            bw.Write(new char[] { 'W','A','V','E','f','m','t',' ' }); bw.Write(16);
            bw.Write((short)1); bw.Write((short)1); bw.Write(sr); bw.Write(sr * 2);
            bw.Write((short)2); bw.Write((short)16);
            bw.Write(new char[] { 'd','a','t','a' }); bw.Write(samples * 2);
            foreach (float s in data) bw.Write((short)(Mathf.Clamp(s, -1f, 1f) * 32767));
        }
    }

    static GameObject CreateBloc(string path, string name, Sprite sprite, Demolition_Block.MaterialType mat, int hp, int pts)
    {
        GameObject go = new GameObject(name, typeof(SpriteRenderer), typeof(BoxCollider2D), typeof(Rigidbody2D), typeof(Demolition_Block), typeof(AudioSource));
        var sr = go.GetComponent<SpriteRenderer>(); sr.sprite = sprite; sr.sortingOrder = 1;
        var rb = go.GetComponent<Rigidbody2D>(); rb.gravityScale = 1; rb.mass = 1; rb.linearDamping = 0.5f;
        var b = go.GetComponent<Demolition_Block>(); b.hp = hp; b.points = pts; b.materialType = mat;
        b.spriteRenderer = sr; b.damageSprites = new Sprite[] { sprite, sprite, sprite }; b.debrisForce = 200f;
        string fp = path + "/" + name + ".prefab";
        PrefabUtility.SaveAsPrefabAsset(go, fp); DestroyImmediate(go);
        return AssetDatabase.LoadAssetAtPath<GameObject>(fp);
    }

    static GameObject CreateDebris(string path, string name, Sprite sprite)
    {
        GameObject go = new GameObject(name, typeof(SpriteRenderer), typeof(Rigidbody2D), typeof(BoxCollider2D));
        go.GetComponent<SpriteRenderer>().sprite = sprite; go.GetComponent<SpriteRenderer>().sortingOrder = 2;
        go.GetComponent<Rigidbody2D>().gravityScale = 1; go.GetComponent<Rigidbody2D>().mass = 0.2f;
        string fp = path + "/" + name + ".prefab";
        PrefabUtility.SaveAsPrefabAsset(go, fp); DestroyImmediate(go);
        return AssetDatabase.LoadAssetAtPath<GameObject>(fp);
    }

    static void LinkDebris(GameObject block, GameObject debris)
    {
        if (block == null || debris == null) return;
        block.GetComponent<Demolition_Block>().debrisPrefab = debris;
        PrefabUtility.SavePrefabAsset(block);
    }

    static void CreateOiseau(string path, Sprite oiSprite, Sprite imSprite)
    {
        GameObject imp = new GameObject("ImpactExplosion", typeof(SpriteRenderer));
        imp.GetComponent<SpriteRenderer>().sprite = imSprite; imp.GetComponent<SpriteRenderer>().sortingOrder = 4;
        string ip = path + "/ImpactExplosion.prefab"; PrefabUtility.SaveAsPrefabAsset(imp, ip); DestroyImmediate(imp);
        GameObject go = new GameObject("Oiseau", typeof(SpriteRenderer), typeof(Demolition_Projectile));
        go.GetComponent<SpriteRenderer>().sprite = oiSprite; go.GetComponent<SpriteRenderer>().sortingOrder = 3;
        var p = go.GetComponent<Demolition_Projectile>();
        p.oiseauDos = oiSprite; p.spriteRenderer = go.GetComponent<SpriteRenderer>();
        p.vitesseDepart = 5; p.acceleration = 2; p.scaleMin = 0.1f; p.scaleMax = 1;
        p.forceExplosion = 500; p.radiusExplosion = 2;
        p.explosionPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(ip);
        PrefabUtility.SaveAsPrefabAsset(go, path + "/Oiseau.prefab"); DestroyImmediate(go);
    }

    static void MakeStruct(string path, string name, string[] blocks, Vector3[] pos)
    {
        GameObject go = new GameObject(name, typeof(Demolition_Structure));
        var s = go.GetComponent<Demolition_Structure>(); s.blocs = new Demolition_Block[blocks.Length];
        for (int i = 0; i < blocks.Length; i++)
        {
            GameObject bp = AssetDatabase.LoadAssetAtPath<GameObject>(path + "/" + blocks[i] + ".prefab");
            if (bp == null) continue;
            GameObject b = (GameObject)PrefabUtility.InstantiatePrefab(bp, go.transform);
            b.transform.localPosition = pos[i]; s.blocs[i] = b.GetComponent<Demolition_Block>();
        }
        PrefabUtility.SaveAsPrefabAsset(go, path + "/" + name + ".prefab"); DestroyImmediate(go);
    }
}