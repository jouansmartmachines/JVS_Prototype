using UnityEngine;
using System.IO;
using System.Collections.Generic;
using UnityEngine.UI;

namespace Demolition
{
    /// <summary>
    /// Charge des données externes (images/texte) depuis un dossier personnalisé.
    /// Pattern Monstres : si des fichiers sont présents dans le dossier, ils remplacent
    /// les textures générées par l'Editor script. Sinon, fallback sur Resources.Load().
    /// </summary>
    public class Demolition_DataLoader : MonoBehaviour
    {
        [Header("Dossier personnalisé")]
        public string customFolderName = "Demolition";
        public bool useCustomFolder = false;

        [Header("Images chargées")]
        public Sprite customBoisSprite;
        public Sprite customVerreSprite;
        public Sprite customPierreSprite;
        public Sprite customOiseauSprite;
        public Sprite customImpactSprite;
        public Sprite[] customDebrisSprites;

        private string folderPath;

        void Awake()
        {
            if (!useCustomFolder) return;

            // Construire le chemin : au-dessus de Assets/
            string appPath = Application.dataPath;
            folderPath = Path.GetFullPath(Path.Combine(appPath, @"../../../../Personnalisation/" + customFolderName + "/Textures/"));

            if (!Directory.Exists(folderPath))
            {
                Debug.Log("Demolition_DataLoader: dossier personnalisé non trouvé -> " + folderPath);
                return;
            }

            Debug.Log("Demolition_DataLoader: chargement depuis " + folderPath);
            LoadTextures();
        }

        void LoadTextures()
        {
            DirectoryInfo dir = new DirectoryInfo(folderPath);
            if (!dir.Exists) return;

            FileInfo[] files = dir.GetFiles("*.*");
            if (files.Length == 0) return;

            // Parcourir les fichiers et les assigner par nom
            foreach (FileInfo file in files)
            {
                string name = Path.GetFileNameWithoutExtension(file.Name).ToLower();
                string ext = file.Extension.ToLower();

                if (ext != ".png" && ext != ".jpg" && ext != ".jpeg") continue;

                Texture2D tex = new Texture2D(2, 2);
                WWW www = new WWW(file.FullName);
                if (www.error != null)
                {
                    Debug.LogWarning("Erreur chargement " + file.Name + ": " + www.error);
                    continue;
                }
                www.LoadImageIntoTexture(tex);

                Sprite sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
                sprite.name = name;

                switch (name)
                {
                    case "bois":
                    case "wood":
                        customBoisSprite = sprite;
                        break;
                    case "verre":
                    case "glass":
                        customVerreSprite = sprite;
                        break;
                    case "pierre":
                    case "stone":
                        customPierreSprite = sprite;
                        break;
                    case "oiseau":
                    case "bird":
                        customOiseauSprite = sprite;
                        break;
                    case "impact":
                        customImpactSprite = sprite;
                        break;
                    case "debris_bois":
                    case "debris_wood":
                        AddDebris(sprite);
                        break;
                    case "debris_verre":
                    case "debris_glass":
                        AddDebris(sprite);
                        break;
                    case "debris_pierre":
                    case "debris_stone":
                        AddDebris(sprite);
                        break;
                }
            }
        }

        void AddDebris(Sprite sprite)
        {
            if (customDebrisSprites == null)
                customDebrisSprites = new Sprite[3];
            for (int i = 0; i < customDebrisSprites.Length; i++)
            {
                if (customDebrisSprites[i] == null)
                {
                    customDebrisSprites[i] = sprite;
                    return;
                }
            }
        }

        /// <summary>
        /// Si un dossier texte est présent, lire les fichiers JSON/CSV
        /// </summary>
        public string[] LoadTextFile(string fileName)
        {
            string textPath = Path.Combine(
                Path.GetDirectoryName(folderPath), "Text/" + fileName);

            if (!File.Exists(textPath))
            {
                Debug.Log("Fichier texte non trouvé: " + textPath);
                return null;
            }

            return File.ReadAllLines(textPath);
        }
    }
}
