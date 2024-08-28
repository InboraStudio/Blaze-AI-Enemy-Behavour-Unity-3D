using UnityEngine;
using UnityEditor;

namespace InboraStudioAISpace
{
    [CustomEditor(typeof(InboraStudioAIAudioManager))]
    public class InboraStudioAIAudioManagerInspector : Editor
    {
        SerializedProperty autoCatchCamera,
        cameraOrPlayer,
        distanceToPlay,
        playAudioEvery;

        void OnEnable()
        {
            autoCatchCamera = serializedObject.FindProperty("autoCatchCamera");
            cameraOrPlayer = serializedObject.FindProperty("cameraOrPlayer");
            distanceToPlay = serializedObject.FindProperty("distanceToPlay");
            playAudioEvery = serializedObject.FindProperty("playAudioEvery");
        }

        public override void OnInspectorGUI () 
        {
            InboraStudioAIAudioManager script = (InboraStudioAIAudioManager)target;
            
            EditorGUILayout.LabelField("The Audio Manager is for centralizing and playing the patrol audios of AIs in normal and alert states only! All other audios are triggered by the AI behaviour itself.", EditorStyles.helpBox);
            EditorGUILayout.Space(10);

            EditorGUILayout.LabelField("Camera & Distance", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(autoCatchCamera);
            if (!script.autoCatchCamera) {
                EditorGUILayout.PropertyField(cameraOrPlayer);
            }
            EditorGUILayout.PropertyField(distanceToPlay);
            EditorGUILayout.Space(10);

            EditorGUILayout.LabelField("Audio Time Cycle", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(playAudioEvery);

            serializedObject.ApplyModifiedProperties();
        }
    }
}