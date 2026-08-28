using UnityEngine;
using TMPro;

namespace Monstres
{
    public class Monstres_Value_OSCOutIP : MonoBehaviour
    {
        public TextMeshProUGUI oscOutIndication;
        // Start is called before the first frame update
        void Start()
        {
            oscOutIndication.text = PlayerPrefs.GetString("OSCOutIp");
        }

        public void RegisterOutIP()
        {
            PlayerPrefs.SetString("OSCOutIp", oscOutIndication.text);
        }
    }
}
