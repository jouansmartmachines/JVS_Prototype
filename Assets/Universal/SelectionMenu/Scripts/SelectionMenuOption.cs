using System.Collections;
using System.Collections.Generic;
using Tool;
using UnityEngine;

namespace MenuSelection
{
    public class SelectionMenuOption : MonoBehaviour
    {
        [SerializeField] MenuSelectionManager _menuSelectionManager;
        [SerializeField] GameOptionToggle _prefab;
        [SerializeField] Transform _holder;
        [SerializeField] GameObject _optionObject;
        private List<GameOptionToggle> _toggle = new();

        public void Start()
        {
            //SetUpAllButton();
            //GetSaveData();
        }

        public void SetUpAllButton()
        {
            for (int i = 0; i < _toggle.Count; i++)
            {
                Destroy(_toggle[i].gameObject);
            }
            _toggle.Clear();

            foreach (MenuSelectionManager.GameButtonData data in _menuSelectionManager.GameData)
            {
                var button = Instantiate(_prefab, _holder);
                button.SetUp(data);
                button.Event.AddListener(UpdateButton);
                _toggle.Add(button);
            }

            GetSaveData();
        }

        public void Update()
        {
            if (Input.GetKeyDown(KeyCode.O))
            {
                _optionObject.SetActive(!_optionObject.activeSelf);
                (_optionObject.transform as RectTransform).RebuildLayout(true);
            }
        }

        public void UpdateButton(bool b)
        {
            List<MenuSelectionManager.GameButtonData> list = new List<MenuSelectionManager.GameButtonData>();
            for (int i = 0; i < _toggle.Count; i++)
            {
                PlayerPrefs.SetInt($"GameSelection_{i}", _toggle[i].IsOn ? 1 : 0);
                if (_toggle[i].IsOn)
                {
                    list.Add(_menuSelectionManager.GameData[i]);
                }
            }
            _menuSelectionManager.SetUpGameButton(list);
        }

        private void GetSaveData()
        {
            for (int i = 0; i < _toggle.Count; i++)
            {
                if (!PlayerPrefs.HasKey($"GameSelection_{i}")) PlayerPrefs.SetInt($"GameSelection_{i}", 1);
                _toggle[i].IsOn = PlayerPrefs.GetInt($"GameSelection_{i}") == 1;
            }
            UpdateButton(true);
        }
    }
}