using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine.UI;
using OSC;

namespace Monstres
{
    public class Monstres_LB_Highscores : LeaderboardParent
    {
        [Header("Display")]
        public GameObject obj_Score_prefab;
        public GameObject obj_enterNamePanel;
        public Image backgroundImg;
        public Transform t_view_rect;
        public int nameMaxLenght = 10; // Longueur max des noms
        public Color newScoreColor;
        //public Color backgroundColor;
        public int scoreToShow = 7;
        private int baseScoreToShow;

        [Header("ScenesTransition")]
        public float timeBeforeHomeScene = 120f;
        private float currentTimeBeforeHomeScene;

        private int scoreIdx = 0;

        private bool weCanEnterName = true;
        private string nameToSubmit = "";
        private string emptyName = "Ballon";
        public List<HighScore_Data> highscores = new List<HighScore_Data>();
        private int newScoreIndex = 0;
        private BinaryFormatter bf;

        protected void Start()
        {
            ShowKeyboard();
            //backgroundImg.color = backgroundColor;
            currentTimeBeforeHomeScene = timeBeforeHomeScene;
            baseScoreToShow = scoreToShow;
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.N)) // To reset the leaderboard
            {
                ResetScore();
            }

            //if (currentTimeBeforeHomeScene > 0) // Timer before the come back to the home
            //{
            //    currentTimeBeforeHomeScene -= Time.deltaTime;
            //}
            //else
            //{
            //    OSC_Manager.Instance.onOSCAccueilAppli();
            //    SceneManager.LoadScene(Monstres_GeneralVariables.Instance.accueilScene);
            //}
        }

        void HideEnterNamePanel()
        {
            obj_enterNamePanel.SetActive(false);
        }

        public override void NameSubmit(string directNameToSubmit = "") // This function is call when someone enter a name in the phone app, handle all leaderboard behavior
        {
            if (weCanEnterName)
            {
                weCanEnterName = false; // player can't enter a second name
                HideEnterNamePanel();

                if (directNameToSubmit != "")
                {
                    nameToSubmit = directNameToSubmit;
                }
                else
                {
                    nameToSubmit = emptyName; // If the name is empty submit a standard name
                }

                if (directNameToSubmit.Length > nameMaxLenght)
                {
                    nameToSubmit = directNameToSubmit.Substring(0, nameMaxLenght); // If the name is too big cut the last letters
                }
                SubmitScore();
            }
        }

        void SubmitScore() // This function handle all the display of the leaderboard
        {
            LoadPlayersScore(highscores);
            HighScore_Data tempScore = new HighScore_Data();
            tempScore.player_name = nameToSubmit;
            tempScore.player_score = PlayerPrefs.GetInt("Monstres_FinalScore");
            SortPlayerScore(tempScore);
            SavePlayersScore(highscores);

            int tempIndex = scoreToShow;

            while (tempIndex <= newScoreIndex)
            {
                scoreIdx += scoreToShow;
                tempIndex += scoreToShow;
            }
            ShowPlayerScores();
        }

        void ShowPlayerScores()
        {
            foreach (Transform child in t_view_rect)
            {
                Destroy(child.gameObject);
            }

            if (highscores != null)
            {
                for (int i = scoreIdx; i < scoreIdx + scoreToShow; i++)
                {
                    GameObject newScore = Instantiate(obj_Score_prefab, t_view_rect);

                    if (i < highscores.Count)
                    {
                        if (i == newScoreIndex)
                        {
                            //Call the function in the Score prefab to setup all the text infos
                            newScore.GetComponent<Monstres_LB_Scores>().SetupScore((1 + i).ToString() + ".", highscores[i].player_name, highscores[i].player_score.ToString(""), newScoreColor);
                        }
                        else
                        {
                            newScore.GetComponent<Monstres_LB_Scores>().SetupScore((1 + i).ToString() + ".", highscores[i].player_name, highscores[i].player_score.ToString(""));
                        }
                    }
                    else
                    {
                        newScore.GetComponent<Monstres_LB_Scores>().SetupScore((1 + i).ToString() + ".", Localizer.Get("Unknown"), "0");
                    }
                }
            }
            else
            {
                for (int i = 0; i < baseScoreToShow; i++)
                {
                    GameObject newScore = Instantiate(obj_Score_prefab, t_view_rect);
                    newScore.GetComponent<Monstres_LB_Scores>().SetupScore((1 + i).ToString() + ".", Localizer.Get("Unknown"), "0");
                }
            }

            //PlayerPrefs.SetInt("Monstres_FinalScore", 0);
        }

        void SortPlayerScore(HighScore_Data scoreToAdd) // Sort player score from the biggest to the smaller
        {
            if (highscores.Count > 0)
            {
                for (int i = 0; i < highscores.Count; i++)
                {
                    if (scoreToAdd.player_score > highscores[i].player_score)
                    {
                        highscores.Insert(i, scoreToAdd);
                        newScoreIndex = i;
                        return;
                    }
                }
            }

            highscores.Add(scoreToAdd); // Si le score est seul ou est le plus bas possible l'ajoute au bout de la liste
            newScoreIndex = highscores.Count - 1;
        }

        void SavePlayersScore(List<HighScore_Data> scoresData) // save player score in a custom file
        {
            if (bf == null)
            {
                bf = new BinaryFormatter();
                FileStream stream = new FileStream(Application.persistentDataPath + "/Monstres_Scores.sav", FileMode.Create, FileAccess.ReadWrite);
                bf.Serialize(stream, scoresData);
                stream.Close();
            }
            else
            {
                FileStream stream = new FileStream(Application.persistentDataPath + "/Monstres_Scores.sav", FileMode.Open, FileAccess.ReadWrite);
                bf.Serialize(stream, scoresData);
                stream.Close();
            }
        }

        void LoadPlayersScore(List<HighScore_Data> scoresData)
        {
            string path = Application.persistentDataPath + "/Monstres_Scores.sav";

            if (File.Exists(path)) // load player score from the custom file
            {
                FileStream stream = new FileStream(path, FileMode.Open, FileAccess.ReadWrite);

                if (stream.Length > 0)
                {
                    BinaryFormatter bf = new BinaryFormatter();
                    highscores = bf.Deserialize(stream) as List<HighScore_Data>;
                    stream.Close();
                }
                else
                {
                    Debug.LogError("Save file was not found in " + path);
                    BinaryFormatter formatter = new BinaryFormatter();
                    formatter.Serialize(stream, scoresData);
                    stream.Close();
                    LoadPlayersScore(scoresData);
                }
            }
        }

        public override void ResetScore() // Reset leaderboard (apply only when for the next game)
        {
            if (File.Exists(Application.persistentDataPath + "/Monstres_Scores.sav"))
            {
                File.Delete(Application.persistentDataPath + "/Monstres_Scores.sav");
                highscores = null;
                ShowPlayerScores();
            }
        }

        public override void ScrollUp()
        {
            if (highscores != null)
            {
                if (scoreIdx > 0)
                {
                    scoreIdx -= scoreToShow;
                    ShowPlayerScores();
                }
            }
        }

        public override void ScrollDown()
        {
            if (highscores != null)
            {
                if (scoreIdx < Mathf.RoundToInt((highscores.Count - 1) / scoreToShow) * scoreToShow)
                {
                    scoreIdx += scoreToShow;
                    ShowPlayerScores();
                }
            }
        }
    }

    [Serializable]
    public class HighScore_Data // Have the name and the score of each player (store data)
    {
        public string player_name;
        public float player_score;
    }
}