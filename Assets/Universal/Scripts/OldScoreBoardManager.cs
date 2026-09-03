using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

public enum OldGameScoreBoard
{
    Mansion_Scores,
    CovidKiller_Scores,
    DeadWar_Scores,
    Molecules_Scores,
    Monstres_Scores,
    Mosquito_Scores,
    Nettoyage_Scores,
    RailShooter_Scores,
    Target_Scores,
    Tetrax_Scores
}

public static class OldScoreBoardManager
{
    static string GetPath(OldGameScoreBoard game) => Application.persistentDataPath + "/" + game.ToString() + ".sav";

    //public async void ResetAllScoreBoard()
    //{
    //    foreach (GameScoreBoard scoreBoard in _allScoreBoard)
    //    {
    //        await CreateScoreBoard(scoreBoard);
    //    }
    //}

    static public void CreateScoreBoard(OldGameScoreBoard game)
    {
        if (File.Exists(GetPath(game)))
        {
            Debug.LogWarning("Old ScoreBoard File Delete");
            File.Delete(GetPath(game));
        }

        using FileStream file = File.Create(GetPath(game));
        //string json = JsonConvert.SerializeObject(playerDatas);
        //File.WriteAllText(PATH, json);
    }

    static public void ResetAll()
    {
        for (int i = 0; i < Enum.GetValues(typeof(OldGameScoreBoard)).Length; i++)
            CreateScoreBoard((OldGameScoreBoard)i);
    }
}
