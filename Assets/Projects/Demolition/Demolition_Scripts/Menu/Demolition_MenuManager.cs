using UnityEngine;
using UnityEngine.SceneManagement;

namespace Demolition
{
    public class Demolition_MenuManager : MonoBehaviour
    {
        void Start()
        {
            // Les Dropdown, Toggle, Slider sont geres automatiquement
            // par les scripts universels (DropDownPlayersPref, ToggleSavePlayerPref, etc.)
            // Rien a faire ici
        }

        public void PlayGame()
        {
            LoadingManager.LoadScene(Demolition_GeneralVariables.Instance.gameScene);
        }

        public void ChangeScene()
        {
            SceneManager.LoadScene(Demolition_GeneralVariables.Instance.accueilScene);
        }
    }
}