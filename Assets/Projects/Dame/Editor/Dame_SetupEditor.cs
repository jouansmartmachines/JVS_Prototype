using UnityEngine;
using UnityEditor;
using System.IO;
using UnityEngine.EventSystems;
using TMPro;
using UnityEditor.SceneManagement;

public class Dame_SetupEditor : EditorWindow
{
    static string _basePath, _prefabPath, _texPath, _soundPath;

    [MenuItem("Tools/Dame - Tout configurer")]
    static void ConfigurerTout()
    {
        if (EditorApplication.isPlaying)
        {
            Debug.LogError("Arretez le jeu avant de lancer l'outil !");
            return;
        }

        _basePath = "Assets/Projects/Dame";
        string resPath = _basePath + "/Resources";
        _prefabPath = resPath + "/Prefabs";
        _texPath = resPath + "/Textures";
        _soundPath = resPath + "/Sounds";

        Directory.CreateDirectory(_prefabPath);
        Directory.CreateDirectory(_texPath);
        Directory.CreateDirectory(_soundPath);

        Debug.Log("=== DEBUT configuration Dame ===");

        // 1. Sons (ne crée que s'ils n'existent pas)
        MakeWAV("move", 0.1f, 400, 0.3f);
        MakeWAV("capture", 0.2f, 600, 0.5f);
        MakeWAV("crown", 0.3f, 800, 0.4f);
        MakeWAV("win", 0.5f, 200, 0.8f);
        Debug.Log("✓ Sons OK");

        AssetDatabase.Refresh();
        Debug.Log("=== Dame configure ===");
        Debug.Log("4 scenes (Accueil/Menu/GameScene/Score) dans Dame_Scenes/");
        Debug.Log("Prefabs dans Dame_Prefabs/");
        Debug.Log("Presets dans Preset/");
        Debug.Log("ScenePrefab MENU_Dame.prefab dans ScenePrefabs/");
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
        Debug.Log($"✓ Son {name}.wav cree");
    }
}