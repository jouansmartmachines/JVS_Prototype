using UnityEngine;
using UnityEditor;
using System.IO;
using Demolition;

public class Demolition_SetupEditor : EditorWindow
{
    [MenuItem("Tools/Demolition - Generer les prefabs")]
    static void GeneratePrefabs()
    {
        string basePath = "Assets/Projects/Demolition";
        string prefabPath = basePath + "/Demolition_Prefabs";
        string spritePath = basePath + "/Resources/Sprites";

        // 1. Generer les textures (si pas deja faites)
        CreateTextures(spritePath);

        AssetDatabase.Refresh();

        // 2. Charger les sprites
        Sprite boisSprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath + "/bois.png");
        Sprite verreSprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath + "/verre.png");
        Sprite pierreSprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath + "/pierre.png");
        Sprite oiseauSprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath + "/oiseau.png");
        Sprite impactSprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath + "/impact.png");

        // 3. Blocs
        CreateBloc(prefabPath, "Bloc_Bois", boisSprite, Demolition_Block.MaterialType.Bois, 2, 50);
        CreateBloc(prefabPath, "Bloc_Verre", verreSprite, Demolition_Block.MaterialType.Verre, 1, 100);
        CreateBloc(prefabPath, "Bloc_Pierre", pierreSprite, Demolition_Block.MaterialType.Pierre, 4, 150);

        // 4. Debris
        GameObject debrisBois = CreateDebris(prefabPath, "Debris_Bois", spritePath + "/debris_bois.png");
        GameObject debrisVerre = CreateDebris(prefabPath, "Debris_Verre", spritePath + "/debris_verre.png");
        GameObject debrisPierre = CreateDebris(prefabPath, "Debris_Pierre", spritePath + "/debris_pierre.png");

        LinkDebrisToBlock(prefabPath + "/Bloc_Bois.prefab", debrisBois);
        LinkDebrisToBlock(prefabPath + "/Bloc_Verre.prefab", debrisVerre);
        LinkDebrisToBlock(prefabPath + "/Bloc_Pierre.prefab", debrisPierre);

        // 5. Oiseau + ImpactExplosion
        CreateOiseau(prefabPath, oiseauSprite, impactSprite);
        CreateImpact(prefabPath, impactSprite);

        // 6. Sons
        CreateSounds(basePath + "/Resources");

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

        // 8. Assigner dans la GameScene
        AssignGameSceneReferences(prefabPath, basePath);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("\u2705 Prefabs Demolition generes ! Ouvre GameScene_Demolition.");
    }

    static void CreateTextures(string spritePath)
    {
        Directory.CreateDirectory(spritePath);
        
        // Creer textures si elles n'existent pas
        if (!File.Exists(spritePath + "/bois.png"))
            CreatePlaceholderTexture(spritePath + "/bois.png", 64, 32, new byte[] { 139, 90, 43 });
        if (!File.Exists(spritePath + "/verre.png"))
            CreatePlaceholderTexture(spritePath + "/verre.png", 64, 32, new byte[] { 173, 216, 230 });
        if (!File.Exists(spritePath + "/pierre.png"))
            CreatePlaceholderTexture(spritePath + "/pierre.png", 64, 32, new byte[] { 128, 128, 128 });
        if (!File.Exists(spritePath + "/oiseau.png"))
            CreatePlaceholderTexture(spritePath + "/oiseau.png", 32, 32, new byte[] { 255, 50, 50 });
        if (!File.Exists(spritePath + "/impact.png"))
            CreatePlaceholderTexture(spritePath + "/impact.png", 64, 64, new byte[] { 255, 165, 0 });
        if (!File.Exists(spritePath + "/debris_bois.png"))
            CreatePlaceholderTexture(spritePath + "/debris_bois.png", 16, 8, new byte[] { 139, 90, 43 });
        if (!File.Exists(spritePath + "/debris_verre.png"))
            CreatePlaceholderTexture(spritePath + "/debris_verre.png", 8, 8, new byte[] { 173, 216, 230 });
        if (!File.Exists(spritePath + "/debris_pierre.png"))
            CreatePlaceholderTexture(spritePath + "/debris_pierre.png", 12, 12, new byte[] { 128, 128, 128 });
        if (!File.Exists(spritePath + "/particule.png"))
            CreatePlaceholderTexture(spritePath + "/particule.png", 4, 4, new byte[] { 255, 255, 255 });
    }

    static void CreatePlaceholderTexture(string path, int w, int h, byte[] color)
    {
        Texture2D tex = new Texture2D(w, h, TextureFormat.RGB24, false);
        Color c = new Color(color[0]/255f, color[1]/255f, color[2]/255f);
        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
                tex.SetPixel(x, y, c);
        tex.Apply();
        File.WriteAllBytes(path, tex.EncodeToPNG());
        UnityEngine.Object.DestroyImmediate(tex);
    }

    static void CreateSounds(string resPath)
    {
        string soundsPath = resPath + "/Sounds";
        Directory.CreateDirectory(soundsPath);

        if (!File.Exists(soundsPath + "/impact.wav"))
            CreateSineWave(soundsPath + "/impact.wav", 440, 0.15f, 0.8f);
        if (!File.Exists(soundsPath + "/destruction.wav"))
            CreateSineWave(soundsPath + "/destruction.wav", 220, 0.3f, 0.7f);
        if (!File.Exists(soundsPath + "/gameover.wav"))
            CreateSineWave(soundsPath + "/gameover.wav", 180, 0.5f, 0.6f);
    }

    static void CreateSineWave(string path, float freq, float duration, float volume)
    {
        int sampleRate = 44100;
        int samples = (int)(sampleRate * duration);
        float[] data = new float[samples];
        
        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / sampleRate;
            float env = 1f - (t / duration); // decay envelope
            data[i] = Mathf.Sin(2 * Mathf.PI * freq * t) * volume * env * env;
        }

        // Convert to 16-bit PCM WAV
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
            bw.Write((short)1); // PCM
            bw.Write((short)1); // mono
            bw.Write(sampleRate);
            bw.Write(byteRate);
            bw.Write(blockAlign);
            bw.Write((short)16); // bits per sample
            bw.Write(new char[] { 'd', 'a', 't', 'a' });
            bw.Write(dataSize);
            foreach (float sample in data)
            {
                short val = (short)(Mathf.Clamp(sample, -1f, 1f) * short.MaxValue);
                bw.Write(val);
            }
        }
    }

    static GameObject CreateBloc(string path, string name, Sprite sprite, Demolition_Block.MaterialType matType, int hp, int points)
    {
        GameObject go = new GameObject(name, typeof(SpriteRenderer), typeof(BoxCollider2D), typeof(Rigidbody2D), typeof(Demolition_Block), typeof(AudioSource));
        go.GetComponent<SpriteRenderer>().sprite = sprite;
        go.GetComponent<SpriteRenderer>().sortingOrder = 1;
        var rb = go.GetComponent<Rigidbody2D>();
        rb.gravityScale = 1;
        rb.mass = 1;
        rb.linearDamping = 0.5f;
        var block = go.GetComponent<Demolition_Block>();
        block.hp = hp;
        block.points = points;
        block.materialType = matType;
        block.spriteRenderer = go.GetComponent<SpriteRenderer>();
        block.damageSprites = new Sprite[] { sprite, sprite, sprite };
        block.debrisForce = 200f;
        string fullPath = path + "/" + name + ".prefab";
        PrefabUtility.SaveAsPrefabAsset(go, fullPath);
        Object.DestroyImmediate(go);
        Debug.Log("Cree: " + fullPath);
        return AssetDatabase.LoadAssetAtPath<GameObject>(fullPath);
    }

    static GameObject CreateDebris(string path, string name, string spritePath)
    {
        Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
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
        if (block == null) { return; }
        var blockComponent = block.GetComponent<Demolition_Block>();
        if (blockComponent != null)
        {
            blockComponent.debrisPrefab = debris;
            PrefabUtility.SavePrefabAsset(block);
        }
    }

    static void CreateOiseau(string path, Sprite oiseauSprite, Sprite impactSprite)
    {
        // Impact d'abord
        GameObject impact = new GameObject("ImpactExplosion", typeof(SpriteRenderer));
        impact.GetComponent<SpriteRenderer>().sprite = impactSprite;
        impact.GetComponent<SpriteRenderer>().sortingOrder = 4;
        string impactPath = path + "/ImpactExplosion.prefab";
        PrefabUtility.SaveAsPrefabAsset(impact, impactPath);
        Object.DestroyImmediate(impact);

        // Oiseau
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
        proj.explosionPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(impactPath);
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
            GameObject blockPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(path + "/" + blockNames[i] + ".prefab");
            if (blockPrefab == null) continue;
            GameObject block = (GameObject)PrefabUtility.InstantiatePrefab(blockPrefab, go.transform);
            block.transform.localPosition = positions[i];
            structure.blocs[i] = block.GetComponent<Demolition_Block>();
        }
        PrefabUtility.SaveAsPrefabAsset(go, path + "/" + name + ".prefab");
        Object.DestroyImmediate(go);
    }

    static void AssignGameSceneReferences(string prefabPath, string basePath)
    {
        string scenePath = "Assets/Projects/Demolition/Demolition_Scenes/GameScene_Demolition.unity";
        var scene = UnityEditor.SceneManagement.EditorSceneManager.OpenScene(scenePath);
        var gameManager = Object.FindFirstObjectByType<Demolition_GameManager>();
        if (gameManager == null) { Debug.LogError("GameManager not found!"); return; }

        gameManager.oiseauPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath + "/Oiseau.prefab");
        gameManager.impactEffectPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath + "/ImpactExplosion.prefab");
        
        // Sons
        AudioClip impactClip = AssetDatabase.LoadAssetAtPath<AudioClip>(basePath + "/Resources/Sounds/impact.wav");
        AudioClip destructionClip = AssetDatabase.LoadAssetAtPath<AudioClip>(basePath + "/Resources/Sounds/destruction.wav");
        AudioClip gameoverClip = AssetDatabase.LoadAssetAtPath<AudioClip>(basePath + "/Resources/Sounds/gameover.wav");
        
        gameManager.impactSound = impactClip;
        gameManager.destructionSound = destructionClip;
        gameManager.gameOverSound = gameoverClip;

        gameManager.tableauPrefabs = new GameObject[] {
            AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath + "/Tableau_1.prefab"),
            AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath + "/Tableau_2.prefab"),
            AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath + "/Tableau_3.prefab"),
        };
        gameManager.structuresParent = GameObject.Find("StructuresParent")?.transform;

        UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene);
        Debug.Log("GameScene references updated with sounds!");
    }
}