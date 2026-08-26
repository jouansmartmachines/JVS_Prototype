using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TransitionManager : MonoBehaviour
{
    public static TransitionManager Instance { get; private set; }

    private void Awake()
    {
        // On gère le Singleton pour qu'il soit accessible partout
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        
        // Permet au Manager de survivre aux changements de scènes
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// Déclenche le nettoyage de la RAM puis le chargement de la scène de score.
    /// </summary>
    public void CleanAndLoadScore(string scoreSceneName)
    {
        StartCoroutine(CleanAndLoadScoreAsync(scoreSceneName));
    }

    private IEnumerator CleanAndLoadScoreAsync(string scoreSceneName)
    {
        Debug.Log("[TransitionManager] Début du déchargement des textures inutilisées (2.69 Go)...");

        // 1. Force Unity à vider la RAM native en arrière-plan
        AsyncOperation unloadOperation = Resources.UnloadUnusedAssets();

        // On attend que la mémoire soit vidée frame par frame sans figer l'écran
        while (!unloadOperation.isDone)
        {
            yield return null;
        }

        Debug.Log("[TransitionManager] RAM nettoyée avec succès. Chargement du Score...");

        // 2. Charge la scène de score de manière asynchrone
        AsyncOperation loadOperation = SceneManager.LoadSceneAsync(scoreSceneName);
        
        while (!loadOperation.isDone)
        {
            yield return null;
        }
    }
}