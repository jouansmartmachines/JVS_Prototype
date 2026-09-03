using UnityEngine;
using TMPro;
using System.Reflection;

public class UniversalFontLinker : MonoBehaviour
{
    [Header("Source (Scène)")]
    [SerializeField] private TMP_Text _sourceTextName ;

    [Header("Cible (Script)")]
    [SerializeField] private MonoBehaviour _targetScript;
    [SerializeField] private string _fontVariableName = "_font";

    void Start()
    {
        ExecuteLink();
    }

    public void ExecuteLink()
    {
        FieldInfo field = _targetScript.GetType().GetField(_fontVariableName, 
        BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

        field.SetValue(_targetScript, _sourceTextName.font);
        Debug.Log($"<color=green>Succès :</color> Font injectée dans {_targetScript.GetType().Name}");
    }
}