using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using OSC;

namespace Monstres
{
    public class Monstres_SetupPictures : MonoBehaviour
    {
        [Header("Data")]
        public string folderPath;
        private int pictureIdx = 0;

        private Sprite firstPicture;
        private Sprite secondPicture;

        [Header("UI")]
        public List<Image> pictureConfirmedDisplayers;
        public List<GameObject> questionMarkPicture;

        void Awake()
        {
            string appPath = Application.dataPath;
            string newPath = Path.GetFullPath(Path.Combine(appPath, @"../../../../"));
            folderPath = newPath + "Personnalisation\\Monstres";
            Debug.Log(folderPath);
#if UNITY_EDITOR
            folderPath = $"{Path.GetFullPath(Path.Combine(appPath, @"../../../../../"))}Documents\\Capteur\\Personnalisation\\Monstres";
            #endif
            Debug.Log(folderPath);


            //if (PlayerPrefs.HasKey(Monstres_GeneralVariables.UseDefaultPictureKEY))
            //    if(PlayerPrefs.GetInt(Monstres_GeneralVariables.UseDefaultPictureKEY) == 1)
            //        folderPath += "\\default";

            SetupPicture();
        }

        public void SetupPicture()
        {
            DirectoryInfo directoryInf = new DirectoryInfo(folderPath);
            if (directoryInf.Exists)
            {
                List<FileInfo> filesInFolder = new List<FileInfo>(directoryInf.GetFiles());

                if (filesInFolder.Count <= 0)
                {
                    return;
                }

                filesInFolder.Sort((x, y) => y.CreationTime.CompareTo(x.CreationTime)); // sort by creation date
                //filesInFolder.RemoveAll(x => x.Extension != ".PNG" && x.Extension != ".png"); // remove all the non jpg image

                if (filesInFolder.Count <= 0)
                {
                    return;
                }

                string pictureFullPath = filesInFolder[0].FullName;
                string pictureFullPath2 = filesInFolder[1].FullName;

                Texture2D tex;
                Texture2D tex2;
                tex = new Texture2D(2, 2);
                tex2 = new Texture2D(2, 2);
                WWW www = new WWW(pictureFullPath);
                WWW www2 = new WWW(pictureFullPath2);
                tex.name = pictureFullPath; // name the tex
                tex2.name = pictureFullPath2; // name the tex

                if (www.error != null)
                {
                    Debug.Log(folderPath);
                    Debug.Log("Image WWW ERROR: " + www.error);
                }
                else
                {
                    www.LoadImageIntoTexture(tex);
                    www2.LoadImageIntoTexture(tex2);
                    firstPicture = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
                    secondPicture = Sprite.Create(tex2, new Rect(0, 0, tex2.width, tex2.height), new Vector2(0.5f, 0.5f));
                    firstPicture.name = "PortraitG";
                    secondPicture.name = "PortraitD";
                    pictureConfirmedDisplayers[pictureIdx].sprite = firstPicture;
                    pictureIdx++;
                    pictureConfirmedDisplayers[pictureIdx].sprite = secondPicture;
                    questionMarkPicture[pictureIdx].SetActive(false);



                    if (pictureIdx < pictureConfirmedDisplayers.Count - 1)
                    {
                        pictureIdx++;
                    }
                    else
                    {
                        pictureIdx = 0;
                    }
                }
            }
        }
    }
}