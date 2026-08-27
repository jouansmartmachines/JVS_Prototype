using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Demolition
{
    public class Demolition_LB_Highscores : LeaderboardParent
    {
        [Header("Display")]
        public GameObject scorePrefab;
        public GameObject enterNamePanel;
        public Transform viewRect;
        public int nameMaxLength = 10;
        public Color newScoreColor;
        public int scoreToShow = 7;
        private int scoreIdx = 0;
        private bool weCanEnterName = true;
        private string nameToSubmit = "";
        private string emptyName = "Bras cassé";
        public List<HighScoreData> highscores = new List<HighScoreData>();
        private int newScoreIndex = 0;

        void Start()
        {
            ShowKeyboard();
        }

        public override void NameSubmit(string directNameToSubmit = "")
        {
            if (!weCanEnterName) return;
            weCanEnterName = false;

            if (enterNamePanel != null)
                enterNamePanel.SetActive(false);

            nameToSubmit = string.IsNullOrEmpty(directNameToSubmit) ? emptyName : directNameToSubmit;
            if (nameToSubmit.Length > nameMaxLength)
                nameToSubmit = nameToSubmit.Substring(0, nameMaxLength);

            SubmitScore();
        }

        void SubmitScore()
        {
            LoadPlayersScore();
            HighScoreData temp = new HighScoreData
            {
                player_name = nameToSubmit,
                player_score = PlayerPrefs.GetInt("Demolition_FinalScore", 0)
            };
            SortPlayerScore(temp);
            SavePlayersScore();
            ShowPlayerScores();
        }

        void ShowPlayerScores()
        {
            foreach (Transform child in viewRect)
                Destroy(child.gameObject);

            if (highscores == null) return;

            for (int i = scoreIdx; i < scoreIdx + scoreToShow && i < highscores.Count; i++)
            {
                GameObject go = Instantiate(scorePrefab, viewRect);
                var scoreScript = go.GetComponent<Demolition_LB_Scores>();
                if (scoreScript != null)
                {
                    Color color = (i == newScoreIndex) ? newScoreColor : Color.white;
                    scoreScript.SetupScore(
                        (1 + i).ToString() + ".",
                        highscores[i].player_name,
                        highscores[i].player_score.ToString(),
                        color
                    );
                }
            }
        }

        void SortPlayerScore(HighScoreData scoreToAdd)
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
            highscores.Add(scoreToAdd);
            newScoreIndex = highscores.Count - 1;
        }

        void SavePlayersScore()
        {
            string path = Application.persistentDataPath + "/Demolition_Scores.sav";
            BinaryFormatter bf = new BinaryFormatter();
            using (FileStream stream = new FileStream(path, FileMode.Create))
            {
                bf.Serialize(stream, highscores);
            }
        }

        void LoadPlayersScore()
        {
            string path = Application.persistentDataPath + "/Demolition_Scores.sav";
            if (File.Exists(path))
            {
                BinaryFormatter bf = new BinaryFormatter();
                using (FileStream stream = new FileStream(path, FileMode.Open))
                {
                    if (stream.Length > 0)
                        highscores = bf.Deserialize(stream) as List<HighScoreData>;
                }
            }
            if (highscores == null)
                highscores = new List<HighScoreData>();
        }

        public override void ResetScore()
        {
            string path = Application.persistentDataPath + "/Demolition_Scores.sav";
            if (File.Exists(path))
            {
                File.Delete(path);
                highscores = null;
                ShowPlayerScores();
            }
        }

        public override void ScrollUp()
        {
            if (scoreIdx > 0) scoreIdx -= scoreToShow;
            ShowPlayerScores();
        }

        public override void ScrollDown()
        {
            if (highscores != null && scoreIdx < highscores.Count - scoreToShow)
            {
                scoreIdx += scoreToShow;
                ShowPlayerScores();
            }
        }
    }

    [System.Serializable]
    public class HighScoreData
    {
        public string player_name;
        public float player_score;
    }
}