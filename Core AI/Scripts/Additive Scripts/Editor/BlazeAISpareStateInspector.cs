using UnityEditor;

[CanEditMultipleObjects]
[CustomEditor(typeof(InboraStudioAISpareState))]
public class InboraStudioAISpareStateInspector : Editor
{
    SerializedProperty spareStates;

    void OnEnable()
    {
        spareStates = serializedObject.FindProperty("spareStates");
    }

    public override void OnInspectorGUI () 
    {
        EditorGUILayout.PropertyField(spareStates);
        serializedObject.ApplyModifiedProperties();
    }
}