using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using OSC;
using System.Collections;
using Tool;

public class LoadingManager : MonoBehaviour
{
    public Slider loadingBar;
    //[SerializeField] private string gameSceneName = "GameScene_Basketball";

    const string loadingScreenSceneName = "LoadingScreen";

    private string sceneToUnload;
    private static string sceneToLoad;

    private static bool isLoading = false;
    private static bool _sendGameEnCours = true;

    public static bool isLoaded = false;
    

    private void Start()
    {
        isLoaded = true;
        sceneToUnload = GetPreviousScene();
        StartCoroutine(LoadGameScene());
    }

    public void OnDestroy()
    {
        isLoaded = false;
    }

    public static void LoadScene(string name, bool sendGameEnCours = true)
    {
        if (isLoading) return;
        isLoading = true;
        _sendGameEnCours = sendGameEnCours;
        name = ToolBox.GetGameNameWithoutSuffix(name);
        Debug.Log("name après découpe : " + name);

        sceneToLoad = name;
        SceneManager.LoadSceneAsync(loadingScreenSceneName, LoadSceneMode.Additive);
    }


    private IEnumerator LoadGameScene()
    {
        if(_sendGameEnCours) OSC_Manager.Instance.GameEnCours();
        OSC_Manager.Instance?.DeactivateAllOscMessages();
        Universal_GeneralVariables.SetShortcutsEnabled(false);

        Universal_GeneralVariables universal_GeneralVariables = FindObjectOfType<Universal_GeneralVariables>();
        if (universal_GeneralVariables != null)
        {
            Destroy(universal_GeneralVariables.gameObject);
        }
        Debug.Log("sceneToLoad" + sceneToLoad);
        //Debug.Log("Touches et OSC désactivés");
        AsyncOperation loadOp = SceneManager.LoadSceneAsync(sceneToLoad, LoadSceneMode.Additive);

        loadOp.allowSceneActivation = false;
        yield return ProgressLoadingBar(loadOp);
        loadOp.allowSceneActivation = true;

        yield return new WaitUntil(() => loadOp.isDone);

        if (!string.IsNullOrEmpty(sceneToUnload))
        {
            yield return UnloadScene(sceneToUnload);
        }

        Scene loadedScene = SceneManager.GetSceneByName(sceneToLoad);


        if (loadedScene.IsValid()) SceneManager.SetActiveScene(loadedScene);

        //Debug.Log("Étape 3 - Scène active : " + loadedScene.name);

        OSC_Manager.Instance.ReactivateAllOscMessages();
        Universal_GeneralVariables.SetShortcutsEnabled(true);
        isLoading = false;

        yield return UnloadScene(loadingScreenSceneName);
    }

    private string GetPreviousScene()
    {
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene s = SceneManager.GetSceneAt(i);
            if (s.isLoaded && s.name != "LoadingScreen")
            {
                Debug.Log("le nom de la scène est" + s.name);
                return s.name;
            }
                
        }
        return null;
    }

    private IEnumerator UnloadScene(string sceneName)
    {
        if (!SceneManager.GetSceneByName(sceneName).isLoaded)
        {
            //Debug.LogWarning($"La scène '{sceneName}' n'est pas chargée.");
            yield break;
        }



        AsyncOperation op = SceneManager.UnloadSceneAsync(sceneName);

        if (op == null)
        {
            //Debug.LogError($"L'opération de déchargement a échoué pour '{sceneName}'");
            yield break;
        }

        yield return new WaitUntil(() => op.isDone);

        //Debug.Log("Étape 2 - Scène déchargée : " + sceneName);
    }


    private IEnumerator ProgressLoadingBar(AsyncOperation op)
    {
        float progress = 0f;

        while (op.progress < 0.9f)
        {
            progress += 0.1f;
            loadingBar.value = Mathf.Min(progress, 0.9f);
            //Debug.Log("Progress: " + op.progress);
            //Debug.Log("IsDone: " + op.isDone);
            yield return new WaitForSeconds(Random.Range(0.1f, 0.1f));
        }

        while (progress < 1f)
        {
            progress += 0.05f;
            loadingBar.value = progress;
            //Debug.Log("Progress: " + op.progress);
            //Debug.Log("IsDone: " + op.isDone);
            yield return new WaitForSeconds(Random.Range(0.1f, 0.1f));
        }

        loadingBar.value = 1f;
    }
}
