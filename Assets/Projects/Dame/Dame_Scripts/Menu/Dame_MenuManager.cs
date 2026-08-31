using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

namespace Dame
{
    public class Dame_MenuManager : MonoBehaviour
    {
        private TMP_Dropdown timeDropdown;
        private TMP_Dropdown themeDropdown;

        private void Start()
        {
            Universal_GeneralVariables.OnPlayerPrefs += SetTime;
            SetTime();

            // Trouver le dropdown automatiquement (instancie depuis prefab universel)
            var ddGO = GameObject.Find("TimeDropdown");
            if (ddGO != null)
                timeDropdown = ddGO.GetComponentInChildren<TMP_Dropdown>();

            if (timeDropdown != null)
            {
                int savedTime = Mathf.RoundToInt(PlayerPrefs.GetFloat(Dame_GeneralVariables.TimePerMoveKey, 15f));
                int index = savedTime == 10 ? 0 : savedTime == 15 ? 1 : savedTime == 30 ? 2 : 3;
                timeDropdown.value = index;
                timeDropdown.onValueChanged.AddListener(OnTimeChanged);
            }

            // Thèmes
            if (themeDropdown != null)
            {
                var themes = new string[] { "Classique", "Bois", "Néon", "Neige" };
                themeDropdown.ClearOptions();
                themeDropdown.AddOptions(new System.Collections.Generic.List<string>(themes));
                int savedTheme = PlayerPrefs.GetInt(Dame_GeneralVariables.ThemeKey, 0);
                themeDropdown.value = savedTheme;
            }
        }

        public void OnDestroy()
        {
            Universal_GeneralVariables.OnPlayerPrefs -= SetTime;
        }

        public void SetTime()
        {
            // Appelé via OnPlayerPrefs
        }

        public void OnTimeChanged(int index)
        {
            float time = index == 0 ? 10f : index == 1 ? 15f : index == 2 ? 30f : 60f;
            PlayerPrefs.SetFloat(Dame_GeneralVariables.TimePerMoveKey, time);
            PlayerPrefs.Save();
        }

        public void OnThemeChanged(int index)
        {
            PlayerPrefs.SetInt(Dame_GeneralVariables.ThemeKey, index);
            PlayerPrefs.Save();
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