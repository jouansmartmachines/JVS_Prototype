using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public enum Language
{
    Français =0,
    Anglais = 1,
    Espagnol = 2,
    Catalan = 3
}


public static class Localizer
{
    private static Dictionary<string, Dictionary<Language, string>> _localizationData;
    public static Language currentLanguage = Language.Anglais;

    // CORRECTION : "Localisation" au lieu de "Localisatin"
    
    private static readonly string FILE_PATH = System.IO.Path.Combine(Application.dataPath, "Universal", "Localisation", "Langues_Jeux.tsv");

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void AutoInitialize()
    {
        string content = "";

        TextAsset tsvFile = Resources.Load<TextAsset>("Langues_Jeux");

        /*
        if (File.Exists(FILE_PATH))
        {
            content = File.ReadAllText(FILE_PATH);
            Debug.Log($"<color=green>[Localizer] Chargé depuis le disque : {FILE_PATH}</color>");
        }
        else
        {
            // CORRECTION : Vérifie que le nom correspond exactement au fichier dans Assets/Resources
            // Si ton fichier s'appelle "Langues_Jeux.tsv", mets juste "Langues_Jeux"
            TextAsset tsvFile = Resources.Load<TextAsset>("Langues_Jeux"); 
            
            if (tsvFile != null)
            {
                content = tsvFile.text;
                Debug.Log("<color=yellow>[Localizer] Chargé depuis Resources.</color>");
            }
        }

        
        */

        if (tsvFile != null)
        {
            Debug.Log("<color=green>[Localizer] Fichier de langue chargé avec succès !</color>");
            ParseTSV(tsvFile.text);
        }
        else
        {
            Debug.LogError("<color=red>[Localizer] ERREUR : Le fichier 'Langues_Jeux' est introuvable dans vos dossiers Resources !</color>");
            // Initialisation d'un dictionnaire vide pour éviter les crashs de l'UI
            _localizationData = new Dictionary<string, Dictionary<Language, string>>();
        }

        /*

        if (!string.IsNullOrEmpty(content))
        {
            ParseTSV(content);
        }
        else
        {
            Debug.LogError($"<color=red>[Localizer] Fichier introuvable à : {FILE_PATH} ou dans Resources</color>");
        }
        */
    }

    public static string Get(string key)
    {
        // Etape A : Vérifier la langue
        //Debug.Log($"[DEBUG] Langue actuelle : '{currentLanguage}'");
        if (_localizationData == null)
        {
            Debug.LogError($"[Localizer] CRITIQUE : _localizationData est NULL lors de l'appel de Get(\"{key}\"). Tentative de secours...</color>");
            AutoInitialize(); // Tentative de secours de dernière minute
        }
        else if (_localizationData.Count == 0)
        {
            Debug.LogWarning($"[Localizer] ALERTE : _localizationData est VIDE (0 entrées) lors de l'appel de Get(\"{key}\").</color>");
            AutoInitialize(); 
        }
        else
        {
            // Log de succès indiquant la santé du dictionnaire
            //Debug.Log($"[DEBUG] Appel à Get(\"{key}\"). Le dictionnaire contient {_localizationData.Count} clés prêtes.");
        }


        if (_localizationData.TryGetValue(key, out var translations))
        {

            if (translations.TryGetValue(currentLanguage, out string text))
            {
                // Etape C : Vérifier le contenu
                //Debug.Log($"[DEBUG] Valeur trouvée : '{text}'");
                return text;
            }
        }
        return "MISSING";
    }
    private static void ParseTSV(string tsvContent)
    {
        _localizationData = new Dictionary<string, Dictionary<Language, string>>();
        //  Debug.Log("[Localzer] ParseTSV");

        // Split par ligne
        string[] lines = tsvContent.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);

        foreach (string line in lines)
        {
            string[] cells = line.Split('\t');
            
            // On vérifie qu'on a bien la colonne Clé (1) + Fr(2), En(3), Es(4), Ca(5)
            if (cells.Length < 2) continue;

            string key = cells[1].Trim();
            if (string.IsNullOrEmpty(key) || key.ToLower() == "clef" || key.ToLower() == "key") continue;

            var trans = new Dictionary<Language, string>();

            TryAdd(trans, Language.Français, cells, 3);
            TryAdd(trans, Language.Anglais, cells, 4);
            TryAdd(trans, Language.Espagnol, cells, 5);
            TryAdd(trans, Language.Catalan, cells, 6);

            _localizationData[key] = trans;
        }
        Debug.Log($"[Localizer] {_localizationData.Count} entrées chargées.");
    }

    private static void TryAdd(Dictionary<Language, string> dict, Language lang, string[] cells, int index)
    {
        if (index < cells.Length && !string.IsNullOrWhiteSpace(cells[index]))
            dict[lang] = cells[index].Trim();
        else
            dict[lang] = "MISSING";
    }

    
}
