using UnityEngine;
using UnityEditor;
using InboraStudioAISpace;

namespace InboraStudioAISpace
{
    [CustomEditor(typeof(InboraStudioAIDistanceCulling))]
    public class InboraStudioAIDistanceCullingInspector : Editor
    {
        SerializedProperty autoCatchCamera,
        cameraOrPlayer,
        distanceToCull,
        cycleFrames,
        restFrames,
        disableInboraStudioOnly;


        void OnEnable()
        {
            autoCatchCamera = serializedObject.FindProperty("autoCatchCamera");
            cameraOrPlayer = serializedObject.FindProperty("cameraOrPlayer");
            distanceToCull = serializedObject.FindProperty("distanceToCull");
            cycleFrames = serializedObject.FindProperty("cycleFrames");
            restFrames = serializedObject.FindProperty("restFrames");
            disableInboraStudioOnly = serializedObject.FindProperty("disableInboraStudioOnly");
        }


        public override void OnInspectorGUI () 
        {
            InboraStudioAIDistanceCulling script = (InboraStudioAIDistanceCulling)target;
            
            EditorGUILayout.LabelField("Camera & Distance", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(autoCatchCamera);
            if (!script.autoCatchCamera) {
                EditorGUILayout.PropertyField(cameraOrPlayer);
            }
            EditorGUILayout.PropertyField(distanceToCull);
            EditorGUILayout.Space(10);

            EditorGUILayout.LabelField("Frames Settings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(cycleFrames);
            EditorGUILayout.PropertyField(restFrames);
            EditorGUILayout.Space(10);

            EditorGUILayout.LabelField("Disabling", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(disableInboraStudioOnly);

            serializedObject.ApplyModifiedProperties();
        }
    }
}