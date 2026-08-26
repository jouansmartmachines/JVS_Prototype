using UnityEngine;
using TMPro;

public class LocalizedText : MonoBehaviour
{

    public string[] key;
    //"{0} {1} !"
    public string sentenceFormat ;

    private TMP_Text _textComponent;

    private void Awake()
    {
        _textComponent = GetComponent<TMP_Text>();
    }

    private void OnEnable()
    {
        UpdateText();
    }

    public void UpdateText()
    {
 
        if (string.IsNullOrEmpty(sentenceFormat))
        {
            string result = Localizer.Get(key[0]);
            _textComponent.text = ProcessLineBreaks(result);
            return;
        }

        string[] translatedValues = new string[key.Length];

        for (int i = 0; i < key.Length; i++)
        {
            if (!string.IsNullOrEmpty(key[i]))
            {
                translatedValues[i] = ProcessLineBreaks(Localizer.Get(key[i]));
            }
            else
            {
                translatedValues[i] = "";
            }
        }
        string finalFormat = ProcessLineBreaks(sentenceFormat);
        _textComponent.text = string.Format(finalFormat, translatedValues);
    }

    private string ProcessLineBreaks(string input)
    {
        if (string.IsNullOrEmpty(input)) return "";
        return input.Replace("\\n", "\n");
    }
}