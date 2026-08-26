using System.Collections;
using System.Collections.Generic;
using Theme;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneManagerUniversal : MonoBehaviour
{
    [SerializeField] private LoadSceneMode _mode;
    [SerializeField] private bool _bstart;
    [SerializeField] private string _sceneName;
    [SerializeField] private ThemeManagerLoader _themeLoader;
    // Update is called once per frame
    public void ChangeScene( string name)
    {
        SceneManager.LoadScene(name,_mode);
    }

    public IEnumerator Start()
    {
        if (!_bstart)
            yield break;

        _themeLoader.LoadAllThemeManagers();

        MenuSelection.MenuSelectionManager.LastPanel = 0;
        string scene = _sceneName;
        if (BuildState.CurrentState != BuildState.State.normal)
        {
            scene = BuildState.MenuSelectionSceneName;
        }
        yield return null;
        yield return SceneManager.LoadSceneAsync(scene, _mode);
    }
}
