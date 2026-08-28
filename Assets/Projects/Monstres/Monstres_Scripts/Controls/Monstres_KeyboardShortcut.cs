using OSC;
using UnityRawInput;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Monstres
{
    public class Monstres_KeyboardShortcut : MonoBehaviour
    {
        public static Monstres_KeyboardShortcut Instance { get; private set; }

        public KeyCode menuShortcut;
        public KeyCode reloadShortcut;
        public KeyCode accueilShortcut;
        public KeyCode quitShortcut;


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
            if (Input.GetKeyDown(reloadShortcut) || RawKeyInput.IsKeyDown((RawKey)System.Enum.Parse(typeof(RawKey), reloadShortcut.ToString())))
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }
            else if (Input.GetKeyDown(accueilShortcut) || RawKeyInput.IsKeyDown((RawKey)System.Enum.Parse(typeof(RawKey), accueilShortcut.ToString())))
            {
                OSC_Manager.Instance.onOSCAccueilAppli();
                SceneManager.LoadScene(Monstres_GeneralVariables.Instance.accueilScene);
            }
            else if (Input.GetKey(quitShortcut) || RawKeyInput.IsKeyDown((RawKey)System.Enum.Parse(typeof(RawKey), quitShortcut.ToString())))
            {
                OSC_Manager.Instance.messageOutQuit();
                Application.Quit();
            }
            else if (Input.GetKeyDown(menuShortcut) || RawKeyInput.IsKeyDown((RawKey)System.Enum.Parse(typeof(RawKey), menuShortcut.ToString())))
            {
                OSC_Manager.Instance.onOSCAccueilAppli();
                SceneManager.LoadScene(Monstres_GeneralVariables.Instance.menuScene);
            }
        }
    }
}