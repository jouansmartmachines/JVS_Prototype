using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace Basket
{
    public class Basket_TimerManager : MonoBehaviour
    {
        public static Basket_TimerManager i;
        private void Awake()
        {
            if (i != null)
            {
                Destroy(gameObject);
                return;
            }

            i = this;
        }

        [SerializeField] TextMeshProUGUI _minutes;
        [SerializeField] TextMeshProUGUI _secondes;
        public UnityEvent OnTimerEnd;
        public UnityEvent<int> OnUpdateTimer;
        int _timer;

        public void Start()
        {
            int time = 60;
            if (PlayerPrefs.HasKey(Basket_GeneralVariable.TimerKey))
                time = 60 + (PlayerPrefs.GetInt(Basket_GeneralVariable.TimerKey) * 30);
            ShowTimer(time);
            StartCoroutine(LauchTimer(time));
        }

        public IEnumerator LauchTimer(int initialTimer)
        {
            _timer = initialTimer;

            for (int i = 0; i < initialTimer; i++)
            {
                yield return new WaitForSeconds(1f);
                _timer--;
                OnUpdateTimer?.Invoke(_timer);
                ShowTimer(_timer);
            }

            OnTimerEnd?.Invoke();
        }

        private void ShowTimer(int timer)
        {
            int seconds = timer % 60;
            _secondes.text = seconds.ToString();
            int minutes = timer / 60;
            _minutes.text = minutes.ToString();
        }
    }
}