using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

namespace Dame
{
    public class Dame_MenuManager : MonoBehaviour
    {
        private void Start()
        {
            // Le Dropdown est géré par DropDownPlayersPref (script universel attaché dans le prefab)
            // Ses options: 10s/15s/30s/60s, clé: Dame_GameTime
            // Rien à faire ici car le script universel gère tout
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