using OSC;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace MenuSelection
{
    public class ButtonMenuSelection : MonoBehaviour
    {
        Universal_Button _button;
        MenuSelectionManager.GameButtonData _gameData;
        [SerializeField] Image _image;
        [SerializeField] GameObject _disableBanner;
        public int PanelId { get; set; }
        public static bool IsLoadingScene = false;

        public IEnumerator Start()
        {
            _button = GetComponent<Universal_Button>();
            _button.Event.AddListener(OnPress);

            yield return new WaitForSeconds(0.25f);

            IsLoadingScene = false;
        }

        private void OnPress()
        {
            if (ButtonHolder.isMoving || IsLoadingScene) return;
            StartCoroutine(OnPressCoroutine());
        }

        private IEnumerator OnPressCoroutine()
        {
            MenuSelectionManager.LastPanel = PanelId;
            IsLoadingScene = true;
            yield return null;
            //yield return new WaitForSeconds(0.25f);
            LoadingManager.LoadScene(_gameData.scene);
        }

        public void SetData(MenuSelectionManager.GameButtonData data)
        {
            _gameData = data;
            _image.sprite = data.logo;
            if (data.isDisable)
            {
                _disableBanner.SetActive(true);
                GetComponent<Universal_Button>().Event.RemoveAllListeners();
                GetComponent<Universal_Button>().IsActive = false;
            }
        }
    }
}