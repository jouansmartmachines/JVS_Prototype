using UnityEngine;
using System.Collections.Generic;

public class MonitorSelector : MonoBehaviour
{
    void Start()
    {
        List<DisplayInfo> displays = new List<DisplayInfo>();
        Screen.GetDisplayLayout(displays);

        // Vérifie si on a au moins 2 écrans
        if (displays.Count > 1)
        {
            // Déplace la fenêtre vers le deuxième écran (index 1)
            // Vector2Int.zero place la fenêtre en haut à gauche de cet écran
            Screen.MoveMainWindowTo(displays[1], Vector2Int.zero);
        }
    }
}