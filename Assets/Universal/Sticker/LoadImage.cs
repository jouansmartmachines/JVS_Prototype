using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class LoadImage : MonoBehaviour
{
    Image _image;
    [SerializeField] string _path = "Personnalisation";
    private string CompletePath { get; set; }

    public void Start()
    {
        _image = GetComponent<Image>();

        string appPath;
        string newPath;
        appPath = Application.dataPath;
        newPath = Path.GetFullPath(Path.Combine(appPath, @"../../../../"));
        CompletePath = newPath + _path;

#if UNITY_EDITOR
        CompletePath = $"{Path.GetFullPath(Path.Combine(appPath, @"../../../../../"))}Documents\\Capteur\\{_path}";
#endif
        Debug.Log(CompletePath);

        var sprite = Tool.ToolBox.CreateSpriteFromPath(CompletePath);
        //Debug.Log(sprite != null);
        if(sprite != null)
        {
            _image.sprite = sprite;
        }
    }
}
