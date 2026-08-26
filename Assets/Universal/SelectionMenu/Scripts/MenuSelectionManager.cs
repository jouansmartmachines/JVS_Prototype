using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using RGSMS.Scene;
using System.IO;
using System;
using System.Linq;
using Tool;

namespace MenuSelection
{
    public class MenuSelectionManager : MonoBehaviour
    {
        [SerializeField] SelectionMenuOption _option;
        [SerializeField] ButtonMenuSelection _buttonPrefab;
        private List<ButtonMenuSelection> _buttons = new();

        [SerializeField] Transform _holder;
        [SerializeField] ButtonHolder _buttonHolderPrefab;
        private List<ButtonHolder> _buttonHolderList = new();

        [SerializeField] List<string> _disableGameList = new();

        [System.Serializable]
        public class GameButtonData
        {
            public string name;
            public Sprite logo;
            public string scene;
            public bool isDisable = false;
        }

        public List<GameButtonData> GameData => _buttonDatas;
        /*[SerializeField] */
        List<GameButtonData> _buttonDatas = new();
        private string path;
        public static int LastPanel
        {
            get
            {
                return PlayerPrefs.GetInt("MenuSelection_LastPanel", 0);
            }

            set
            {
                PlayerPrefs.SetInt("MenuSelection_LastPanel", value);
            }
        }

        public void Start()
        {
            string newPath = Path.GetFullPath(Path.Combine(Application.dataPath, @"../../../../"));
            path = newPath + "data\\Images_Jeux_JVS\\Logos";
            Debug.Log(path);
#if UNITY_EDITOR
            path = $"{Path.GetFullPath(Path.Combine(Application.dataPath, @"../../../../../"))}Documents\\Capteur\\data\\Images_Jeux_JVS\\Logos";
#endif
            //Debug.Log(path);

            var paths = Tool.ToolBox.GetFiles(path, "*.png");
            _buttonDatas.Clear();
            foreach (var p in paths)
            {
                var gameName = Path.GetFileName(p).Replace("&", "_");
                gameName = gameName.Remove(gameName.Length - 4);

                if (gameName.Length > 4)
                    gameName = gameName.Substring(4); 
                //Debug.Log(gameName);
                var sprite = Tool.ToolBox.CreateSpriteFromPath(p);
                GameButtonData data = new()
                {
                    name = gameName,
                    logo = sprite,
                    scene = $"Accueil_{gameName}",
                    isDisable = _disableGameList.Contains(gameName)
                };
                _buttonDatas.Add(data);
            }

            SetUpGameButton(_buttonDatas);
            _option.SetUpAllButton();
        }

        public void SetUpGameButton(List<GameButtonData> gameSelected)
        {
            for (int i = 0; i < _buttons.Count; i++)
            {
                Destroy(_buttons[i].gameObject);
            }
            _buttons.Clear();

            for (int i = 0; i < _buttonHolderList.Count; i++)
            {
                Destroy(_buttonHolderList[i].gameObject);
            }
            _buttonHolderList.Clear();
            _holder.position = new(960, 540);

            int holderCount = -1;
            for (int i = 0; i < gameSelected.Count; i++)
            {
                ButtonHolder holder = null;
                if (i / 6 != holderCount)
                {
                    holderCount++;
                    holder = Instantiate(_buttonHolderPrefab, _holder);
                    holder.transform.position = new((1920 * holderCount) + 960, 540);

                    bool right = false;
                    if (i / 6 < (gameSelected.Count - 1) / 6) right = true;
                    bool left = false;
                    if (i / 6 > 0) left = true;
                    //Debug.Log((i / 6) + " : " + ((gameSelected.Count - 1) / 6) + " | " + right + " | " + left);
                    holder.SetUpButton(right, left, _holder);
                    _buttonHolderList.Add(holder);
                }
                else
                {
                    holder = _buttonHolderList.Last();
                }

                CreateButton(gameSelected[i], holder.transform, holderCount);
            }

            _holder.localPosition = new((-1920 * LastPanel), 0);
            (_holder as RectTransform).RebuildLayout(true);
        }

        private void CreateButton(GameButtonData data, Transform holder, int panelId)
        {
            var button = Instantiate(_buttonPrefab, holder);
            button.PanelId = panelId;
            button.SetData(data);
            _buttons.Add(button);
        }
    }
}