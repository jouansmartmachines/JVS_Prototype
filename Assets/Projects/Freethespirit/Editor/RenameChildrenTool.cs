using UnityEngine;

public class CameraVisionRenamer : MonoBehaviour
{
    private Camera cam;

    void Start()
    {
        cam = GetComponent<Camera>();
        if (cam == null)
        {
            Debug.LogError("Ce script doit être attaché à un GameObject ayant un composant Camera !");
            return;
        }

        ProcessVisibleObjects();
    }

    void ProcessVisibleObjects()
    {
        Plane[] cameraPlanes = GeometryUtility.CalculateFrustumPlanes(cam);
        Renderer[] allRenderers = FindObjectsOfType<Renderer>();
        int renamedCount = 0;

        foreach (Renderer rend in allRenderers)
        {
            if (rend == null) continue;
            GameObject obj = rend.gameObject;

            // Si l'objet commence par "tsi" (insensible à la casse)
            if (obj.name.ToLower().StartsWith("tsi"))
            {
                // Teste si l'objet est dans le champ de vision de la caméra
                if (GeometryUtility.TestPlanesAABB(cameraPlanes, rend.bounds))
                {
                    obj.name = "utils";
                    // Note: En Play Mode, HideFlags.NotEditable peut se réinitialiser à l'arrêt, 
                    // mais l'objet sera bien renommé.
                    renamedCount++;
                }
            }
        }

        Debug.Log($"[CameraVisionRenamer] Terminé ! {renamedCount} objets 'tsi' vus par la caméra ont été renommés en 'utils'.");
    }
}