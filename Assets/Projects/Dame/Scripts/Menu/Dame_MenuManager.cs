using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

namespace Dame
{
    public class Dame_MenuManager : MonoBehaviour
    {
        private TMP_Dropdown timeDropdown;

        void Start()
        {
            // Le dropdown Difficulty est gere par DropDownPlayersPref (cle: Dame_GameTime)
            // Le dropdown Theme est gere par ThemeSelector
            // Rien a faire ici — tout est automatique
        }

        public void PlayGame()
        {
            LoadingManager.LoadScene(Dame_GeneralVariables.Instance.gameScene);
        }

        public void ChangeScene()
        {
            SceneManager.LoadScene(Dame_GeneralVariables.Instance.accueilScene);
        }
    }
}