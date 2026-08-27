using System.Collections;
using System.Collections.Generic;
using Tool;
using UnityEngine;

namespace Basket
{
    public class ShowTimerOnHoop : MonoBehaviour
    {
        [System.Serializable]
        public struct Cache
        {
            public GameObject[] array;
        }

        [SerializeField] List<Cache> _caches;

        readonly private Dictionary<int, int> realNumber = new Dictionary<int, int>()
        {
            { 0, 125 },
            { 1, 96 },
            { 2, 55 },
            { 3, 103 },
            { 4, 106 },
            { 5, 79 },
            { 6, 95 },
            { 7, 100 },
            { 8, 127 },
            { 9, 111 },
        };

        int[] binaire =
        {
            1, 
            2,
            4,
            8,
            16,
            32,
            64
        };

        private void DisplayNumber(int number, Cache cache)
        {
            var rn = realNumber[number];

            List<int> code = new List<int>();
            List<int> total = new List<int>();
            for (int i = binaire.Length - 1; i >= 0; i--)
            {
                var div = rn / ((binaire[i] + total.Total()) * 1f);
                if (div >= 1)
                {
                    code.Add(i);
                    total.Add(binaire[i]);
                    if(div == 1) break;
                }
            }

            for (int i = 0; i < cache.array.Length; i++)
            {
                if (!code.Contains(i))
                    cache.array[i].SetActive(true);
                else
                    cache.array[i].SetActive(false);
            }

            //Debug.Log($"{number} => {rn} | Final Number : {total.Total()}");
        }

        private void Start()
        {
            Basket_TimerManager.i.OnUpdateTimer.AddListener(DisplayTimer);
        }

        public void DisplayTimer(int timer)
        {
            int minutes = timer / 60;
            DisplayNumber(minutes / 10, _caches[0]);
            DisplayNumber(minutes % 10, _caches[1]);

            int seconds = timer % 60;
            DisplayNumber(seconds / 10, _caches[2]);
            DisplayNumber(seconds % 10, _caches[3]);

            //Debug.Log($"Total : {timer} | Minutes : {minutes} => {minutes / 10}{minutes % 10} | Secondes : {seconds} => {seconds / 10}{seconds % 10}");
        }
    }
}