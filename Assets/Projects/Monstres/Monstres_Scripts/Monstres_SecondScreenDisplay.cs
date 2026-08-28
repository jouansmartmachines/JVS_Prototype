using UnityEngine;

namespace Monstres
{
    public class Monstres_SecondScreenDisplay : MonoBehaviour
    {
        private void Awake()
        {
            if (Display.displays.Length > 1)
            {
                PlayerPrefs.SetInt("UnitySelectMonitor", 1);
            }
        }
    }
}
