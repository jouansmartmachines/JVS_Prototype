using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SliderValueText : MonoBehaviour
{
    [SerializeField] TMP_Text textRef; 
    [SerializeField] Slider slider;
    [SerializeField] string unit = "";
    [SerializeField] int digits = 0;

    void Start()
    {
        if (!textRef) textRef = GetComponent<TMP_Text>();
        if (!slider)  slider  = GetComponentInParent<Slider>();

        slider.onValueChanged.AddListener(UpdateText);
        UpdateText(slider.value);
    }

    void UpdateText(float v)
    {
        textRef.text = v.ToString("F" + digits) + unit;
    }
}
