using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class PresetValueManager
{
    public static string GetPath()
    {
        string path = "";
        string newPath;
        newPath = Path.GetFullPath(Path.Combine(Application.dataPath, @"../../../../"));
        path = newPath + "data\\Settings\\jeux-parameter.csv";

#if UNITY_EDITOR
        path = $"{Path.GetFullPath(Path.Combine(Application.dataPath, @"../../../../../"))}Documents\\Capteur\\data\\Settings\\jeux-parameter.csv";
#endif

        return path;
    }

    public static void SaveDataToCsv(List<ValuePresetData> data)
    {
        using (StreamWriter writer = new StreamWriter(GetPath()))
        {
            writer.WriteLine("ID;Easy;Normal;Hard");

            foreach (var row in data)
            {
                writer.WriteLine($"{row.Id};{row.Easy};{row.Normal};{row.Hard}");
            }
        }

        //Debug.Log($"Donnees sauvegardees en CSV a : {GetPath()}");
    }


    public static List<ValuePresetData> LoadDataFromCsv()
    {
        List<ValuePresetData> data = new List<ValuePresetData>();

        if (!File.Exists(GetPath()))
        {
            Debug.LogError($"Fichier introuvable : {GetPath()}");
            return data;
        }

        string[] lines = File.ReadAllLines(GetPath());

        for (int i = 1; i < lines.Length; i++)
        {
            string line = lines[i];
            if (string.IsNullOrWhiteSpace(line)) continue;

            string[] values = line.Split(';');
            if (values.Length < 4) continue;

            ValuePresetData row = new ValuePresetData
            {
                Id = values[0],
                Easy = values[1],
                Normal = values[2],
                Hard = values[3]
            };

            data.Add(row);
        }

        //Debug.Log($"Données chargées depuis : {GetPath()}");
        return data;
    }

    public static void AddOrUpdateRow(List<ValuePresetData> data, ValuePresetData newRow)
    {
        for (int i = 0; i < data.Count; i++)
        {
            if (data[i].Id == newRow.Id)
            {
                data[i].Easy = newRow.Easy;
                data[i].Normal = newRow.Normal;
                data[i].Hard = newRow.Hard;
                //Debug.Log($"Ligne mise a jour pour ID = {newRow.Id}");
                SaveDataToCsv(data);
                return;
            }
        }

        data.Add(newRow);
        //Debug.Log($"Nouvelle ligne ajoutee pour ID = {newRow.Id}");
        SaveDataToCsv(data);
    }

    public static void AddOrUpdateRow<T1, T2, T3>(List<ValuePresetData> data, string id, T1 easy, T2 normal, T3 hard)
    {
        ValuePresetData row = new ValuePresetData()
        {
            Id = id,
            Easy = ToStringValue(easy),
            Normal = ToStringValue(normal),
            Hard = ToStringValue(hard)
        };

        AddOrUpdateRow(data, row);
    }

    public static T GetValue<T>(string value)
    {
        try
        {
            if (typeof(T) == typeof(int))
                return (T)(object)int.Parse(value);

            if (typeof(T) == typeof(float))
                return (T)(object)float.Parse(value, System.Globalization.CultureInfo.InvariantCulture);

            if (typeof(T) == typeof(bool))
                return (T)(object)bool.Parse(value);

            return (T)(object)value;
        }
        catch
        {
            return default;
        }
    }

    public static string ToStringValue<T>(T value)
    {
        if (value == null) return "";

        if (typeof(T) == typeof(float))
            return ((float)(object)value).ToString(System.Globalization.CultureInfo.InvariantCulture);

        return value.ToString();
    }
}

[System.Serializable]
public class ValuePresetData
{
    public string Id;
    public string Easy;
    public string Normal;
    public string Hard;
}