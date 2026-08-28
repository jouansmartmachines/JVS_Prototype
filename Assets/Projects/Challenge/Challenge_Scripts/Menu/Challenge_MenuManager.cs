using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.IO;
using UnityEngine.UI;

namespace Challenge
{
    public class Challenge_MenuManager : MonoBehaviour
    {
        public TextMeshProUGUI pathIndication;
        public TMP_Dropdown difficultyDropDown;
        public Toggle useDefaultPictureToggle;

        private void Start()
        {
            //ChangeFolderIndication();
            Universal_GeneralVariables.OnPlayerPrefs += SetDifficulty;
            SetDifficulty();

            
        }

        public void OnDestroy()
        {
            Universal_GeneralVariables.OnPlayerPrefs -= SetDifficulty;
        }

        public void ChangeScene()
        {
            SceneManager.LoadScene(Challenge_GeneralVariables.i.accueilScene);
        }

        public void PlayGame()
        {
            LoadingManager.LoadScene(Challenge_GeneralVariables.i.gameScene);
        }

        public void OpenFileBrowser()
        {

        }

        public void ChangeFolderIndication()
        {
            pathIndication.text = PlayerPrefs.GetString("Challenge_PicturePath");
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
            DirectoryInfo file = new DirectoryInfo(PlayerPrefs.GetString("Challenge_PicturePath"));
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
           
        }
    }
}