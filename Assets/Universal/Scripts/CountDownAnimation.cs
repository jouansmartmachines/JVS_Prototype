using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CountDownAnimation : MonoBehaviour
{
    [SerializeField]
    TextMeshProUGUI _countDownText;

    public bool IsActive => _isActive;
    private bool _isActive = false;

    public void PlayAnim()
    {
        _countDownText.gameObject.SetActive(true);
        StartCoroutine(CountDown());
    }
    IEnumerator CountDown()
    {
        _isActive = true;
        for (int i = 0; i < 10; i++)
        {
            _countDownText.text = (10 - i).ToString();
            yield return new WaitForSeconds(1f);
        }
        _countDownText.text = "0";
        _countDownText.gameObject.SetActive(false);
        _isActive = false;
    }
}
