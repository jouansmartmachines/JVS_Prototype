using OSC;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class BaseLeaderboard : LeaderboardParent
{
    [Header("Display")]
    public GameObject obj_Score_prefab;
    public GameObject obj_enterNamePanel;
    public Image backgroundImg;
    public Transform t_view_rect;
    public int nameMaxLenght = 10; // Longueur max des noms
    public Color newScoreColor;
    public Color backgroundColor;
    public int scoreToShow = 7;
    private int baseScoreToShow;

    [Header("ScenesTransition")]
    public float timeBeforeHomeScene = 120f;
    private float currentTimeBeforeHomeScene;

    private int scoreIdx = 0;

    private bool weCanEnterName = true;
    private string nameToSubmit = "";
    private string emptyName = "Ballon";
    [SerializeField] private Color baseColor;
    [SerializeField] public SaveData highscores;
    [SerializeField] public String gameScore;
    private int newScoreIndex = 0;
    [SerializeField] float baseValue = 0;
    [SerializeField] private ScriptableObjectValue _score;

    [SerializeField] private bool LowestFirst;
    //private BinaryFormatter bf;

    protected void Start()
    {
        ShowKeyboard();
        backgroundImg.color = backgroundColor;
        currentTimeBeforeHomeScene = timeBeforeHomeScene;
        baseScoreToShow = scoreToShow;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.N)) // To reset the leaderboard
        {
            ResetScore();
        }

        if (currentTimeBeforeHomeScene > 0) // Timer before the come back to the home
        {
            currentTimeBeforeHomeScene -= Time.deltaTime;
        }
        else
        {
            //OSC_Manager.Instance.onOSCAccueilAppli();
            //SceneManager.LoadScene(Monstres_GeneralVariables.Instance.accueilScene);
        }
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
        HighScoreData tempScore = new HighScoreData();
        tempScore.player_name = nameToSubmit;
        tempScore.player_score = (int)_score.value;
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

                if (i < highscores.scores.Count)
                {
                    if (i == newScoreIndex)
                    {
                        //Call the function in the Score prefab to setup all the text infos
                        newScore.GetComponentInChildren<ScorePrefabScript>().SetText((1 + i).ToString() + ".", highscores.scores[i].player_name, highscores.scores[i].player_score.ToString(""), newScoreColor);

                    }
                    else
                    {
                        newScore.GetComponentInChildren<ScorePrefabScript>().SetText((1 + i).ToString() + ".", highscores.scores[i].player_name, highscores.scores[i].player_score.ToString(""), baseColor);
                        //newScore.GetComponent<Monstres_LB_Scores>().SetupScore((1 + i).ToString() + ".", highscores[i].player_name, highscores[i].player_score.ToString(""), Color.white);
                    }
                }
                else
                {
                    newScore.GetComponentInChildren<ScorePrefabScript>().SetText((1 + i).ToString() + ".", Localizer.Get("Unknown"), baseValue.ToString(), baseColor);
                }
            }
        }
        else
        {
            for (int i = 0; i < baseScoreToShow; i++)
            {
                GameObject newScore = Instantiate(obj_Score_prefab, t_view_rect);
                newScore.GetComponent<ScorePrefabScript>().SetText((1 + i).ToString() + ".", Localizer.Get("Unknown"), baseValue.ToString(), baseColor);
            }
        }

        //PlayerPrefs.SetInt("Monstres_FinalScore", 0);
    }

    void SortPlayerScore(HighScoreData scoreToAdd) // Sort player score from the biggest to the smaller
    {
        if (highscores.scores.Count > 0)
        {
            for (int i = 0; i < highscores.scores.Count; i++)
            {
                if ((scoreToAdd.player_score > highscores.scores[i].player_score && !LowestFirst) || (scoreToAdd.player_score < highscores.scores[i].player_score && LowestFirst))
                {
                    highscores.scores.Insert(i, scoreToAdd);
                    newScoreIndex = i;
                    return;
                }
            }
        }

        highscores.scores.Add(scoreToAdd); // Si le score est seul ou est le plus bas possible l'ajoute au bout de la liste
        newScoreIndex = highscores.scores.Count - 1;
    }

    void SavePlayersScore(SaveData scoresData) // save player score in a custom file
    {
        //if (bf == null)
        //{
        //    bf = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
        //    FileStream stream = new FileStream(Application.persistentDataPath + "/Monstres_Scores.txt", FileMode.Create);
        //    bf.Serialize(stream, scoresData);
        //    stream.Close();
        //}
        //else
        //{
        //    FileStream stream = new FileStream(Application.persistentDataPath + "/Monstres_Scores.txt", FileMode.Open);
        //    bf.Serialize(stream, scoresData);
        //    stream.Close();
        //}
        //byte[] bytes = await Task.Run(() =>

        string jsonString = JsonUtility.ToJson(scoresData);
        byte[] bytes = System.Text.Encoding.UTF8.GetBytes(jsonString);

        using FileStream file = new FileStream(Application.persistentDataPath + "/" + gameScore + ".txt", FileMode.Create, FileAccess.Write, FileShare.Write);

        if (file == null)
            return;

        file.Write(bytes, 0, bytes.Length);
        file.Close();
    }

    void LoadPlayersScore(SaveData scoresData)
    {
        string path = Application.persistentDataPath + "/" + gameScore + ".txt";
        try
        {
            using FileStream file = File.Open(Application.persistentDataPath + "/" + gameScore + ".txt", FileMode.Open, FileAccess.ReadWrite);
            byte[] bytes = new byte[file.Length];
            file.Read(bytes, 0, (int)file.Length);
            string jsonData = System.Text.Encoding.UTF8.GetString(bytes);
            highscores = JsonUtility.FromJson<SaveData>(jsonData);
            file.Close();
        }
        catch 
        {
            return;
        }
    }

    void ResetScore() // Reset leaderboard (apply only when for the next game)
    {
        if (File.Exists(Application.persistentDataPath + "/"+ gameScore+".txt"))
        {
            File.Delete(Application.persistentDataPath + "/" + gameScore + ".txt");
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
            if (scoreIdx < Mathf.RoundToInt((highscores.scores.Count - 1) / scoreToShow) * scoreToShow)
            {
                scoreIdx += scoreToShow;
                ShowPlayerScores();
            }
        }
    }
}

[Serializable]
public class HighScoreData // Have the name and the score of each player (store data)
{
    [SerializeField]
    public string player_name;
    [SerializeField]
    public float player_score;
}
[Serializable]
public class SaveData // Have the name and the score of each player (store data)
{
    [SerializeField]
    public List<HighScoreData> scores;
}

