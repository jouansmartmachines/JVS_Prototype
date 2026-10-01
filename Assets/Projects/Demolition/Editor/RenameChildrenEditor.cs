using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class RenameChildrenEditor : EditorWindow
{
    [MenuItem("Tools/Renommer Hiérarchie UT")]
    public static void RenameObjects()
    {
        Transform[] allTransforms = Object.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        List<GameObject> objectsToRename = new List<GameObject>();

        foreach (Transform t in allTransforms)
        {
            // Vérifie que l'objet commence par "tsi"
            if (t.name.StartsWith("tsi", System.StringComparison.OrdinalIgnoreCase))
            {
                // Vérifie si un de ses PARENTS est nommé "utils" ou commence par "ut"
                if (HasUtilsAncestor(t))
                {
                    objectsToRename.Add(t.gameObject);
                }
            }
        }

        if (objectsToRename.Count == 0)
        {
            Debug.Log("Aucun enfant TSI trouvé sous un dossier UT/Utils.");
            return;
        }

        // Enregistre pour le Ctrl+Z
        Undo.RegisterCompleteObjectUndo(objectsToRename.ToArray(), "Remplacer TSI par UT");

        int count = 0;
        foreach (GameObject go in objectsToRename)
        {
            // Remplace "TSI" ou "tsi" au début du nom par "ut"
            // Exemple : "TSI_Rock_Large_01A" devient "ut_Rock_Large_01A"
            go.name = Regex.Replace(go.name, "^tsi", "ut", RegexOptions.IgnoreCase);
            count++;
        }

        EditorApplication.RepaintHierarchyWindow();

        Debug.Log($"Succès ! {count} enfant(s) renommé(s) (TSI remplacé par ut).");
    }

    private static bool HasUtilsAncestor(Transform t)
    {
        Transform parent = t.parent;
        while (parent != null)
        {
            if (parent.name.StartsWith("utils", System.StringComparison.OrdinalIgnoreCase) ||
                parent.name.StartsWith("ut", System.StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
            parent = parent.parent;
        }
        return false;
    }
}