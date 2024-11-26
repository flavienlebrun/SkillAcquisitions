#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(NoiseConfig))]
public class NoiseEditor : Editor
{
    NoiseConfig noiseConfig;

    public override void OnInspectorGUI()
    {
        EditorGUI.BeginChangeCheck();

        noiseConfig = (NoiseConfig)target;

        EditorGUILayout.LabelField("TEXTURE PARAMETERS", EditorStyles.boldLabel);
        EditorGUILayout.Space(5);
        noiseConfig.textureFormat = (TextureFormat)EditorGUILayout.EnumPopup("Texture format :", noiseConfig.textureFormat);
        noiseConfig.resolutionX =  EditorGUILayout.IntSlider("Résolution X :",noiseConfig.resolutionX, 2, 256);
        noiseConfig.resolutionY =  EditorGUILayout.IntSlider("Résolution Y :", noiseConfig.resolutionY, 2, 256);
        noiseConfig.mipChain = EditorGUILayout.Toggle("mipChain :", noiseConfig.mipChain);
        noiseConfig.linear = EditorGUILayout.Toggle("Linear :", noiseConfig.linear);
        noiseConfig.wrapMode = (TextureWrapMode)EditorGUILayout.EnumPopup("WrapMode :", noiseConfig.wrapMode);
        noiseConfig.filterMode = (FilterMode)EditorGUILayout.EnumPopup("FilterMode :", noiseConfig.filterMode);
        noiseConfig.anisoLevel = EditorGUILayout.IntSlider("Aniso Level :",noiseConfig.anisoLevel, 1, 16);

        EditorGUILayout.Space(20);
        EditorGUILayout.LabelField("NOISE PARAMETERS", EditorStyles.boldLabel);
        EditorGUILayout.Space(5);
        noiseConfig.scale =  EditorGUILayout.FloatField("Scale :", noiseConfig.scale);
        noiseConfig.frequency =  EditorGUILayout.FloatField("Frequency :", noiseConfig.frequency);
        noiseConfig.persistance =  EditorGUILayout.FloatField("Persistance :", noiseConfig.persistance);
        noiseConfig.lacunarity =  EditorGUILayout.FloatField("Lacunarity :", noiseConfig.lacunarity);
        noiseConfig.amplitude =  EditorGUILayout.FloatField("Amplitude :", noiseConfig.amplitude);
        noiseConfig.octaves =  EditorGUILayout.IntSlider("Octaves :", noiseConfig.octaves, 1, 8);
        noiseConfig.seed =  EditorGUILayout.IntField("Seed :", noiseConfig.seed);
        noiseConfig.offsetDynamic = EditorGUILayout.Toggle("Offset Dynamic :", noiseConfig.offsetDynamic);
        noiseConfig.offset = EditorGUILayout.Vector2Field("Offset :", noiseConfig.offset);

        SerializedObject serializedGradient = new SerializedObject(target);
        SerializedProperty colorGradient = serializedGradient.FindProperty("coloring");
        EditorGUILayout.PropertyField(colorGradient, true, null);
        if (EditorGUI.EndChangeCheck())
        {
            serializedGradient.ApplyModifiedProperties();
        }
    }
}
#endif
