using UnityEditor;

[CustomEditor(typeof(InboraStudioAIDistraction))]
public class InboraStudioAIDistractionInspector : Editor
{
    SerializedProperty distractOnAwake,
    agentLayers,
    distractionRadius,
    passThroughColliders,
    distractOnlyPrioritizedAgent;

    void OnEnable()
    {
        distractOnAwake = serializedObject.FindProperty("distractOnAwake");
        agentLayers = serializedObject.FindProperty("agentLayers");
        distractionRadius = serializedObject.FindProperty("distractionRadius");
        passThroughColliders = serializedObject.FindProperty("passThroughColliders");
        distractOnlyPrioritizedAgent = serializedObject.FindProperty("distractOnlyPrioritizedAgent");
    }

    public override void OnInspectorGUI () 
    {
        EditorGUILayout.LabelField("General", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(distractOnAwake);
        EditorGUILayout.Space(10);

        EditorGUILayout.LabelField("Layers & Radius", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(agentLayers);
        EditorGUILayout.PropertyField(distractionRadius);
        EditorGUILayout.PropertyField(passThroughColliders);
        EditorGUILayout.Space(10);

        EditorGUILayout.LabelField("Distraction Options", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(distractOnlyPrioritizedAgent);

        serializedObject.ApplyModifiedProperties();
    }
}