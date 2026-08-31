using UnityEngine;
using UnityEditor;
using System.IO;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using UnityEditor.SceneManagement;
using Dame;

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

        // 1. Sons
        MakeWAV("move", 0.1f, 400, 0.3f);
        MakeWAV("capture", 0.2f, 600, 0.5f);
        MakeWAV("crown", 0.3f, 800, 0.4f);
        MakeWAV("win", 0.5f, 200, 0.8f);
        Debug.Log("1/5 Sons crees");

        AssetDatabase.Refresh();

        // 2. Creer les scenes
        CreateAccueilScene();
        Debug.Log("2/5 Scene Accueil creee");

        CreateMenuScene();
        Debug.Log("3/5 Scene Menu creee");

        CreateGameScene();
        Debug.Log("4/5 Scene Game creee");

        CreateScoreScene();
        Debug.Log("5/5 Scene Score creee");

        AssetDatabase.Refresh();
        Debug.Log("=== FINI: Dame completement configure ===");
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

    static Sprite LoadSprite(string name)
    {
        AssetDatabase.Refresh();
        return AssetDatabase.LoadAssetAtPath<Sprite>(_texPath + "/" + name + ".png");
    }

    static void CreateAccueilScene()
    {
        string scenePath = _basePath + "/Dame_Scenes/Accueil_Dame.unity";
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        EditorSceneManager.SaveScene(scene, scenePath);

        // Camera
        var camGO = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
        camGO.GetComponent<Camera>().orthographic = true;
        camGO.GetComponent<Camera>().orthographicSize = 5;
        camGO.transform.position = new Vector3(0, 0, -10);

        // Background
        var bg = new GameObject("Background", typeof(SpriteRenderer));
        var bgTex = LoadSprite("bg_accueil");
        if (bgTex != null) bg.GetComponent<SpriteRenderer>().sprite = bgTex;

        // PlayButton
        var playGO = new GameObject("PlayButton", typeof(SpriteRenderer), typeof(BoxCollider2D), typeof(Universal_PlayButton));
        playGO.transform.position = new Vector3(0, -1, 0);
        var playTex = LoadSprite("dame_blanche");
        if (playTex != null) playGO.GetComponent<SpriteRenderer>().sprite = playTex;
        playGO.GetComponent<SpriteRenderer>().sortingOrder = 2;
        var col = playGO.GetComponent<BoxCollider2D>();
        col.size = new Vector2(2, 2);

        // EventSystem
        new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));

        EditorSceneManager.SaveScene(scene);
    }

    static void CreateMenuScene()
    {
        string scenePath = _basePath + "/Dame_Scenes/Menu_Dame.unity";
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        EditorSceneManager.SaveScene(scene, scenePath);

        // Camera
        var camGO = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
        camGO.GetComponent<Camera>().orthographic = true;
        camGO.GetComponent<Camera>().orthographicSize = 5;
        camGO.transform.position = new Vector3(0, 0, -10);

        // Background
        var bg = new GameObject("Background", typeof(SpriteRenderer));
        var bgTex = LoadSprite("bg_menu");
        if (bgTex != null) bg.GetComponent<SpriteRenderer>().sprite = bgTex;

        // Canvas
        var canvasGO = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        var canvas = canvasGO.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasGO.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        // Titre
        var title = new GameObject("Title", typeof(RectTransform));
        title.transform.SetParent(canvasGO.transform);
        var trt = title.GetComponent<RectTransform>();
        trt.anchorMin = new Vector2(0.5f, 0.5f);
        trt.anchorMax = new Vector2(0.5f, 0.5f);
        trt.anchoredPosition = new Vector2(0, 200);
        trt.sizeDelta = new Vector2(600, 100);
        var titleTxt = title.AddComponent<TextMeshProUGUI>();
        titleTxt.text = "JEU DE DAMES";
        titleTxt.fontSize = 72;
        titleTxt.alignment = TextAlignmentOptions.Center;
        titleTxt.color = Color.white;

        // Dropdown - Temps par coup
        var timeLabel = new GameObject("TimeLabel", typeof(RectTransform));
        timeLabel.transform.SetParent(canvasGO.transform);
        var tlrt = timeLabel.GetComponent<RectTransform>();
        tlrt.anchorMin = new Vector2(0.5f, 0.5f);
        tlrt.anchorMax = new Vector2(0.5f, 0.5f);
        tlrt.anchoredPosition = new Vector2(-200, 50);
        tlrt.sizeDelta = new Vector2(300, 60);
        var timeLblTxt = timeLabel.AddComponent<TextMeshProUGUI>();
        timeLblTxt.text = "Temps par coup :";
        timeLblTxt.fontSize = 36;
        timeLblTxt.alignment = TextAlignmentOptions.MidlineRight;

        var timeDropdownGO = new GameObject("TimeDropdown", typeof(RectTransform));
        timeDropdownGO.transform.SetParent(canvasGO.transform);
        var tdrt = timeDropdownGO.GetComponent<RectTransform>();
        tdrt.anchorMin = new Vector2(0.5f, 0.5f);
        tdrt.anchorMax = new Vector2(0.5f, 0.5f);
        tdrt.anchoredPosition = new Vector2(200, 50);
        tdrt.sizeDelta = new Vector2(250, 60);
        var dropdown = timeDropdownGO.AddComponent<TMP_Dropdown>();
        dropdown.AddOptions(new System.Collections.Generic.List<string> { "10s", "15s", "30s", "60s" });
        var ddImg = timeDropdownGO.AddComponent<Image>();
        ddImg.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
        var ddLabel = new GameObject("Label", typeof(RectTransform));
        ddLabel.transform.SetParent(timeDropdownGO.transform);
        var dlrt = ddLabel.GetComponent<RectTransform>();
        dlrt.anchorMin = Vector2.zero; dlrt.anchorMax = Vector2.one;
        dlrt.sizeDelta = Vector2.zero;
        var ddLabelTxt = ddLabel.AddComponent<TextMeshProUGUI>();
        ddLabelTxt.fontSize = 28;
        ddLabelTxt.text = "15s";
        dropdown.captionText = ddLabelTxt;
        var ddTemplate = new GameObject("Template", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster), typeof(Image), typeof(ScrollRect), typeof(Mask));
        ddTemplate.transform.SetParent(timeDropdownGO.transform);
        // On s'arrête là pour le template - l'utilisateur finira dans Unity

        // Jouer
        var playGO = new GameObject("PlayButton", typeof(RectTransform));
        playGO.transform.SetParent(canvasGO.transform);
        var prt = playGO.GetComponent<RectTransform>();
        prt.anchorMin = new Vector2(0.5f, 0.5f);
        prt.anchorMax = new Vector2(0.5f, 0.5f);
        prt.anchoredPosition = new Vector2(0, -100);
        prt.sizeDelta = new Vector2(300, 80);
        var playTxt = playGO.AddComponent<TextMeshProUGUI>();
        playTxt.text = "JOUER";
        playTxt.fontSize = 48;
        playTxt.alignment = TextAlignmentOptions.Center;
        playTxt.color = Color.white;
        var playImg = playGO.AddComponent<Image>();
        playImg.color = new Color(0.2f, 0.5f, 0.2f, 1f);
        var playBtn = playGO.AddComponent<Button>();
        // playBtn.targetGraphic = playImg;  // obsolete property

        // EventSystem
        new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));

        EditorSceneManager.SaveScene(scene);
    }

    static void CreateGameScene()
    {
        string scenePath = _basePath + "/Dame_Scenes/GameScene_Dame.unity";
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        EditorSceneManager.SaveScene(scene, scenePath);

        // Camera
        var camGO = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
        var cam = camGO.GetComponent<Camera>();
        cam.orthographic = true;
        cam.orthographicSize = 5.5f;
        cam.backgroundColor = new Color(0.15f, 0.1f, 0.05f);
        camGO.transform.position = new Vector3(0, 0, -10);

        // Directional Light
        var lightGO = new GameObject("Directional Light", typeof(Light));
        lightGO.GetComponent<Light>().intensity = 0.5f;

        // EventSystem
        new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));

        // Parent du plateau
        var boardParent = new GameObject("BoardParent");

        // GameManager
        var gmGO = new GameObject("Dame_GameManager", typeof(Dame_GameManager), typeof(AudioSource));
        var gm = gmGO.GetComponent<Dame_GameManager>();

        // Canvas
        var canvasGO = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        var canvas = canvasGO.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasGO.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        // Score
        var scoreGO = new GameObject("ScoreText", typeof(RectTransform));
        scoreGO.transform.SetParent(canvasGO.transform);
        var srt = scoreGO.GetComponent<RectTransform>();
        srt.anchorMin = new Vector2(0.5f, 1);
        srt.anchorMax = new Vector2(0.5f, 1);
        srt.anchoredPosition = new Vector2(0, -30);
        srt.sizeDelta = new Vector2(400, 50);
        var scoreTxt = scoreGO.AddComponent<TextMeshProUGUI>();
        scoreTxt.text = "Blanc: 0  |  Noir: 0";
        scoreTxt.fontSize = 32;
        scoreTxt.alignment = TextAlignmentOptions.Center;
        scoreTxt.color = Color.white;
        gm.scoreText = scoreTxt;

        // Timer
        var timerGO = new GameObject("TimerText", typeof(RectTransform));
        timerGO.transform.SetParent(canvasGO.transform);
        var trt = timerGO.GetComponent<RectTransform>();
        trt.anchorMin = new Vector2(0.5f, 1);
        trt.anchorMax = new Vector2(0.5f, 1);
        trt.anchoredPosition = new Vector2(0, -80);
        trt.sizeDelta = new Vector2(200, 60);
        var timerTxt = timerGO.AddComponent<TextMeshProUGUI>();
        timerTxt.text = "15";
        timerTxt.fontSize = 48;
        timerTxt.alignment = TextAlignmentOptions.Center;
        timerTxt.color = Color.yellow;
        gm.timerText = timerTxt;

        // Current player
        var playerGO = new GameObject("CurrentPlayerText", typeof(RectTransform));
        playerGO.transform.SetParent(canvasGO.transform);
        var prt = playerGO.GetComponent<RectTransform>();
        prt.anchorMin = new Vector2(0.5f, 1);
        prt.anchorMax = new Vector2(0.5f, 1);
        prt.anchoredPosition = new Vector2(0, -130);
        prt.sizeDelta = new Vector2(400, 40);
        var playerTxt = playerGO.AddComponent<TextMeshProUGUI>();
        playerTxt.text = "Tour des Blancs";
        playerTxt.fontSize = 28;
        playerTxt.alignment = TextAlignmentOptions.Center;
        playerTxt.color = Color.white;
        gm.currentPlayerText = playerTxt;

        // GeneralVariable - creer directement
        var gvGO = new GameObject("GeneralVariable", typeof(Dame_GeneralVariables));
        var gv = gvGO.GetComponent<Dame_GeneralVariables>();
        gv.gameName = "Dame";

        // Board
        var boardGO = new GameObject("Board", typeof(Dame_Board));
        gm.board = boardGO.GetComponent<Dame_Board>();
        var boardTransform = boardGO.transform;

        EditorSceneManager.SaveScene(scene);
    }

    static void CreateScoreScene()
    {
        string scenePath = _basePath + "/Dame_Scenes/Score_Dame.unity";
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        EditorSceneManager.SaveScene(scene, scenePath);

        // Camera
        var camGO = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
        camGO.GetComponent<Camera>().orthographic = true;
        camGO.GetComponent<Camera>().orthographicSize = 5;
        camGO.transform.position = new Vector3(0, 0, -10);

        // Background
        var bg = new GameObject("Background", typeof(SpriteRenderer));
        var bgTex = LoadSprite("bg_score");
        if (bgTex != null) bg.GetComponent<SpriteRenderer>().sprite = bgTex;

        // Canvas
        var canvasGO = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasGO.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasGO.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        // Winner text
        var winnerGO = new GameObject("WinnerText", typeof(RectTransform));
        winnerGO.transform.SetParent(canvasGO.transform);
        var wrt = winnerGO.GetComponent<RectTransform>();
        wrt.anchorMin = new Vector2(0.5f, 0.5f);
        wrt.anchorMax = new Vector2(0.5f, 0.5f);
        wrt.anchoredPosition = new Vector2(0, 100);
        wrt.sizeDelta = new Vector2(600, 80);
        var winnerTxt = winnerGO.AddComponent<TextMeshProUGUI>();
        winnerTxt.text = "Victoire !";
        winnerTxt.fontSize = 64;
        winnerTxt.alignment = TextAlignmentOptions.Center;
        winnerTxt.color = Color.yellow;

        // EventSystem
        new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));

        EditorSceneManager.SaveScene(scene);
    }
}