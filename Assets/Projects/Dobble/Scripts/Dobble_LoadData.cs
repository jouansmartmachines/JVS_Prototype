using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using Tool;
using UnityEngine.UI;

namespace Dobble
{
    public class Dobble_LoadData : MonoBehaviour
    {
        [SerializeField] private Dobble_ButtonLinked _buttonLinkedPrefab;
        [SerializeField] private RectTransform _canvasTransform;

        public List<Dobble_ButtonLinked> LoadPersonnalisationData(string teamFolderName)
        {
            List<Dobble_ButtonLinked> buttons = new List<Dobble_ButtonLinked>();
            string basePath;

#if UNITY_EDITOR
            basePath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "Capteur", "Personnalisation", "Mo&Mat", "Mots", "Animaux"
            );
#else
            string appPath = Application.dataPath;
            basePath = Path.GetFullPath(Path.Combine(appPath, @"../../../../Personnalisation/Pareil"));
#endif

            string teamPath = Path.Combine(basePath, teamFolderName);

            if (!Directory.Exists(teamPath))
            {
                Debug.LogError($"❌ Dossier inexistant pour l'équipe : {teamPath}");
                return buttons;
            }

            List<string> imagesPaths = ToolBox.GetFiles(teamPath, new string[]
            {
                "*.jpg", "*.png", "*.bmp", "*.exr", "*.gif", "*.hdr", "*.iff",
                "*.jpeg", "*.pct", "*.pic", "*.pict", "*.psd", "*.tga", "*.tif", "*.tiff"
            }).ToList();

            if (imagesPaths.Count == 0)
            {
                Debug.LogWarning($"⚠️ Aucun fichier image trouvé dans : {teamPath}");
                return buttons;
            }

            foreach (var path in imagesPaths)
            {

                Sprite sprite = ToolBox.CreateSpriteFromPath(path, false);

                var button = Instantiate(_buttonLinkedPrefab,_canvasTransform);
                button.gameObject.SetActive(true);
                button._rightSprite = sprite;


                Image img = button.GetComponent<Image>();
                img.sprite = sprite;

                RectTransform rt = button.GetComponent<RectTransform>();
                if (rt != null)
                    rt.sizeDelta = new Vector2(sprite.rect.width, sprite.rect.height);
                button.name = Path.GetFileNameWithoutExtension(path);
                button.buttonName = Path.GetFileNameWithoutExtension(path);
                buttons.Add(button);

            }


            return buttons;
        }

        public List<string> GetAvailableTeamFolders()
        {
#if UNITY_EDITOR
            string basePath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "JVS", "Personnalisation", "Mo&Mat", "Mots", "Animaux"
            );
#else
            string appPath = Application.dataPath;
            string basePath = Path.GetFullPath(Path.Combine(appPath, @"../../../../Personnalisation/Dobble"));
#endif

            if (!Directory.Exists(basePath))
            {
                Debug.LogError($"❌ Dossier de base introuvable : {basePath}");
                return new List<string>();
            }

            return Directory.GetDirectories(basePath)
                            .Select(Path.GetFileName)
                            .ToList();
        }
    }
}
