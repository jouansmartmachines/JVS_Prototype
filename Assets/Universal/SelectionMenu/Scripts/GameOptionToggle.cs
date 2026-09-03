using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace MenuSelection
{
    public class GameOptionToggle : MonoBehaviour
    {
        public bool IsOn
        {
            get { return _toggle.isOn; }
            set { _toggle.SetIsOnWithoutNotify(value); }
        }
        public Toggle.ToggleEvent Event => _toggle.onValueChanged;
        [SerializeField] Toggle _toggle;
        [SerializeField] TextMeshProUGUI _text;
        public MenuSelectionManager.GameButtonData Data { get; set; }

        public void SetUp(MenuSelectionManager.GameButtonData data)
        {
            Data = data;
            _text.text = Data.name;
        }
    }
}