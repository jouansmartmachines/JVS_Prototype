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
        string texPath = basePath + "/Textures";
        string soundPath = basePath + "/Sounds";

        Directory.CreateDirectory(texPath);
        Directory.CreateDirectory(soundPath);
        Directory.CreateDirectory(prefabPath);

        // 1. Creer les textures PNG
        AssetDatabase.StartAssetEditing();
        CreatePNG(texPath + "/bois.png", 64, 32, new Color(0.545f, 0.353f, 0.169f));
        CreatePNG(texPath + "/verre.png", 64, 32, new Color(0.678f, 0.847f, 0.902f, 0.7f));
        CreatePNG(texPath + "/pierre.png", 64, 32, new Color(0.5f, 0.5f, 0.5f));
        CreatePNG(texPath + "/oiseau.png", 32, 32, new Color(0.863f, 0.196f, 0.196f));
        CreatePNG(texPath + "/impact.png", 64, 64, new Color(1f, 0.647f, 0f));
        CreatePNG(texPath + "/debris_bois.png", 16, 8, new Color(0.545f, 0.353f, 0.169f));
        CreatePNG(texPath + "/debris_verre.png", 8, 8, new Color(0.678f, 0.847f, 0.902f));
        CreatePNG(texPath + "/debris_pierre.png", 12, 12, new Color(0.5f, 0.5f, 0.5f));
        AssetDatabase.StopAssetEditing();
        AssetDatabase.Refresh();

        // 2. Sons
        CreateWAV(soundPath + "/impact.wav", 440, 0.15f, 0.8f);
        CreateWAV(soundPath + "/destruction.wav", 220, 0.3f, 0.7f);
        CreateWAV(soundPath + "/gameover.wav", 180, 0.5f, 0.6f);
        AssetDatabase.Refresh();

        // 3. Charger les sprites fraichement importes
        Sprite boisSprite = AssetDatabase.LoadAssetAtPath<Sprite>(texPath + "/bois.png");
        Sprite verreSprite = AssetDatabase.LoadAssetAtPath<Sprite>(texPath + "/verre.png");
        Sprite pierreSprite = AssetDatabase.LoadAssetAtPath<Sprite>(texPath + "/pierre.png");
        Sprite oiseauSprite = AssetDatabase.LoadAssetAtPath<Sprite>(texPath + "/oiseau.png");
        Sprite impactSprite = AssetDatabase.LoadAssetAtPath<Sprite>(texPath + "/impact.png");
        Sprite debrisBoisSprite = AssetDatabase.LoadAssetAtPath<Sprite>(texPath + "/debris_bois.png");
        Sprite debrisVerreSprite = AssetDatabase.LoadAssetAtPath<Sprite>(texPath + "/debris_verre.png");
        Sprite debrisPierreSprite = AssetDatabase.LoadAssetAtPath<Sprite>(texPath + "/debris_pierre.png");

        if (boisSprite == null) { Debug.LogError("Sprites non trouves! Reessaie."); return; }

        // 4. Blocs
        GameObject blocBois = CreateBloc(prefabPath, "Bloc_Bois", boisSprite, Demolition_Block.MaterialType.Bois, 2, 50);
        GameObject blocVerre = CreateBloc(prefabPath, "Bloc_Verre", verreSprite, Demolition_Block.MaterialType.Verre, 1, 100);
        GameObject blocPierre = CreateBloc(prefabPath, "Bloc_Pierre", pierreSprite, Demolition_Block.MaterialType.Pierre, 4, 150);

        // 5. Debris
        GameObject dBois = CreateDebris(prefabPath, "Debris_Bois", debrisBoisSprite);
        GameObject dVerre = CreateDebris(prefabPath, "Debris_Verre", debrisVerreSprite);
        GameObject dPierre = CreateDebris(prefabPath, "Debris_Pierre", debrisPierreSprite);

        // Lier debris -> blocs
        LinkDebris(blocBois, dBois);
        LinkDebris(blocVerre, dVerre);
        LinkDebris(blocPierre, dPierre);

        // 6. Oiseau + Explosion
        CreateOiseau(prefabPath, oiseauSprite, impactSprite);
        CreateImpact(prefabPath, impactSprite);

        // 7. Structures
        CreateStruct(prefabPath, "Structure_Exemple",
            new[] { "Bloc_Bois", "Bloc_Bois", "Bloc_Bois" },
            new Vector3[] { new(0, 0, 0), new(0.7f, 0.35f, 0), new(1.4f, 0.7f, 0) });
        CreateStruct(prefabPath, "Tableau_1",
            new[] { "Bloc_Bois", "Bloc_Verre", "Bloc_Bois", "Bloc_Pierre" },
            new Vector3[] { new(0, 0, 0), new(0.7f, 0.35f, 0), new(1.4f, 0, 0), new(2.1f, 0.35f, 0) });
        CreateStruct(prefabPath, "Tableau_2",
            new[] { "Bloc_Pierre", "Bloc_Bois", "Bloc_Verre", "Bloc_Verre" },
            new Vector3[] { new(0, 0, 0), new(0.7f, 0.7f, 0), new(1.4f, 0, 0), new(2.1f, 0, 0) });
        CreateStruct(prefabPath, "Tableau_3",
            new[] { "Bloc_Verre", "Bloc_Bois", "Bloc_Pierre", "Bloc_Bois" },
            new Vector3[] { new(0, 0, 0), new(0.7f, 0, 0), new(1.4f, 0.7f, 0), new(0.7f, 0.7f, 0) });

        // 8. Ajouter GeneralVariable a la GameScene + assigner references
        SetupGameScene(prefabPath, soundPath);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("OK - Prefabs Demolition generes !");
    }

    static void CreatePNG(string path, int w, int h, Color color)
    {
        if (File.Exists(path)) return;
        Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
                tex.SetPixel(x, y, color);
        tex.Apply();
        File.WriteAllBytes(path, tex.EncodeToPNG());
        DestroyImmediate(tex);
    }

    static void CreateWAV(string path, float freq, float dur, float vol)
    {
        if (File.Exists(path)) return;
        int sr = 44100;
        int samples = (int)(sr * dur);
        float[] data = new float[samples];
        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / sr;
            float env = 1f - (t / dur);
            data[i] = Mathf.Sin(2 * Mathf.PI * freq * t) * vol * env * env;
        }
        using (var fs = new FileStream(path, FileMode.Create))
        using (var bw = new BinaryWriter(fs))
        {
            bw.Write(new char[] { 'R', 'I', 'F', 'F' });
            bw.Write(36 + samples * 2);
            bw.Write(new char[] { 'W', 'A', 'V', 'E', 'f', 'm', 't', ' ' });
            bw.Write(16); bw.Write((short)1); bw.Write((short)1);
            bw.Write(sr); bw.Write(sr * 2); bw.Write((short)2); bw.Write((short)16);
            bw.Write(new char[] { 'd', 'a', 't', 'a' });
            bw.Write(samples * 2);
            foreach (float s in data)
                bw.Write((short)(Mathf.Clamp(s, -1f, 1f) * 32767));
        }
    }

    static GameObject CreateBloc(string path, string name, Sprite sprite, Demolition_Block.MaterialType mat, int hp, int pts)
    {
        GameObject go = new GameObject(name, typeof(SpriteRenderer), typeof(BoxCollider2D), typeof(Rigidbody2D), typeof(Demolition_Block), typeof(AudioSource));
        var sr = go.GetComponent<SpriteRenderer>();
        sr.sprite = sprite; sr.sortingOrder = 1;
        var rb = go.GetComponent<Rigidbody2D>();
        rb.gravityScale = 1; rb.mass = 1; rb.linearDamping = 0.5f;
        var b = go.GetComponent<Demolition_Block>();
        b.hp = hp; b.points = pts; b.materialType = mat;
        b.spriteRenderer = sr;
        b.damageSprites = new Sprite[] { sprite, sprite, sprite };
        b.debrisForce = 200f;
        string fp = path + "/" + name + ".prefab";
        PrefabUtility.SaveAsPrefabAsset(go, fp);
        DestroyImmediate(go);
        return AssetDatabase.LoadAssetAtPath<GameObject>(fp);
    }

    static GameObject CreateDebris(string path, string name, Sprite sprite)
    {
        GameObject go = new GameObject(name, typeof(SpriteRenderer), typeof(Rigidbody2D), typeof(BoxCollider2D));
        go.GetComponent<SpriteRenderer>().sprite = sprite; go.GetComponent<SpriteRenderer>().sortingOrder = 2;
        go.GetComponent<Rigidbody2D>().gravityScale = 1; go.GetComponent<Rigidbody2D>().mass = 0.2f;
        string fp = path + "/" + name + ".prefab";
        PrefabUtility.SaveAsPrefabAsset(go, fp);
        DestroyImmediate(go);
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
        string ip = path + "/ImpactExplosion.prefab";
        PrefabUtility.SaveAsPrefabAsset(imp, ip); DestroyImmediate(imp);

        GameObject go = new GameObject("Oiseau", typeof(SpriteRenderer), typeof(Demolition_Projectile));
        go.GetComponent<SpriteRenderer>().sprite = oiSprite; go.GetComponent<SpriteRenderer>().sortingOrder = 3;
        var p = go.GetComponent<Demolition_Projectile>();
        p.oiseauDos = oiSprite; p.spriteRenderer = go.GetComponent<SpriteRenderer>();
        p.vitesseDepart = 5; p.acceleration = 2; p.scaleMin = 0.1f; p.scaleMax = 1;
        p.forceExplosion = 500; p.radiusExplosion = 2;
        p.explosionPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(ip);
        PrefabUtility.SaveAsPrefabAsset(go, path + "/Oiseau.prefab");
        DestroyImmediate(go);
    }

    static void CreateImpact(string path, Sprite sprite)
    {
        GameObject go = new GameObject("ImpactExplosion", typeof(SpriteRenderer));
        go.GetComponent<SpriteRenderer>().sprite = sprite; go.GetComponent<SpriteRenderer>().sortingOrder = 4;
        PrefabUtility.SaveAsPrefabAsset(go, path + "/ImpactExplosion.prefab");
        DestroyImmediate(go);
    }

    static void CreateStruct(string path, string name, string[] blocks, Vector3[] pos)
    {
        GameObject go = new GameObject(name, typeof(Demolition_Structure));
        var s = go.GetComponent<Demolition_Structure>();
        s.blocs = new Demolition_Block[blocks.Length];
        for (int i = 0; i < blocks.Length; i++)
        {
            GameObject bp = AssetDatabase.LoadAssetAtPath<GameObject>(path + "/" + blocks[i] + ".prefab");
            if (bp == null) continue;
            GameObject b = (GameObject)PrefabUtility.InstantiatePrefab(bp, go.transform);
            b.transform.localPosition = pos[i];
            s.blocs[i] = b.GetComponent<Demolition_Block>();
        }
        PrefabUtility.SaveAsPrefabAsset(go, path + "/" + name + ".prefab");
        DestroyImmediate(go);
    }

    static void SetupGameScene(string prefabPath, string soundPath)
    {
        string scenePath = "Assets/Projects/Demolition/Demolition_Scenes/GameScene_Demolition.unity";
        var scene = UnityEditor.SceneManagement.EditorSceneManager.OpenScene(scenePath);

        // Ajouter GeneralVariable prefab instance
        GameObject gvPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath + "/GeneralVariable.prefab");
        if (gvPrefab != null)
        {
            var existing = GameObject.Find("GeneralVariable");
            if (existing == null)
            {
                var gv = (GameObject)PrefabUtility.InstantiatePrefab(gvPrefab);
                gv.name = "GeneralVariable";
                gv.GetComponent<Demolition_GeneralVariables>().gameName = "Demolition";
                Debug.Log("GeneralVariable ajoute a la scene");
            }
        }

        var gm = Object.FindFirstObjectByType<Demolition_GameManager>();
        if (gm == null) { Debug.LogError("GameManager pas trouve!"); return; }

        gm.oiseauPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath + "/Oiseau.prefab");
        gm.impactEffectPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath + "/ImpactExplosion.prefab");
        gm.impactSound = AssetDatabase.LoadAssetAtPath<AudioClip>(soundPath + "/impact.wav");
        gm.destructionSound = AssetDatabase.LoadAssetAtPath<AudioClip>(soundPath + "/destruction.wav");
        gm.gameOverSound = AssetDatabase.LoadAssetAtPath<AudioClip>(soundPath + "/gameover.wav");
        gm.tableauPrefabs = new GameObject[] {
            AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath + "/Tableau_1.prefab"),
            AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath + "/Tableau_2.prefab"),
            AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath + "/Tableau_3.prefab"),
        };
        gm.structuresParent = GameObject.Find("StructuresParent")?.transform;

        UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene);
        Debug.Log("GameScene mise a jour avec toutes les references!");
    }
}