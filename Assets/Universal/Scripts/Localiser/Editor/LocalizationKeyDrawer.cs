using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System;
using System.Text;

[CustomPropertyDrawer(typeof(LocalizationKeyAttribute))]
public class LocalizationKeyDrawer : PropertyDrawer
{
    private List<string> _keysCache;
    private float _lastReadTime;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // Indispensable pour que le menu contextuel (clic droit) et le prefab override fonctionnent
        label = EditorGUI.BeginProperty(position, label, property);

        if (_keysCache == null || Time.realtimeSinceStartup > _lastReadTime + 3f)
        {
            _keysCache = ReadKeysSafe();
            _lastReadTime = Time.realtimeSinceStartup;
        }

        if (_keysCache != null && _keysCache.Count > 0)
        {
            int currentIndex = _keysCache.IndexOf(property.stringValue);
            if (currentIndex == -1) currentIndex = 0;

            // On vérifie si l'utilisateur change la valeur
            EditorGUI.BeginChangeCheck();
            
            currentIndex = EditorGUI.Popup(position, label.text, currentIndex, _keysCache.ToArray());
            
            if (EditorGUI.EndChangeCheck())
            {
                // Applique la modification à la SerializedProperty
                property.stringValue = _keysCache[currentIndex];
            }
        }
        else
        {
            EditorGUI.PropertyField(position, property, label);
        }

        EditorGUI.EndProperty();
    }
    
    private List<string> ReadKeysSafe()
    {
        List<string> keys = new List<string>();
        string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                                    "Capteur", "Personnalisation", "Localisation", "Langues_Jeux.tsv");

        if (!File.Exists(path))
        {
            Debug.LogWarning("Fichier TSV introuvable : " + path);
            return keys;
        }

        try
        {
            using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (StreamReader sr = new StreamReader(fs, Encoding.GetEncoding("iso-8859-1")))
            {
                // Lire la ligne de headers
                string firstLine = sr.ReadLine();
                if (string.IsNullOrEmpty(firstLine)) return keys;

                // ✅ Séparateur TAB
                string[] headers = firstLine.Split('\t');

                int keyIndex = Array.FindIndex(headers, h => h.Trim().Equals("clef", StringComparison.OrdinalIgnoreCase));

                if (keyIndex < 0)
                {
                    Debug.LogWarning("Colonne 'clef' introuvable. Headers détectés : " + string.Join(" | ", headers));
                    return keys;
                }

                while (!sr.EndOfStream)
                {
                    string line = sr.ReadLine();

                    // Ignorer lignes vides
                    if (string.IsNullOrWhiteSpace(line)) continue;

                    // ✅ Séparateur TAB
                    string[] row = line.Split('\t');

                    if (row.Length > keyIndex)
                    {
                        string key = row[keyIndex].Trim();
                        if (!string.IsNullOrWhiteSpace(key))
                            keys.Add(key);
                    }
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning("Erreur lecture TSV : " + e.Message);
        }

        // Debug temporaire — retirez ces lignes une fois validé
        Debug.Log($"=== CLEFS TROUVÉES ({keys.Count}) ===\n" + string.Join("\n", keys));

        return keys;
    }
}