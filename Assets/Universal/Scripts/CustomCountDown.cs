using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;

[RequireComponent(typeof(TextMeshProUGUI)), RequireComponent(typeof(AudioSource))]
public class CustomCountDown : MonoBehaviour
{
    [System.Serializable]
    public class TextAudio
    {
        public string text = string.Empty;
        public AudioClip audio = null;
    }

    TextMeshProUGUI _textField;
    AudioSource _source;

    [SerializeField] int _countDownValue;
    [SerializeField] List<TextAudio> _countDownText = new List<TextAudio>();
    [SerializeField] bool showList = true;

    public Action OnCountDownEnd;

    public void StartCountDown()
    {
        if (_countDownValue <= 0) return;
        _textField = GetComponent<TextMeshProUGUI>();
        _source = GetComponent<AudioSource>();
        _countDownText[0].text = Localizer.Get("Parti !");
        StartCoroutine(CountDown());
    }

    private IEnumerator CountDown()
    {
        for (int i = _countDownValue; i >= 0; i--)
        {
            _textField.text = _countDownText[i].text;
            _source.clip = _countDownText[i].audio;
            _source.Play();
            yield return new WaitForSeconds(1f);
        }

        _textField.text = _countDownText[0].text;
        yield return new WaitForSeconds(0.25f);

        OnCountDownEnd?.Invoke();
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(CustomCountDown))]
    public class CustomCountDown_Inspector : Editor
    {
        public override void OnInspectorGUI()
        {
            CustomCountDown countDown = (CustomCountDown)target;

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("CountDown Value");
            countDown._countDownValue = EditorGUILayout.IntField(countDown._countDownValue);
            EditorGUILayout.EndHorizontal();

            if(countDown._countDownValue > 0)
            {
                countDown.showList = EditorGUILayout.Foldout(countDown.showList, "Texts", true);

                if (countDown.showList)
                {
                    while (countDown._countDownValue + 1 > countDown._countDownText.Count)
                    {
                        countDown._countDownText.Add(new TextAudio());
                    }

                    while (countDown._countDownValue + 1 < countDown._countDownText.Count)
                    {
                        countDown._countDownText.RemoveAt(countDown._countDownText.Count - 1);
                    }

                    for (int i = 0; i < countDown._countDownText.Count; i++)
                    {
                        countDown._countDownText[i].text = EditorGUILayout.TextField("Text at " + i.ToString(), countDown._countDownText[i].text);
                        countDown._countDownText[i].audio  = (AudioClip)EditorGUILayout.ObjectField("Audio ", countDown._countDownText[i].audio, typeof(AudioClip), true);
                    }
                }
            }
        }
    }
#endif
}
