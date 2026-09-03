using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityRawInput;

public class OptionsMenuLoader : MonoBehaviour
{
    public KeyCode shortcutKey;
    public string targetSceneName; 
    public static string previousSceneName;

    public void Start()
    {
        RawKeyInput.Start(true);
    }

    public void Update()
    {
        if (Input.GetKeyDown(shortcutKey) || RawKeyInput.IsKeyDown((RawKey)System.Enum.Parse(typeof(RawKey), shortcutKey.ToString())))
        {
            string currentScene = SceneManager.GetActiveScene().name;
            if (currentScene.StartsWith("Menu"))
            {
                SceneManager.LoadScene(previousSceneName);
            }
            else
            {
                previousSceneName = SceneManager.GetActiveScene().name;
                SceneManager.LoadScene(targetSceneName);
            }

        }
    }
}
