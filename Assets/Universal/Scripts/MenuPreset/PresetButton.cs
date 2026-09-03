using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(UnityEngine.UI.Button))]
public class PresetButton : MonoBehaviour
{
    [SerializeField] BasePreset preset;
    [SerializeField] ValuePreset.PresetEnum type;
    UnityEngine.UI.Button button;
    [SerializeField] Image outlineImage;
    private static PresetButton selectedButton = null;
    Image background;
    Color defaultColor;

    public void Start()
    {
        button = GetComponent<UnityEngine.UI.Button>();
        background = GetComponent<Image>();
        defaultColor = background.color;
        button.onClick.AddListener(OnClick);
        if (preset.State == type)
        {

            Select();
            selectedButton = this;
        }
    }
    public void OnClick()
    {

        if (selectedButton != null && selectedButton != this)
        {
            selectedButton.Deselect();
        }
        selectedButton = this;
        Select();
        if (Input.GetKey(KeyCode.LeftControl))
        {
            preset.SavePreset(type);
        }
        else
        {
            preset.ActivePreset(type);
        }
    }
    
    private void Select()
    { 
        outlineImage.enabled = true;
    }

    private void Deselect()
    {
        outlineImage.enabled = false;
    }

    public void Update()
    {
        if (Input.GetKey(KeyCode.LeftControl))
        {
            background.color = Color.grey;
        }
        else
        {
            background.color = defaultColor;
        }
    }
}
