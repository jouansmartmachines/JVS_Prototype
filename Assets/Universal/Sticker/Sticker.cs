using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
public class Sticker : MonoBehaviour
{
    [SerializeField] private bool _bintro;
    private string _path;
    [SerializeField] private Image _img;
    [SerializeField] private GameObject[] _gos;
    [SerializeField] string _playerPrefPath;
    [SerializeField] Image parent;

    void Start()
    {
        _img.enabled = false;
        if(parent != null) parent.enabled = false;
        string appPath;
        string newPath;
        appPath = Application.dataPath;
        newPath = Path.GetFullPath(Path.Combine(appPath, @"../../../../"));
        _path = newPath + "Personnalisation\\Logo_Entreprise";

#if UNITY_EDITOR
        _path = $"{Path.GetFullPath(Path.Combine(appPath, @"../../../../../"))}Documents\\Capteur\\Personnalisation\\Logo_Entreprise";
#endif

        Debug.Log(_path + ".png");

        if (PlayerPrefs.HasKey(_playerPrefPath) && _playerPrefPath != string.Empty)
        {
            if(PlayerPrefs.GetInt(_playerPrefPath) == 1)
                StartCoroutine(LoadImage());
        }
        else
        {
            StartCoroutine(LoadImage());
        }
    }

    IEnumerator LoadImage()
    {
        string imagesPath = _path + (_bintro ? @"\stickerAccueilTous.png" : @"\sticker.png");
        if(parent != null) parent.enabled = true;

        using (UnityEngine.Networking.UnityWebRequest uwr = UnityEngine.Networking.UnityWebRequestTexture.GetTexture("file://" + imagesPath))
        {
            yield return uwr.SendWebRequest();

            if (uwr.result != UnityEngine.Networking.UnityWebRequest.Result.Success)
            {
                yield return LoadImageJGP();
            }
            else
            {
                Texture2D tex = UnityEngine.Networking.DownloadHandlerTexture.GetContent(uwr);
                
                // --- ALTERNATIVE : NETTOYAGE DES BORDS ---
                Color32[] pixels = tex.GetPixels32();
                for (int i = 0; i < pixels.Length; i++)
                {
                    // Si le pixel est totalement ou partiellement transparent
                    if (pixels[i].a < 255)
                    {
                        // On force les composants RGB pour éviter le liseré blanc
                        // mais on garde la transparence (alpha) originale.
                        // Cela évite que l'interpolation "tire" du blanc.
                        pixels[i].r = (byte)(pixels[i].r * pixels[i].a / 255);
                        pixels[i].g = (byte)(pixels[i].g * pixels[i].a / 255);
                        pixels[i].b = (byte)(pixels[i].b * pixels[i].a / 255);
                    }
                }
                tex.SetPixels32(pixels);
                
                // Paramètres de texture critiques
                tex.wrapMode = TextureWrapMode.Clamp;
                tex.filterMode = FilterMode.Bilinear; // On peut repasser en Bilinear si nettoyé
                tex.Apply();
                // -----------------------------------------

                _img.sprite = Tool.ToolBox.CreateSpriteFromTexture(tex);
                _img.enabled = true;
            }
        }
    }
    IEnumerator LoadImageJGP()
    {
        string imagesPath;

        if (!_bintro)
        {
            imagesPath = _path + @"\sticker.jpg";
        }
        else
        {
            imagesPath =  _path + @"\stickerAccueilTous.jpg";
        }

        Texture2D tex;
        tex = new Texture2D(2, 2);
        WWW www = new WWW(imagesPath);

        //Debug.Log(imagesPath);
        while (!www.isDone)
            yield return null;
        if (www.error != null)
        {
            Debug.LogError("Image WWW ERROR: " + www.error);

            foreach (GameObject go in _gos)
            {
                go.SetActive(false);
            }

        }
        else
        {

            www.LoadImageIntoTexture(tex);

            //_img.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
            _img.sprite = Tool.ToolBox.CreateSpriteFromTexture(tex);
            _img.enabled = true;
        }
    }
}
