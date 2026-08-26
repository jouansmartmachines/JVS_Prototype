using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Profiling; 

public static class SceneTransitionUtility
{
    private static readonly ProfilerMarker s_UnloadAssetsMarker = new ProfilerMarker("Transition.UnloadUnusedAssetsStep");
    private static readonly ProfilerMarker s_LoadSceneMarker = new ProfilerMarker("Transition.LoadSceneStep");

    // Seuil en secondes pour détecter un freeze (0.1s = 100ms)
    // private const double FREEZE_THRESHOLD = 0.1; 

    public static void CleanRAMAndLoadScene(MonoBehaviour caller, int sceneIndex)
    {
        caller.StartCoroutine(CleanAndLoadAsync(sceneIndex.ToString(), isIndex: true));
    }

    public static void CleanRAMAndLoadScene(MonoBehaviour caller, string sceneName)
    {
        caller.StartCoroutine(CleanAndLoadAsync(sceneName, isIndex: false));
    }

    private static IEnumerator CleanAndLoadAsync(string sceneTarget, bool isIndex)
    {
        // double tempsDebutGlobal = Time.realtimeSinceStartupAsDouble;
        // double tempsBloqueTotal = 0;

        // Debug.Log("<color=cyan>====================================================================</color>");
        // Debug.Log($"<color=cyan>[TRACKER CRITIQUE] 🚀 DÉBUT DE LA TRANSITION À : {DateTime.Now:HH:mm:ss.fff}</color>");
        // Debug.Log("<color=cyan>====================================================================</color>");

        // ---------------------------------------------------------
        // ÉTAPE 1 : NETTOYAGE RAM (UnloadUnusedAssets)
        // ---------------------------------------------------------
        // double tempsDebutRAM = Time.realtimeSinceStartupAsDouble;
        // double tempsDerniereFrame = tempsDebutRAM;
        // int frameCountRAM = 0;

        // Debug.Log($"<color=orange>[TRACKER] ⏳ Étape 1 : Libération des ressources inutilisées...</color>");
        
        AsyncOperation unloadOperation = Resources.UnloadUnusedAssets();
        while (!unloadOperation.isDone)
        {
            using (s_UnloadAssetsMarker.Auto())
            {
                /* Variable de debug désactivée
                frameCountRAM++;
                double tempsFrameActuelle = Time.realtimeSinceStartupAsDouble;
                double dureeFrame = tempsFrameActuelle - tempsDerniereFrame;
                
                if (dureeFrame > FREEZE_THRESHOLD)
                {
                    tempsBloqueTotal += dureeFrame;
                    // Debug.LogWarning($"<color=red>[⚠️ FREEZE RAM] Frame {frameCountRAM} bloquée pendant {dureeFrame * 1000:F0} ms | Avancement : {(unloadOperation.progress * 100):F0}%</color>");
                }

                tempsDerniereFrame = tempsFrameActuelle;
                */
            }
            yield return null; 
        }
        
        // double tempsTotalRAM = (Time.realtimeSinceStartupAsDouble - tempsDebutRAM) * 1000;
        // Debug.Log($"<color=orange><b>[TRACKER] ✅ FIN ÉTAPE 1</b> -> Temps : {tempsTotalRAM:F0} ms | Frames : {frameCountRAM}</color>");
        // Debug.Log("<color=cyan>--------------------------------------------------------------------</color>");

        // ---------------------------------------------------------
        // ÉTAPE 2 : CHARGEMENT DE LA SCÈNE EN ARRIÈRE-PLAN
        // ---------------------------------------------------------
        // double tempsDebutScene = Time.realtimeSinceStartupAsDouble;
        // tempsDerniereFrame = tempsDebutScene;
        // int frameCountScene = 0;

        // Debug.Log($"<color=green>[TRACKER] ⏳ Étape 2 : Chargement asynchrone de la scène [{sceneTarget}]...</color>");

        AsyncOperation loadOperation = isIndex 
            ? SceneManager.LoadSceneAsync(int.Parse(sceneTarget)) 
            : SceneManager.LoadSceneAsync(sceneTarget);

        // OPTIMISATION MAJEURE : Empêche la scène de s'activer pendant qu'elle charge.
        // Cela force Unity à charger les assets sur un thread secondaire sans bloquer l'écran.
        loadOperation.allowSceneActivation = false;

        // Unity bloque à 0.9 tant que allowSceneActivation est faux
        while (loadOperation.progress < 0.9f)
        {
            using (s_LoadSceneMarker.Auto())
            {
                /* Variable de debug désactivée
                frameCountScene++;
                double tempsFrameActuelle = Time.realtimeSinceStartupAsDouble;
                double dureeFrame = tempsFrameActuelle - tempsDerniereFrame;
                
                if (dureeFrame > FREEZE_THRESHOLD)
                {
                    tempsBloqueTotal += dureeFrame;
                    // Debug.LogWarning($"<color=red>[⚠️ FREEZE CHARGEMENT] Frame {frameCountScene} bloquée pendant {dureeFrame * 1000:F0} ms | Loading : {(loadOperation.progress * 100):F0}%</color>");
                }

                tempsDerniereFrame = tempsFrameActuelle;
                */
            }
            yield return null;
        }

        // ---------------------------------------------------------
        // ÉTAPE 3 : ACTIVATION DE LA SCÈNE (Awake / Start)
        // ---------------------------------------------------------
        // Debug.Log("<color=yellow>[TRACKER] 🔑 Chargement en tâche de fond terminé à 90%. Activation de la scène...</color>");
        
        // On autorise enfin le moteur à basculer sur la nouvelle scène
        loadOperation.allowSceneActivation = true;

        // On attend la fin définitive (le sursaut final)
        while (!loadOperation.isDone)
        {
            /* Variable de debug désactivée
            frameCountScene++;
            double tempsFrameActuelle = Time.realtimeSinceStartupAsDouble;
            double dureeFrame = tempsFrameActuelle - tempsDerniereFrame;
            
            if (dureeFrame > FREEZE_THRESHOLD)
            {
                tempsBloqueTotal += dureeFrame;
                // Debug.LogWarning($"<color=red>[⚠️ FREEZE INITIALISATION] Phase finale / Awake-Start bloqué pendant {dureeFrame * 1000:F0} ms</color>");
            }

            tempsDerniereFrame = tempsFrameActuelle;
            */
            yield return null;
        }

        // double tempsTotalScene = (Time.realtimeSinceStartupAsDouble - tempsDebutScene) * 1000;
        // double tempsTotalGlobal = (Time.realtimeSinceStartupAsDouble - tempsDebutGlobal) * 1000;

        // ---------------------------------------------------------
        // BILAN FINAL
        // ---------------------------------------------------------
        // Debug.Log("<color=cyan>====================================================================</color>");
        // Debug.Log($"<color=yellow><b>🔥 RAPPORT DE SACCADE OPTIMISÉ :</b></color>");
        // Debug.Log($"<color=yellow>-> Durée totale de la transition : <b>{tempsTotalGlobal / 1000:F2}s</b> ({tempsTotalGlobal:F0} ms)</color>");
        // Debug.Log($"<color=yellow>-> TEMPS TOTAL DE FIXATION (Moteur bloqué) : <color=red><b>{tempsBloqueTotal * 1000:F0} ms</b></color></color>");
        // Debug.Log($"<color=yellow>-> Total Frames calculées : {frameCountRAM + frameCountScene}</color>");
        // Debug.Log("<color=cyan>====================================================================</color>");
    }
}