using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

namespace Demolition
{
    public class Demolition_MenuManager : MonoBehaviour
    {
        public TMP_Dropdown difficultyDropdown;
        public Toggle modeOiseauToggle;

        void Start()
        {
            Universal_GeneralVariables.OnPlayerPrefs += SetDifficulty;
            SetDifficulty();

            // Mode oiseau par défaut
            modeOiseauToggle.isOn = PlayerPrefs.GetInt(Demolition_GeneralVariables.ModeOiseauKey, 1) == 1;
        }

        void OnDestroy()
        {
            Universal_GeneralVariables.OnPlayerPrefs -= SetDifficulty;
        }

        public void PlayGame()
        {
            SceneManager.LoadScene(Demolition_GeneralVariables.Instance.gameScene);
        }

        public void ChangeScene()
        {
            SceneManager.LoadScene(Demolition_GeneralVariables.Instance.accueilScene);
        }

        void SetDifficulty()
        {
            switch (PlayerPrefs.GetString("Difficulty"))
            {
                case "Easy": difficultyDropdown.value = 0; break;
                case "Medium": difficultyDropdown.value = 1; break;
                case "Hard": difficultyDropdown.value = 2; break;
            }
        }

        public void SaveDifficulty()
        {
            switch (difficultyDropdown.value)
            {
                case 0: PlayerPrefs.SetString("Difficulty", "Easy"); break;
                case 1: PlayerPrefs.SetString("Difficulty", "Medium"); break;
                case 2: PlayerPrefs.SetString("Difficulty", "Hard"); break;
            }
        }

        public void SaveModeOiseau()
        {
            PlayerPrefs.SetInt(Demolition_GeneralVariables.ModeOiseauKey, modeOiseauToggle.isOn ? 1 : 0);
        }
    }
}