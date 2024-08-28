using UnityEditor;
using UnityEngine;

namespace InboraStudioAISpace 
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(SurprisedStateBehaviour))]
    public class SurprisedStateBehaviourInspector : Editor
    {
        #region SERIALIZED PROPERTIES

        SerializedProperty anim,
        animT,
        duration,
        turnSpeed,
        playAudio,
        onStateEnter,
        onStateExit;

        #endregion

        #region VARIABLES

        int spaceBetween = 10;
        SurprisedStateBehaviour script;
        SurprisedStateBehaviour[] scripts;

        #endregion

        #region METHODS

        void SetScripts()
        {
            script = (SurprisedStateBehaviour) target;

            Object[] objs = targets;
            scripts = new SurprisedStateBehaviour[objs.Length];
            for (int i = 0; i < objs.Length; i++) {
                scripts[i] = objs[i] as SurprisedStateBehaviour;
            }
        }

        void OnEnable()
        {
            SetScripts();

            anim = serializedObject.FindProperty("anim");
            animT = serializedObject.FindProperty("animT");
            duration = serializedObject.FindProperty("duration");
            turnSpeed = serializedObject.FindProperty("turnSpeed");
            playAudio = serializedObject.FindProperty("playAudio");
            onStateEnter = serializedObject.FindProperty("onStateEnter");
            onStateExit = serializedObject.FindProperty("onStateExit");
        }

        public override void OnInspectorGUI () 
        {
            EditorGUILayout.LabelField("Hover on any property below for insights", EditorStyles.helpBox);
            EditorGUILayout.Space(10);

            InboraStudioAIEditor.RefreshAnimationStateNames(script.InboraStudio.anim);
            EditorGUILayout.LabelField("Animation", EditorStyles.boldLabel);

            InboraStudioAIEditor.DrawPopupProperty(script.InboraStudio.anim, "Anim", "anim", ref script.anim, scripts);
            EditorGUILayout.PropertyField(animT);
            EditorGUILayout.Space(spaceBetween);

            EditorGUILayout.LabelField("Duration", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(duration);
            EditorGUILayout.Space(spaceBetween);

            EditorGUILayout.LabelField("Turn Speed", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(turnSpeed);
            EditorGUILayout.Space(spaceBetween);

            EditorGUILayout.LabelField("Audio", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(playAudio);
            EditorGUILayout.Space(spaceBetween);

            EditorGUILayout.LabelField("State Events", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(onStateEnter);
            EditorGUILayout.PropertyField(onStateExit);

            serializedObject.ApplyModifiedProperties();
        }

        #endregion
    }
}