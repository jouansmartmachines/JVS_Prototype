using UnityEngine;
using UnityEngine.UI;

namespace Monstres
{
    public class Monstres_Value_Indication : MonoBehaviour
    {
        public Toggle toggleIndication;

        private void Start()
        {
            if (PlayerPrefs.GetInt("ShowIndication") == 1)
            {
                toggleIndication.isOn = true;
            }
            else
            {
                toggleIndication.isOn = false;
            }
        }

        public void SetIndicationValue()
        {
            int indicationValueInt;

            if (toggleIndication.isOn)
            {
                indicationValueInt = 1;
            }
            else
            {
                indicationValueInt = -1;
            }

            PlayerPrefs.SetInt("ShowIndication", indicationValueInt);
        }
    }
}