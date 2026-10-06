using UnityEngine;
using UnityEditor;
using System;

[CustomEditor(typeof(Transform))]
[CanEditMultipleObjects]
public class LODTransformEditor : Editor
{
    private Editor defaultTransformEditor;

    private void OnEnable()
    {
        defaultTransformEditor = CreateEditor(target, Type.GetType("UnityEditor.TransformInspector, UnityEditor"));
    }

    public override void OnInspectorGUI()
    {
        Transform t = (Transform)target;
        bool showButton = false;
        Transform targetChild = null;
        Transform targetParent = null;

        // Règle 1 : L'objet sélectionné est un enfant qui a un parent
        if (t.parent != null)
        {
            if (HasOnlyLevelZeroChildren(t.parent))
            {
                // Vérifier si l'enfant sélectionné est à zéro (le mauvais LOD)
                bool isSelectedAtZero = (t.localPosition == Vector3.zero && t.localRotation == Quaternion.identity && t.localScale == Vector3.one);

                if (isSelectedAtZero)
                {
                    // Il est à zéro, on va chercher un voisin (sibling) qui a un décalage valide
                    foreach (Transform sibling in t.parent)
                    {
                        if (sibling.localPosition != Vector3.zero || sibling.localRotation != Quaternion.identity || sibling.localScale != Vector3.one)
                        {
                            targetChild = sibling;
                            targetParent = t.parent;
                            showButton = true;
                            break;
                        }
                    }
                }
                else
                {
                    // L'enfant sélectionné a déjà le bon décalage
                    targetChild = t;
                    targetParent = t.parent;
                    showButton = true;
                }
            }
        }
        // Règle 2 : L'objet sélectionné est un parent, on cherche un enfant avec des positions non à 0
        else if (t.childCount > 0)
        {
            if (HasOnlyLevelZeroChildren(t))
            {
                foreach (Transform child in t)
                {
                    if (child.localPosition != Vector3.zero || child.localRotation != Quaternion.identity || child.localScale != Vector3.one)
                    {
                        targetChild = child;
                        targetParent = t;
                        showButton = true;
                        break;
                    }
                }
            }
        }

        // Affichage du bouton si toutes les conditions de sécurité sont remplies
        if (showButton && targetChild != null && targetParent != null)
        {
            EditorGUILayout.Space(5);
            
            GUI.backgroundColor = new Color(0.4f, 0.8f, 1f);
            if (GUILayout.Button("Fix LOD & Align Parent", GUILayout.Height(30)))
            {
                Undo.RecordObject(targetParent, "Fix Parent Position");
                Undo.RecordObject(targetChild, "Reset Child Transform");

                // Récupération du décalage local de l'enfant cible
                Vector3 localOffset = targetChild.localPosition;
                Quaternion localRotOffset = targetChild.localRotation;

                // Application du décalage au parent dans l'espace mondial
                targetParent.position += targetParent.TransformDirection(localOffset);
                targetParent.rotation *= localRotOffset;

                // Remise à zéro de TOUS les enfants de ce parent
                foreach (Transform sibling in targetParent)
                {
                    Undo.RecordObject(sibling, "Reset Sibling Transform");
                    sibling.localPosition = Vector3.zero;
                    sibling.localRotation = Quaternion.identity;
                    sibling.localScale = Vector3.one;
                }

                Debug.Log($"Parent et LODs corrigés pour : {targetParent.name} (via l'enfant {targetChild.name})", targetParent);
            }
            GUI.backgroundColor = Color.white;

            EditorGUILayout.Space(5);
        }

        // Affichage du Transform normal d'Unity
        if (defaultTransformEditor != null)
        {
            defaultTransformEditor.OnInspectorGUI();
        }
    }

    /// <summary>
    /// Vérifie que le parent ne possède que des enfants de niveau 0 (qui n'ont pas eux-mêmes d'enfants).
    /// </summary>
    private bool HasOnlyLevelZeroChildren(Transform parent)
    {
        foreach (Transform child in parent)
        {
            if (child.childCount > 0)
            {
                return false; 
            }
        }
        return parent.childCount > 0;
    }
}