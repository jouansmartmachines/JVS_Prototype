using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace Monstres1
{
    public class Monstres_Sliders : MonoBehaviour
    {
        public string saveData;
        public TextMeshProUGUI valorIndication;
        public bool b_roundToUnit = false;
        public bool pixelValue = false;
        public bool percentageValue = false;
        public bool roundToHundred = false;
        public string unitIndication;

        private void Start()
        {
            GetComponent<Slider>().value = PlayerPrefs.GetFloat(saveData);
            ShowIndication();

            Universal_GeneralVariables.OnPlayerPrefs += SetData;
        }

        public void OnDestroy()
        {
            Universal_GeneralVariables.OnPlayerPrefs -= SetData;
        }

        public void SaveData()
        {
            if (!b_roundToUnit)
            {
                GetComponent<Slider>().value = Mathf.Round(GetComponent<Slider>().value * 100) / 100f;
            }
            else
            {
                GetComponent<Slider>().value = Mathf.Round(GetComponent<Slider>().value);
            }

            if (roundToHundred)
            {
                GetComponent<Slider>().value = Mathf.Round(GetComponent<Slider>().value / 100) * 100f;
            }


            PlayerPrefs.SetFloat(saveData, GetComponent<Slider>().value);
            ShowIndication();
        }

        void SetData()
        {
            if (PlayerPrefs.HasKey(saveData))
                GetComponent<Slider>().value = PlayerPrefs.GetFloat(saveData);
        }

        void ShowIndication()
        {
            if (pixelValue)
            {
                float pixelValue = GetComponent<Slider>().value * 100f;
                valorIndication.text = pixelValue.ToString("") + unitIndication;
                return;
            }

            if (percentageValue)
            {
                valorIndication.text = Mathf.Round(GetComponent<Slider>().value / GetComponent<Slider>().maxValue * 100).ToString() + unitIndication;
                return;
            }

            valorIndication.text = GetComponent<Slider>().value.ToString("") + unitIndication;
        }
    }
}