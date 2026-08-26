using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MenuSelection
{
    public class ButtonHolder : MonoBehaviour
    {
        public static bool isMoving = false;

        [SerializeField] Universal_Button _rightButton;
        [SerializeField] Universal_Button _leftButton;

        public void Start()
        {
            isMoving = false;
        }

        public void SetUpButton(bool right, bool left, Transform holder)
        {
            if (right)
            {
                _rightButton.Event.AddListener(() => MoveHolder(holder, false));
                _rightButton.gameObject.SetActive(true);
            }

            if (left)
            {
                _leftButton.Event.AddListener(() => MoveHolder(holder, true));
                _leftButton.gameObject.SetActive(true);
            }
        }

        private void MoveHolder(Transform holder, bool isLeft)
        {
            if (isMoving) return;
            StartCoroutine(Move(holder, isLeft));
        }

        private IEnumerator Move(Transform holder, bool isLeft)
        {
            isMoving = true;
            var endValue = holder.transform.position.x;
            endValue += isLeft ? 1920 : -1920;
            yield return holder.DOMoveX(endValue, 0.5f).WaitForCompletion();
            isMoving = false;
        }
    }
}