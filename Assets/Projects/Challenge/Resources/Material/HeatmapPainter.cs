using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

/*
[ExecuteAlways]
public class HeatmapPainter : MonoBehaviour
{
    [System.Serializable]
    public class Zone
    {
        [Header("X Range (0 = left, 1 = right)")]
        public Vector2 xRange = new Vector2(0f, 1f);

        [Header("Y Range (0 = bottom, 1 = top)")]
        public Vector2 yRange = new Vector2(0f, 1f);

        [Header("Intensity")]
        [Range(0f, 2f)]
        public float intensity = 1f;
    }

    [Header("Material & RenderTexture")]
    public Material heatmapMaterial;
    public RenderTexture targetTexture;

    [Header("Zones")]
    public Zone[] zones;

    const int MAX_ZONES = 16;

    public void SendZones()
    {
        if (!heatmapMaterial) return;

        int count = Mathf.Min(zones.Length, MAX_ZONES);

        Vector4[] zoneData = new Vector4[MAX_ZONES];       // xCenter, yCenter, sizeX, sizeY
        Vector4[] intensityData = new Vector4[MAX_ZONES];  // intensité

        for (int i = 0; i < count; i++)
        {
            // centre = moyenne des extrémités
            float centerX = (zones[i].xRange.x + zones[i].xRange.y) * 0.5f;
            float centerY = (zones[i].yRange.x + zones[i].yRange.y) * 0.5f;

            // taille = différence entre max et min
            float sizeX = Mathf.Max(0.001f, zones[i].xRange.y - zones[i].xRange.x);
            float sizeY = Mathf.Max(0.001f, zones[i].yRange.y - zones[i].yRange.x);

            zoneData[i] = new Vector4(centerX, centerY, sizeX, sizeY);
            intensityData[i] = new Vector4(zones[i].intensity, 0, 0, 0);
        }

        heatmapMaterial.SetInt("_ZoneCount", count);
        heatmapMaterial.SetVectorArray("_Zones", zoneData);
        heatmapMaterial.SetVectorArray("_ZoneIntensity", intensityData);
    }



    public void RenderHeatmap()
    {
        if (!heatmapMaterial || !targetTexture) return;

        SendZones();
        Graphics.Blit(null, targetTexture, heatmapMaterial);
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(HeatmapPainter))]
public class HeatmapPainterEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        HeatmapPainter painter = (HeatmapPainter)target;

        if (GUILayout.Button("Render Heatmap"))
        {
            painter.SendZones();
            painter.RenderHeatmap();
        }
    }
}
#endif

[CustomPropertyDrawer(typeof(HeatmapPainter.Zone))]
[CustomPropertyDrawer(typeof(HeatmapPainter.Zone))]
public class ZoneDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        float lineHeight = EditorGUIUtility.singleLineHeight;
        float spacing = 2f;
        Rect rect = new Rect(position.x, position.y, position.width, lineHeight);

        // Label principale
        EditorGUI.LabelField(rect, label, EditorStyles.boldLabel);

        rect.y += lineHeight + spacing;

        // --- X Range Min-Max ---
        SerializedProperty xRangeProp = property.FindPropertyRelative("xRange");
        Vector2 xRange = xRangeProp.vector2Value; // copie locale

        EditorGUI.MinMaxSlider(rect, new GUIContent("X Range"), ref xRange.x, ref xRange.y, 0f, 1f);
        rect.y += lineHeight + spacing;
        EditorGUI.LabelField(rect, $"X Min: {xRange.x:F2} | X Max: {xRange.y:F2}");

        // réassigner la valeur modifiée
        xRangeProp.vector2Value = xRange;

        rect.y += lineHeight + spacing;

        // --- Y Range Min-Max ---
        SerializedProperty yRangeProp = property.FindPropertyRelative("yRange");
        Vector2 yRange = yRangeProp.vector2Value; // copie locale

        EditorGUI.MinMaxSlider(rect, new GUIContent("Y Range"), ref yRange.x, ref yRange.y, 0f, 1f);
        rect.y += lineHeight + spacing;
        EditorGUI.LabelField(rect, $"Y Min: {yRange.x:F2} | Y Max: {yRange.y:F2}");

        // réassigner la valeur modifiée
        yRangeProp.vector2Value = yRange;

        rect.y += lineHeight + spacing;

        // Intensité
        SerializedProperty intensityProp = property.FindPropertyRelative("intensity");
        EditorGUI.Slider(rect, intensityProp, 0f, 2f);

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUIUtility.singleLineHeight * 7 + 10;
    }
}
*/