using OSC;
using UnityRawInput;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class Universal_KeyboardShortcut : MonoBehaviour
{
    public static Universal_KeyboardShortcut Instance { get; private set; }

    public KeyCode menuShortcut = KeyCode.M;
    public string menuScene = "Menu_";

    public KeyCode reloadShortcut = KeyCode.L;
    public KeyCode accueilShortcut = KeyCode.A;
    public string accueilScene = "Accueil_";
    public KeyCode quitShortcut = KeyCode.Escape;
    public KeyCode resetScoreShortcut = KeyCode.S;

    public static bool ShortcutsActive = true;

    private void Awake()
    {
        if (Instance == null)
        {
            UnityRawInput.RawKeyInput.Start(true);//true to work in background
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        UnityRawInput.RawKeyInput.Stop();
    }

    private void Update()
    {
        if (!ShortcutsActive) return;

        if (Input.GetKeyDown(reloadShortcut) || RawKeyInput.IsKeyDown((RawKey)System.Enum.Parse(typeof(RawKey), reloadShortcut.ToString())))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        if (Input.GetKeyDown(accueilShortcut) || RawKeyInput.IsKeyDown((RawKey)System.Enum.Parse(typeof(RawKey), accueilShortcut.ToString())))
        {
            if (SceneManager.GetActiveScene().name == accueilScene || BuildState.CurrentState == BuildState.State.normal)
            {
                OSC_Manager.Instance.onOSCAccueilAppli();
                SceneManager.LoadScene(accueilScene);
            }
            else
            {
                SceneManager.LoadScene("SelectionMenu");
            }
        }

        if (Input.GetKey(quitShortcut) || RawKeyInput.IsKeyDown((RawKey)System.Enum.Parse(typeof(RawKey), quitShortcut.ToString())))
        {
            OSC_Manager.Instance.messageOutQuit();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
            Application.Quit();
        }

        if (Input.GetKeyDown(menuShortcut) || RawKeyInput.IsKeyDown((RawKey)System.Enum.Parse(typeof(RawKey), menuShortcut.ToString())))
        {
            OSC_Manager.Instance.onOSCAccueilAppli();
            SceneManager.LoadScene(menuScene);


        }

        if (Input.GetKeyDown(resetScoreShortcut) || RawKeyInput.IsKeyDown((RawKey)System.Enum.Parse(typeof(RawKey), resetScoreShortcut.ToString())))
        {
            OSC_Manager.Instance.OnResetAllScoreBoard(null);
        }
    }

    public static void SetShortcutsEnabled(bool value)
    {
        ShortcutsActive = value;
        UnityRawInput.RawKeyInput.Start(value);
        if (EventSystem.current != null)
            EventSystem.current.sendNavigationEvents = value;

    }
}