using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.IO;
using UnityEngine.UI;

namespace Monstres
{
    public class Monstres_MenuManager : MonoBehaviour
    {
        public TextMeshProUGUI pathIndication;
        public TMP_Dropdown difficultyDropDown;
        public Toggle useDefaultPictureToggle;

        private void Start()
        {
            //ChangeFolderIndication();
            Universal_GeneralVariables.OnPlayerPrefs += SetDifficulty;
            SetDifficulty();

            if (PlayerPrefs.HasKey(Monstres_GeneralVariables.UseDefaultPictureKEY))
                useDefaultPictureToggle.isOn = PlayerPrefs.GetInt(Monstres_GeneralVariables.UseDefaultPictureKEY) == 1;
        }

        public void OnDestroy()
        {
            Universal_GeneralVariables.OnPlayerPrefs -= SetDifficulty;
        }

        public void ChangeScene()
        {
            SceneManager.LoadScene(Monstres_GeneralVariables.Instance.accueilScene);
        }

        public void PlayGame()
        {
            LoadingManager.LoadScene(Monstres_GeneralVariables.Instance.gameScene);
        }

        public void OpenFileBrowser()
        {

        }

        public void ChangeFolderIndication()
        {
            pathIndication.text = PlayerPrefs.GetString("Monstres_PicturePath");
            CheckIfFolderHavePicture();
        }

        public void SetDifficulty()
        {
            switch (PlayerPrefs.GetString("Difficulty"))
            {
                case "Easy":
                    difficultyDropDown.value = 0;
                    break;
                case "Medium":
                    difficultyDropDown.value = 1;
                    break;
                case "Hard":
                    difficultyDropDown.value = 2;
                    break;
            }
        }

        public void SaveDifficulty()
        {
            switch (difficultyDropDown.value)
            {
                case 0:
                    PlayerPrefs.SetString("Difficulty", "Easy");
                    break;
                case 1:
                    PlayerPrefs.SetString("Difficulty", "Medium");
                    break;
                case 2:
                    PlayerPrefs.SetString("Difficulty", "Hard");
                    break;
            }
        }

        void CheckIfFolderHavePicture()
        {
            DirectoryInfo file = new DirectoryInfo(PlayerPrefs.GetString("Monstres_PicturePath"));
            FileInfo[] fInfosArray = file.GetFiles();

            foreach (FileInfo info in fInfosArray)
            {
                if (info.Extension == ".png")
                {
                    return;
                }
            }

            Debug.Log("No image in file");
        }

        public void SaveDefaultPicture()
        {
            PlayerPrefs.SetInt(Monstres_GeneralVariables.UseDefaultPictureKEY, useDefaultPictureToggle.isOn ? 1 : 0);
        }
    }
}