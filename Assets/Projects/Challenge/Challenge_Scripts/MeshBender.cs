using UnityEngine;
using TMPro;
using System.Collections;

public class TextBenderStart : MonoBehaviour
{
    private TMP_Text textComponent;

    [Header("Paramètres de Courbure")]
    [Tooltip("Intensité de la courbe (valeurs positives ou négatives)")]
    public float intensiteCourbe = 20f;

    void Awake()
    {
        textComponent = GetComponent<TMP_Text>();
    }

    void Start()
    {
        if (textComponent == null) return;

        // On utilise une Coroutine pour s'assurer que TextMesh Pro 
        // a bien généré son mesh initial avant qu'on ne le modifie
        StartCoroutine(AppliquerCourbure());
    }

    IEnumerator AppliquerCourbure()
    {
        // Attend la fin du frame pour que le texte soit correctement initialisé par Unity
        yield return new WaitForEndOfFrame();

        // Force la mise à jour du texte pour avoir les données géométriques exactes
        textComponent.ForceMeshUpdate();

        TMP_TextInfo textInfo = textComponent.textInfo;
        int characterCount = textInfo.characterCount;

        if (characterCount == 0) yield break;

        // Calcul des limites horizontales du texte
        float boundsMinX = textComponent.bounds.min.x;
        float boundsMaxX = textComponent.bounds.max.x;
        float textWidth = boundsMaxX - boundsMinX;

        // Si le texte n'a pas de largeur (ex: texte vide), on arrête
        if (textWidth <= 0) yield break;

        // Boucle unique sur chaque lettre
        for (int i = 0; i < characterCount; i++)
        {
            TMP_CharacterInfo charInfo = textInfo.characterInfo[i];

            // On passe les espaces et caractères invisibles
            if (!charInfo.isVisible) continue;

            int materialIndex = charInfo.materialReferenceIndex;
            int vertexIndex = charInfo.vertexIndex;

            Vector3[] vertices = textInfo.meshInfo[materialIndex].vertices;

            // Calcul du centre X de la lettre en cours
            float charCenterX = (vertices[vertexIndex + 0].x + vertices[vertexIndex + 2].x) / 2f;

            // Position relative entre 0.0 (gauche) et 1.0 (droite)
            float positionRelative = (charCenterX - boundsMinX) / textWidth;

            // Calcul de la hauteur de l'arc via un Sinus (courbe fluide)
            float angle = positionRelative * Mathf.PI; 
            float decalageY = Mathf.Sin(angle) * intensiteCourbe;

            // Application du décalage sur les 4 sommets du caractère
            Vector3 offset = new Vector3(0, decalageY, 0);
            vertices[vertexIndex + 0] += offset; // Bas Gauche
            vertices[vertexIndex + 1] += offset; // Haut Gauche
            vertices[vertexIndex + 2] += offset; // Haut Droite
            vertices[vertexIndex + 3] += offset; // Bas Droite
        }

        // On applique les modifications une bonne fois pour toutes au Mesh
        textComponent.UpdateVertexData(TMP_VertexDataUpdateFlags.Vertices);
    }
}