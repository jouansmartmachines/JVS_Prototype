using UnityEngine;
using UnityEditor;
using System.IO;
using Demolition;

public class Demolition_SetupEditor : EditorWindow
{
    private static Material _matBois, _matVerre, _matPierre;

    [MenuItem("Tools/Demolition - Generer les prefabs")]
    static void GeneratePrefabs()
    {
        string basePath = "Assets/Projects/Demolition";
        string prefabPath = basePath + "/Demolition_Prefabs";

        // 1. Creer les materiaux colores
        CreateMaterials(basePath);

        // 2. Creer les sprites proceduraux
        Sprite boisSprite = CreateColoredSprite(64, 32, new Color(0.545f, 0.353f, 0.169f), "bois");
        Sprite verreSprite = CreateColoredSprite(64, 32, new Color(0.678f, 0.847f, 0.902f, 0.7f), "verre");
        Sprite pierreSprite = CreateColoredSprite(64, 32, new Color(0.5f, 0.5f, 0.5f), "pierre");
        Sprite oiseauSprite = CreateColoredSprite(32, 32, new Color(0.863f, 0.196f, 0.196f), "oiseau");
        Sprite impactSprite = CreateColoredSprite(64, 64, new Color(1f, 0.647f, 0f), "impact");
        Sprite debrisBoisSprite = CreateColoredSprite(16, 8, new Color(0.545f, 0.353f, 0.169f), "debris_bois");
        Sprite debrisVerreSprite = CreateColoredSprite(8, 8, new Color(0.678f, 0.847f, 0.902f), "debris_verre");
        Sprite debrisPierreSprite = CreateColoredSprite(12, 12, new Color(0.5f, 0.5f, 0.5f), "debris_pierre");

        // 3. Blocs avec materiaux
        CreateBloc(prefabPath, "Bloc_Bois", boisSprite, _matBois, Demolition_Block.MaterialType.Bois, 2, 50);
        CreateBloc(prefabPath, "Bloc_Verre", verreSprite, _matVerre, Demolition_Block.MaterialType.Verre, 1, 100);
        CreateBloc(prefabPath, "Bloc_Pierre", pierreSprite, _matPierre, Demolition_Block.MaterialType.Pierre, 4, 150);

        // 4. Debris
        GameObject debrisBois = CreateDebris(prefabPath, "Debris_Bois", debrisBoisSprite);
        GameObject debrisVerre = CreateDebris(prefabPath, "Debris_Verre", debrisVerreSprite);
        GameObject debrisPierre = CreateDebris(prefabPath, "Debris_Pierre", debrisPierreSprite);

        // Lier debris aux blocs
        LinkDebrisToBlock(prefabPath + "/Bloc_Bois.prefab", debrisBois);
        LinkDebrisToBlock(prefabPath + "/Bloc_Verre.prefab", debrisVerre);
        LinkDebrisToBlock(prefabPath + "/Bloc_Pierre.prefab", debrisPierre);

        // 5. Oiseau + explosion
        CreateOiseau(prefabPath, oiseauSprite, impactSprite);
        CreateImpact(prefabPath, impactSprite);

        // 6. Sons (WAV)
        CreateSounds(basePath + "/Resources/Sounds");

        // 7. Structures
        CreateStructure(prefabPath, "Structure_Exemple",
            new[] { "Bloc_Bois", "Bloc_Bois", "Bloc_Bois" },
            new Vector3[] { new(0, 0, 0), new(0.7f, 0.35f, 0), new(1.4f, 0.7f, 0) });

        CreateStructure(prefabPath, "Tableau_1",
            new[] { "Bloc_Bois", "Bloc_Verre", "Bloc_Bois", "Bloc_Pierre" },
            new Vector3[] { new(0, 0, 0), new(0.7f, 0.35f, 0), new(1.4f, 0, 0), new(2.1f, 0.35f, 0) });

        CreateStructure(prefabPath, "Tableau_2",
            new[] { "Bloc_Pierre", "Bloc_Bois", "Bloc_Verre", "Bloc_Verre" },
            new Vector3[] { new(0, 0, 0), new(0.7f, 0.7f, 0), new(1.4f, 0, 0), new(2.1f, 0, 0) });

        CreateStructure(prefabPath, "Tableau_3",
            new[] { "Bloc_Verre", "Bloc_Bois", "Bloc_Pierre", "Bloc_Bois" },
            new Vector3[] { new(0, 0, 0), new(0.7f, 0, 0), new(1.4f, 0.7f, 0), new(0.7f, 0.7f, 0) });

        // 8. Assigner references dans GameScene
        AssignGameSceneReferences(prefabPath, basePath);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("OK - Prefabs Demolition generes !");
    }

    static void CreateMaterials(string basePath)
    {
        string matPath = basePath + "/Resources/Materials";
        Directory.CreateDirectory(matPath);

        _matBois = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        _matBois.color = new Color(0.545f, 0.353f, 0.169f);
        AssetDatabase.CreateAsset(_matBois, matPath + "/Bois.mat");

        _matVerre = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        _matVerre.color = new Color(0.678f, 0.847f, 0.902f, 0.5f);
        _matVerre.SetFloat("_Surface", 1); // Transparent
        AssetDatabase.CreateAsset(_matVerre, matPath + "/Verre.mat");

        _matPierre = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        _matPierre.color = new Color(0.5f, 0.5f, 0.5f);
        AssetDatabase.CreateAsset(_matPierre, matPath + "/Pierre.mat");
    }

    static Sprite CreateColoredSprite(int w, int h, Color color, string name)
    {
        Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
                tex.SetPixel(x, y, color);
        tex.Apply();
        tex.filterMode = FilterMode.Point;
        Sprite sprite = Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 100);
        sprite.name = name;
        return sprite;
    }

    static void CreateSounds(string path)
    {
        Directory.CreateDirectory(path);
        if (!File.Exists(path + "/impact.wav"))
            CreateSineWave(path + "/impact.wav", 440, 0.15f, 0.8f);
        if (!File.Exists(path + "/destruction.wav"))
            CreateSineWave(path + "/destruction.wav", 220, 0.3f, 0.7f);
        if (!File.Exists(path + "/gameover.wav"))
            CreateSineWave(path + "/gameover.wav", 180, 0.5f, 0.6f);
        AssetDatabase.Refresh();
    }

    static void CreateSineWave(string path, float freq, float duration, float volume)
    {
        int sampleRate = 44100;
        int samples = (int)(sampleRate * duration);
        float[] data = new float[samples];
        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / sampleRate;
            float env = 1f - (t / duration);
            data[i] = Mathf.Sin(2 * Mathf.PI * freq * t) * volume * env * env;
        }
        int byteRate = sampleRate * 2;
        short blockAlign = 2;
        int dataSize = samples * 2;
        using (var fs = new FileStream(path, FileMode.Create))
        using (var bw = new BinaryWriter(fs))
        {
            bw.Write(new char[] { 'R', 'I', 'F', 'F' });
            bw.Write(36 + dataSize);
            bw.Write(new char[] { 'W', 'A', 'V', 'E', 'f', 'm', 't', ' ' });
            bw.Write(16);
            bw.Write((short)1);
            bw.Write((short)1);
            bw.Write(sampleRate);
            bw.Write(byteRate);
            bw.Write(blockAlign);
            bw.Write((short)16);
            bw.Write(new char[] { 'd', 'a', 't', 'a' });
            bw.Write(dataSize);
            foreach (float sample in data)
            {
                short val = (short)(Mathf.Clamp(sample, -1f, 1f) * short.MaxValue);
                bw.Write(val);
            }
        }
    }

    static GameObject CreateBloc(string path, string name, Sprite sprite, Material mat, Demolition_Block.MaterialType matType, int hp, int points)
    {
        GameObject go = new GameObject(name, typeof(SpriteRenderer), typeof(BoxCollider2D), typeof(Rigidbody2D), typeof(Demolition_Block), typeof(AudioSource));
        var sr = go.GetComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.material = mat;
        sr.sortingOrder = 1;
        var rb = go.GetComponent<Rigidbody2D>();
        rb.gravityScale = 1;
        rb.mass = 1;
        rb.linearDamping = 0.5f;
        var block = go.GetComponent<Demolition_Block>();
        block.hp = hp;
        block.points = points;
        block.materialType = matType;
        block.spriteRenderer = sr;
        block.damageSprites = new Sprite[] { sprite, sprite, sprite };
        block.debrisForce = 200f;
        string fullPath = path + "/" + name + ".prefab";
        PrefabUtility.SaveAsPrefabAsset(go, fullPath);
        Object.DestroyImmediate(go);
        return AssetDatabase.LoadAssetAtPath<GameObject>(fullPath);
    }

    static GameObject CreateDebris(string path, string name, Sprite sprite)
    {
        GameObject go = new GameObject(name, typeof(SpriteRenderer), typeof(Rigidbody2D), typeof(BoxCollider2D));
        go.GetComponent<SpriteRenderer>().sprite = sprite;
        go.GetComponent<SpriteRenderer>().sortingOrder = 2;
        var rb = go.GetComponent<Rigidbody2D>();
        rb.gravityScale = 1;
        rb.mass = 0.2f;
        string fullPath = path + "/" + name + ".prefab";
        PrefabUtility.SaveAsPrefabAsset(go, fullPath);
        Object.DestroyImmediate(go);
        return AssetDatabase.LoadAssetAtPath<GameObject>(fullPath);
    }

    static void LinkDebrisToBlock(string blockPath, GameObject debris)
    {
        GameObject block = AssetDatabase.LoadAssetAtPath<GameObject>(blockPath);
        if (block == null) return;
        var bc = block.GetComponent<Demolition_Block>();
        if (bc != null) { bc.debrisPrefab = debris; PrefabUtility.SavePrefabAsset(block); }
    }

    static void CreateOiseau(string path, Sprite oiseauSprite, Sprite impactSprite)
    {
        GameObject impact = new GameObject("ImpactExplosion", typeof(SpriteRenderer));
        impact.GetComponent<SpriteRenderer>().sprite = impactSprite;
        impact.GetComponent<SpriteRenderer>().sortingOrder = 4;
        string ipath = path + "/ImpactExplosion.prefab";
        PrefabUtility.SaveAsPrefabAsset(impact, ipath);
        Object.DestroyImmediate(impact);

        GameObject go = new GameObject("Oiseau", typeof(SpriteRenderer), typeof(Demolition_Projectile));
        go.GetComponent<SpriteRenderer>().sprite = oiseauSprite;
        go.GetComponent<SpriteRenderer>().sortingOrder = 3;
        var proj = go.GetComponent<Demolition_Projectile>();
        proj.oiseauDos = oiseauSprite;
        proj.spriteRenderer = go.GetComponent<SpriteRenderer>();
        proj.vitesseDepart = 5f;
        proj.acceleration = 2f;
        proj.scaleMin = 0.1f;
        proj.scaleMax = 1f;
        proj.forceExplosion = 500f;
        proj.radiusExplosion = 2f;
        proj.explosionPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(ipath);
        PrefabUtility.SaveAsPrefabAsset(go, path + "/Oiseau.prefab");
        Object.DestroyImmediate(go);
    }

    static void CreateImpact(string path, Sprite impactSprite)
    {
        GameObject go = new GameObject("ImpactExplosion", typeof(SpriteRenderer));
        go.GetComponent<SpriteRenderer>().sprite = impactSprite;
        go.GetComponent<SpriteRenderer>().sortingOrder = 4;
        PrefabUtility.SaveAsPrefabAsset(go, path + "/ImpactExplosion.prefab");
        Object.DestroyImmediate(go);
    }

    static void CreateStructure(string path, string name, string[] blockNames, Vector3[] positions)
    {
        GameObject go = new GameObject(name, typeof(Demolition_Structure));
        var structure = go.GetComponent<Demolition_Structure>();
        structure.blocs = new Demolition_Block[blockNames.Length];
        for (int i = 0; i < blockNames.Length; i++)
        {
            GameObject bp = AssetDatabase.LoadAssetAtPath<GameObject>(path + "/" + blockNames[i] + ".prefab");
            if (bp == null) continue;
            GameObject b = (GameObject)PrefabUtility.InstantiatePrefab(bp, go.transform);
            b.transform.localPosition = positions[i];
            structure.blocs[i] = b.GetComponent<Demolition_Block>();
        }
        PrefabUtility.SaveAsPrefabAsset(go, path + "/" + name + ".prefab");
        Object.DestroyImmediate(go);
    }

    static void AssignGameSceneReferences(string prefabPath, string basePath)
    {
        string scenePath = "Assets/Projects/Demolition/Demolition_Scenes/GameScene_Demolition.unity";
        var scene = UnityEditor.SceneManagement.EditorSceneManager.OpenScene(scenePath);
        var gm = Object.FindFirstObjectByType<Demolition_GameManager>();
        if (gm == null) { Debug.LogError("GameManager not found!"); return; }

        gm.oiseauPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath + "/Oiseau.prefab");
        gm.impactEffectPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath + "/ImpactExplosion.prefab");
        gm.impactSound = AssetDatabase.LoadAssetAtPath<AudioClip>(basePath + "/Resources/Sounds/impact.wav");
        gm.destructionSound = AssetDatabase.LoadAssetAtPath<AudioClip>(basePath + "/Resources/Sounds/destruction.wav");
        gm.gameOverSound = AssetDatabase.LoadAssetAtPath<AudioClip>(basePath + "/Resources/Sounds/gameover.wav");
        gm.tableauPrefabs = new GameObject[] {
            AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath + "/Tableau_1.prefab"),
            AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath + "/Tableau_2.prefab"),
            AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath + "/Tableau_3.prefab"),
        };
        gm.structuresParent = GameObject.Find("StructuresParent")?.transform;

        UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene);
        Debug.Log("GameScene mise a jour !");
    }
}