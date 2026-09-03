using UnityEngine;
using System.Reflection;

/// <summary>
/// Assigne dynamiquement un Transform à une variable cible par son nom.
/// </summary>
public class UniversalTransformLinker : MonoBehaviour
{
    [Header("Cible")]
    [SerializeField] private MonoBehaviour _targetScript;

    [Header("Configuration des noms")]
    [SerializeField] private string _variableToFill = "_collum";
    [SerializeField] private string _gameObjectNameToFind = "collum";

    private void Start()
    {
        LinkTransform();
    }

    public void LinkTransform()
    {
        Transform foundTransform = FindDeepChild(transform, _gameObjectNameToFind);
        if (foundTransform == null) return;


        FieldInfo field = _targetScript.GetType().GetField(_variableToFill, 
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        if (field != null && field.FieldType.IsAssignableFrom(typeof(Transform)))
        {
            field.SetValue(_targetScript, foundTransform);
        }
    }

    /// <summary>
    /// Recherche récursive propre pour éviter les erreurs de hiérarchie
    /// </summary>
    private Transform FindDeepChild(Transform parent, string name)
    {
        if (parent.name == name) return parent;

        foreach (Transform child in parent)
        {
            Transform result = FindDeepChild(child, name);
            if (result != null) return result;
        }
        return null;
    }
}