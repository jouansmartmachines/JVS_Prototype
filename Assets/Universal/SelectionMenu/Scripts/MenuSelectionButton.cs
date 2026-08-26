using OSC;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MenuSelection
{
    public class MenuSelectionButton : Universal_Button
    {
        public static MenuSelectionButton Instance
        {
            get
            {
                OnButtonActivated?.Invoke();
                return _instance;
            }
        }
        private static MenuSelectionButton _instance;

        public static Action OnButtonActivated;
        public static Action OnButtonClick;

        public void Awake()
        {
            if (_instance != null)
            {
                Destroy(this.gameObject);
                return;
            }
            _instance = this;

            _event.AddListener(GoToMenuSelection);
            this.gameObject.SetActive(false);
        }

        private void GoToMenuSelection()
        {
            Debug.Log("Go to MenuSelection Scene");
            OSC_Manager.Instance.SendAccueilTous();
            OnButtonClick?.Invoke();
            SceneManager.LoadScene(BuildState.MenuSelectionSceneName);
        }
        
        private void OnDestroy()
        {
            _event.RemoveListener(GoToMenuSelection);

            OnButtonClick = null;
            OnButtonActivated = null;

            if (_instance == this)
                _instance = null;
        }

    }
}