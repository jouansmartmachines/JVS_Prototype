using OSC;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityRawInput;
using UnityEngine.UI;
using TMPro;


public class 
Universal_GeneralVariables : ReceiveParent
{
    public static Action OnPlayerPrefs;

    [Header("Scene")]
    public string gameName;
    public string menuScene => "Menu_" + gameName;
    public string accueilScene => "Accueil_" + gameName;
    public string introScene => "Intro_" + gameName;
    public string gameScene => "GameScene_" + gameName;
    public string scoreScene => "Score_" + gameName;
    public bool callGameEnCoursAtStart = true;

    [Space(10), Header("Settings")]
    public List<SavedData> dataSaved;

    protected bool gotAPt;
    protected Vector3 newPt;
    protected int w, h;
    public float margeY = 1f;
    public float margex = 1f;

    //[Space(10), Header("Shortcut")]
    public static KeyCode menuShortcut = KeyCode.M;
    public static KeyCode reloadShortcut = KeyCode.L;
    public static KeyCode accueilShortcut = KeyCode.A;
    public static KeyCode quitShortcut = KeyCode.Escape;
    public static KeyCode resetScoreShortcut = KeyCode.S;

    public static bool ActiveShortcut = true; 

    private void ResetWait() => waitValue = 120f;
    private float waitValue = 120f;

    


    protected virtual IEnumerator Start()
    {
        UnityRawInput.RawKeyInput.Start(true); //true to work in background

        while (waitValue > 0)
        {
            yield return new WaitForSeconds(1f);
            waitValue--;
        }
        OSC_Manager.Instance.onOSCAccueilTous(0);
    }

    /// <summary>
    /// All Shortcut from Universal_KeyboardShortcut
    /// </summary>
    public virtual void Update()
    {
        if (!ActiveShortcut) return;

/*
        if (UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject != null)
        {
            InputField input = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject.GetComponent<InputField>();
            TMP_InputField tmpInput = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject.GetComponent<TMPro.TMP_InputField>();

            if (input != null || tmpInput != null)
                return; 
        }
    */    
        /*
        if (Input.GetKeyDown(reloadShortcut) || RawKeyInput.IsKeyDown((RawKey)System.Enum.Parse(typeof(RawKey), reloadShortcut.ToString())))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            return;
        }
        */

        if (Input.GetKeyDown(accueilShortcut) || RawKeyInput.IsKeyDown((RawKey)System.Enum.Parse(typeof(RawKey), accueilShortcut.ToString())))
        {
            Debug.Log(SceneManager.GetActiveScene().name);
            if (SceneManager.GetActiveScene().name != accueilScene)
            {
                OSC_Manager.Instance.onOSCAccueilAppli();
                SceneManager.LoadScene(accueilScene);
                return;
            }
            else
            if (BuildState.CurrentState == BuildState.State.menuSelection)
            {
                SceneManager.LoadScene("SelectionMenu");
                return;
            }
            else
            {
                OSC_Manager.Instance.onOSCAccueilTous(0);
                return;
            }
        }

        if (Input.GetKey(quitShortcut) || RawKeyInput.IsKeyDown((RawKey)System.Enum.Parse(typeof(RawKey), quitShortcut.ToString())))
        {
            OSC_Manager.Instance.messageOutQuit();
            #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
            #endif
            Application.Quit();
            return;
        }

        if (Input.GetKeyDown(menuShortcut) || RawKeyInput.IsKeyDown((RawKey)System.Enum.Parse(typeof(RawKey), menuShortcut.ToString())))
        {
            OSC_Manager.Instance.onOSCAccueilAppli();
            SceneManager.LoadScene(menuScene);
            return;
        }

        if (Input.GetKeyDown(resetScoreShortcut) || RawKeyInput.IsKeyDown((RawKey)System.Enum.Parse(typeof(RawKey), resetScoreShortcut.ToString())))
        {
            OSC_Manager.Instance.OnResetAllScoreBoard(null);
            return;
        }
    }

    public static void SetShortcutsEnabled(bool value)
    {
        ActiveShortcut = value;

        if (value)
        {
            UnityRawInput.RawKeyInput.Start(true);
            Debug.Log("Shortcuts are enabled.");
        }
        else
        {
            UnityRawInput.RawKeyInput.Stop();
            Debug.Log("Shortcuts are disabled.");
        }

    }
    
    public static void SetShortcutsEnabled()
    {
        ActiveShortcut = !ActiveShortcut;

    }

    public virtual void OnDestroy()
    {
        UnityRawInput.RawKeyInput.Stop();
    }

    protected virtual void CheckSavedData()
    {
        foreach (SavedData datas in dataSaved)
        {
            switch (datas.dataSavedType)
            {
                case SavedData.DataType.Float:

                    if (PlayerPrefs.GetFloat(datas.saveDataName) == 0)
                    {
                        PlayerPrefs.SetFloat(datas.saveDataName, datas.fSaveDataBaseValue);
                    }

                    break;

                case SavedData.DataType.Int:

                    if (PlayerPrefs.GetInt(datas.saveDataName) == 0)
                    {
                        PlayerPrefs.SetInt(datas.saveDataName, Mathf.RoundToInt(datas.fSaveDataBaseValue));
                    }

                    break;

                case SavedData.DataType.String:

                    if (PlayerPrefs.GetString(datas.saveDataName) == "")
                    {
                        PlayerPrefs.SetString(datas.saveDataName, datas.sSaveDataBaseValue);
                    }

                    break;
            }
        }
    }

    public void StartGame(bool sendGameEnCours = false)
    {
        Debug.Log("GameScene name 1 :" + gameScene);
        LoadingManager.LoadScene(gameScene, sendGameEnCours);
        //if (sendGameEnCours) OSC_Manager.Instance.GameEnCours();
    }

    public void StartGame()
    {
        Debug.Log("GameScene name 2 :" + gameScene);
        LoadingManager.LoadScene(gameScene, callGameEnCoursAtStart);
        //OSC_Manager.Instance.GameEnCours();
    }

    public void LoadAccueil()
    {
        OSC_Manager.Instance.onOSCAccueilAppli();
         Debug.Log("GameScene name 3 :" + accueilScene);
        SceneManager.LoadScene(accueilScene);
    }

    public void StartChoix()
    {
        OSC_Manager.Instance.StartChoix();
    }

    public override void ReceivePoint(float xPoint, float yPoint)
    {
        newPt.x = xPoint * w;
        newPt.y = yPoint * h;
        gotAPt = true;
        ResetWait();
    }
    public virtual void ReceiveName(string name) { }
    public virtual void OnConfigGame(OscMessage message) { }
    public virtual void OnChoix(OscMessage message) { }
}

[System.Serializable]
public class SavedData
{
    public string saveDataName;
    public float fSaveDataBaseValue;
    public string sSaveDataBaseValue;

    public enum DataType { Float, Int, String }
    public DataType dataSavedType;
}

