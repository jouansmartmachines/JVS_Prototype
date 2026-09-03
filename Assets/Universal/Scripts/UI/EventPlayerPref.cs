using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

public class EventPlayerPref : MonoBehaviour
{
    [SerializeField] string _key;
    private enum Type
    {
        String,
        Float,
        Int
    }
    [SerializeField] Type _type;

    [SerializeField] public string s_value;
    [SerializeField] public float f_value;
    [SerializeField] public int i_value;

    [SerializeField] public UnityEvent<string> s_event;
    [SerializeField] public UnityEvent<float> f_event;
    [SerializeField] public UnityEvent<int> i_event;

    [SerializeField] public UnityEvent u_event;

    private void Start()
    {
        if (PlayerPrefs.HasKey(_key))
        {
            switch (_type)
            {
                case Type.String:
                    if (PlayerPrefs.GetString(_key).Equals(s_value))
                        s_event?.Invoke(s_value);
                    break;
                case Type.Float:
                    if (PlayerPrefs.GetFloat(_key).Equals(f_value))
                        f_event?.Invoke(f_value);
                    break;
                case Type.Int:
                    if (PlayerPrefs.GetInt(_key).Equals(i_value))
                        i_event?.Invoke(i_value);
                    break;
                default:
                    break;
            }
            u_event?.Invoke();
        }

    }

#if UNITY_EDITOR
    [CustomEditor(typeof(EventPlayerPref))]
    public class EventPlayerPref_Inspector : Editor
    {
        SerializedProperty m_OnInteract;
        SerializedProperty m_Event;

        void OnEnable()
        {
            m_OnInteract = serializedObject.FindProperty("s_event");
            m_Event = serializedObject.FindProperty("u_event");
        }

        public override void OnInspectorGUI()
        {
            EventPlayerPref eventPlayerPref = (EventPlayerPref)target;

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Key :");
            eventPlayerPref._key = EditorGUILayout.TextField(eventPlayerPref._key);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Value Type :");
            eventPlayerPref._type = (Type)EditorGUILayout.EnumPopup(eventPlayerPref._type);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Value :");
            switch (eventPlayerPref._type)
            {
                case Type.String:
                    eventPlayerPref.s_value = EditorGUILayout.TextField(eventPlayerPref.s_value);
                    m_OnInteract = serializedObject.FindProperty("s_event");
                    break;
                case Type.Float:
                    eventPlayerPref.f_value = EditorGUILayout.FloatField(eventPlayerPref.f_value);
                    m_OnInteract = serializedObject.FindProperty("f_event");
                    break;
                case Type.Int:
                    eventPlayerPref.i_value = EditorGUILayout.IntField(eventPlayerPref.i_value);
                    m_OnInteract = serializedObject.FindProperty("i_event");
                    break;
                default:
                    break;
            }
            EditorGUILayout.EndHorizontal();

            //EditorGUILayout.BeginHorizontal();
            //EditorGUILayout.LabelField("Event :");
            EditorGUILayout.LabelField("Value Event (Invoke() if Value = KeyValue) :");
            serializedObject.Update();
            EditorGUILayout.PropertyField(m_OnInteract);
            serializedObject.ApplyModifiedProperties();
            //EditorGUILayout.EndHorizontal();

            EditorGUILayout.LabelField("Universal Event (Invoke() if HasKey() = True) :");
            serializedObject.Update();
            EditorGUILayout.PropertyField(m_Event);
            serializedObject.ApplyModifiedProperties();
        }
    }
#endif
}
