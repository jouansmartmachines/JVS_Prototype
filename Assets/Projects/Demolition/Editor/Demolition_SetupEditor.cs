using UnityEngine;
using UnityEditor;
using System.IO;

public class Demolition_SetupEditor : EditorWindow
{
    [MenuItem("Tools/Demolition - Generer les prefabs")]
    static void GeneratePrefabs()
    {
        string basePath = "Assets/Projects/Demolition";
        string prefabPath = basePath + "/Demolition_Prefabs";
        string spritePath = basePath + "/Resources/Sprites";

        Sprite boisSprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath + "/bois.png");
        Sprite verreSprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath + "/verre.png");
        Sprite pierreSprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath + "/pierre.png");
        Sprite oiseauSprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath + "/oiseau.png");
        Sprite impactSprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath + "/impact.png");

        // 1. Blocs
        CreateBloc(prefabPath, "Bloc_Bois", boisSprite, Demolition_Block.MaterialType.Bois, 2, 50);
        CreateBloc(prefabPath, "Bloc_Verre", verreSprite, Demolition_Block.MaterialType.Verre, 1, 100);
        CreateBloc(prefabPath, "Bloc_Pierre", pierreSprite, Demolition_Block.MaterialType.Pierre, 4, 150);

        // 2. Debris
        GameObject debrisBois = CreateDebris(prefabPath, "Debris_Bois", spritePath + "/debris_bois.png");
        GameObject debrisVerre = CreateDebris(prefabPath, "Debris_Verre", spritePath + "/debris_verre.png");
        GameObject debrisPierre = CreateDebris(prefabPath, "Debris_Pierre", spritePath + "/debris_pierre.png");

        // 3. Lier debris aux blocs
        LinkDebrisToBlock(prefabPath + "/Bloc_Bois.prefab", debrisBois);
        LinkDebrisToBlock(prefabPath + "/Bloc_Verre.prefab", debrisVerre);
        LinkDebrisToBlock(prefabPath + "/Bloc_Pierre.prefab", debrisPierre);

        // 4. Oiseau + ImpactExplosion
        CreateOiseau(prefabPath, oiseauSprite, impactSprite);
        CreateImpact(prefabPath, impactSprite);

        // 5. Structures
        string[] blocks1 = { "Bloc_Bois", "Bloc_Bois", "Bloc_Bois" };
        Vector3[] pos1 = { new Vector3(0, 0, 0), new Vector3(0.7f, 0.35f, 0), new Vector3(1.4f, 0.7f, 0) };
        CreateStructure(prefabPath, "Structure_Exemple", blocks1, pos1);

        string[] blocks2 = { "Bloc_Bois", "Bloc_Verre", "Bloc_Bois", "Bloc_Pierre" };
        Vector3[] pos2 = { new Vector3(0, 0, 0), new Vector3(0.7f, 0.35f, 0), new Vector3(1.4f, 0, 0), new Vector3(2.1f, 0.35f, 0) };
        CreateStructure(prefabPath, "Tableau_1", blocks2, pos2);

        string[] blocks3 = { "Bloc_Pierre", "Bloc_Bois", "Bloc_Verre", "Bloc_Verre" };
        Vector3[] pos3 = { new Vector3(0, 0, 0), new Vector3(0.7f, 0.7f, 0), new Vector3(1.4f, 0, 0), new Vector3(2.1f, 0, 0) };
        CreateStructure(prefabPath, "Tableau_2", blocks3, pos3);

        string[] blocks4 = { "Bloc_Verre", "Bloc_Bois", "Bloc_Pierre", "Bloc_Bois" };
        Vector3[] pos4 = { new Vector3(0, 0, 0), new Vector3(0.7f, 0, 0), new Vector3(1.4f, 0.7f, 0), new Vector3(0.7f, 0.7f, 0) };
        CreateStructure(prefabPath, "Tableau_3", blocks4, pos4);

        // 6. Assigner dans la GameScene
        AssignGameSceneReferences(prefabPath);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("\n\u2705 Prefabs Demolition generes ! Ouvre GameScene_Demolition.");
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
        Debug.Log("Cree: " + fullPath);
        return AssetDatabase.LoadAssetAtPath<GameObject>(fullPath);
    }

    static void LinkDebrisToBlock(string blockPath, GameObject debris)
    {
        GameObject block = AssetDatabase.LoadAssetAtPath<GameObject>(blockPath);
        if (block == null) { Debug.LogError("Block not found: " + blockPath); return; }
        var blockComponent = block.GetComponent<Demolition_Block>();
        if (blockComponent != null)
        {
            blockComponent.debrisPrefab = debris;
            PrefabUtility.SavePrefabAsset(block);
        }
    }

    static void CreateOiseau(string path, Sprite oiseauSprite, Sprite impactSprite)
    {
        // Creer d'abord l'explosion
        GameObject impact = new GameObject("ImpactExplosion", typeof(SpriteRenderer));
        impact.GetComponent<SpriteRenderer>().sprite = impactSprite;
        impact.GetComponent<SpriteRenderer>().sortingOrder = 4;
        string impactPath = path + "/ImpactExplosion.prefab";
        PrefabUtility.SaveAsPrefabAsset(impact, impactPath);
        Object.DestroyImmediate(impact);

        // Creer l'oiseau
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
        Debug.Log("Cree: Oiseau.prefab");
    }

    static void CreateImpact(string path, Sprite impactSprite)
    {
        GameObject go = new GameObject("ImpactExplosion", typeof(SpriteRenderer));
        go.GetComponent<SpriteRenderer>().sprite = impactSprite;
        go.GetComponent<SpriteRenderer>().sortingOrder = 4;
        string fullPath = path + "/ImpactExplosion.prefab";
        PrefabUtility.SaveAsPrefabAsset(go, fullPath);
        Object.DestroyImmediate(go);
        Debug.Log("Cree: " + fullPath);
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
        string fullPath = path + "/" + name + ".prefab";
        PrefabUtility.SaveAsPrefabAsset(go, fullPath);
        Object.DestroyImmediate(go);
        Debug.Log("Cree: " + fullPath);
    }

    static void AssignGameSceneReferences(string prefabPath)
    {
        string scenePath = "Assets/Projects/Demolition/Demolition_Scenes/GameScene_Demolition.unity";
        var scene = UnityEditor.SceneManagement.EditorSceneManager.OpenScene(scenePath);
        var gameManager = Object.FindFirstObjectByType<Demolition_GameManager>();
        if (gameManager == null) { Debug.LogError("GameManager not found!"); return; }

        gameManager.oiseauPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath + "/Oiseau.prefab");
        gameManager.impactEffectPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath + "/ImpactExplosion.prefab");
        gameManager.tableauPrefabs = new GameObject[] {
            AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath + "/Tableau_1.prefab"),
            AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath + "/Tableau_2.prefab"),
            AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath + "/Tableau_3.prefab"),
        };
        gameManager.structuresParent = GameObject.Find("StructuresParent")?.transform;
        UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene);
        Debug.Log("GameScene references updated");
    }
}
