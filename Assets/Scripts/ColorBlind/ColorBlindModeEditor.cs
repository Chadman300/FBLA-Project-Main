using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ColorBlindMode))]
public class ColorBlindModeEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector(); // Draw the default inspector

        ColorBlindMode colorBlindModeController = (ColorBlindMode)target;

        // Add a button to the inspector
        if (!Application.isPlaying)
        {
            GUILayout.Space(EditorGUIUtility.singleLineHeight);
            if (GUILayout.Button("Debug Update Color-blind Mode"))
            {
                colorBlindModeController.GetChannelMixer();
                colorBlindModeController.UpdateColorBlindMode();
            }
        }
    }
}