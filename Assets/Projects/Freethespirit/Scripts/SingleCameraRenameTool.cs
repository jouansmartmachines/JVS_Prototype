#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

[ExecuteInEditMode]
public class SmartFrustumRenamer : MonoBehaviour
{
    [ContextMenu("Exécuter le renommage permanent")]
    public void ExecuteRenaming()
    {
        Camera cam = GetComponent<Camera>();
        if (cam == null)
        {
            Debug.LogError("[SmartFrustumRenamer] Ce script doit être attaché à un GameObject possédant une Camera !");
            return;
        }

        Plane[] cameraPlanes = GeometryUtility.CalculateFrustumPlanes(cam);
        Renderer[] allRenderers = FindObjectsOfType<Renderer>();
        
        int renamedCount = 0;

        foreach (Renderer rend in allRenderers)
        {
            if (rend == null) continue;
            GameObject obj = rend.gameObject;

            // Vérifier si l'objet est dans le champ de vision de la caméra
            if (GeometryUtility.TestPlanesAABB(cameraPlanes, rend.bounds))
            {
                // 1. Si l'objet commence par "tsi" (insensible à la casse)
                if (obj.name.ToLower().StartsWith("tsi"))
                {
                    Undo.RecordObject(obj, "Renommer tsi en utils");
                    obj.name = "utils";
                    renamedCount++;
                }

                // 2. Si son parent direct commence par "tsi"
                if (obj.transform.parent != null)
                {
                    GameObject parentObj = obj.transform.parent.gameObject;
                    if (parentObj.name.ToLower().StartsWith("tsi"))
                    {
                        Undo.RecordObject(parentObj, "Renommer parent tsi en utils");
                        parentObj.name = "utils";
                        renamedCount++;
                    }
                }
            }
        }

        // Marquer la scène comme modifiée pour que Unity demande à sauvegarder
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

        Debug.Log($"[SmartFrustumRenamer] Terminé ! {renamedCount} éléments renommés en 'utils' et sauvegardés définitivement.");
    }
}
#endif