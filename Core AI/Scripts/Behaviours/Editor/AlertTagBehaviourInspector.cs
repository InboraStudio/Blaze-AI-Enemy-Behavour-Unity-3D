using UnityEditor;
using UnityEngine;

namespace InboraStudioAISpace
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(AlertTagBehaviour))]
    public class AlertTagBehaviourInspector : Editor
    {
        #region SERIALIZED PROPERTIES

        SerializedProperty checkLocation,
        onSightAnim,
        onSightDuration,
        reachedLocationAnim,
        reachedLocationDuration,
        animT,
        playAudio,
        audioIndex,
        callOtherAgents,
        callRange,
        showCallRange,
        otherAgentsLayers,
        callPassesColliders,
        randomizeCallPosition,
        onStateEnter,
        onStateExit;

        #endregion

        #region EDITOR VARS
        
        int spaceBetween = 20;
        AlertTagBehaviour script;
        AlertTagBehaviour[] scripts;
        
        #endregion

        #region METHODS
        
        void SetScripts()
        {
            script = (AlertTagBehaviour) target;

            Object[] objs = targets;
            scripts = new AlertTagBehaviour[objs.Length];
            for (int i = 0; i < objs.Length; i++) {
                scripts[i] = objs[i] as AlertTagBehaviour;
            }
        }

        void OnEnable()
        {
            SetScripts();

            checkLocation = serializedObject.FindProperty("checkLocation");
            onSightAnim = serializedObject.FindProperty("onSightAnim");
            onSightDuration = serializedObject.FindProperty("onSightDuration");
            reachedLocationAnim = serializedObject.FindProperty("reachedLocationAnim");
            reachedLocationDuration = serializedObject.FindProperty("reachedLocationDuration");
            animT = serializedObject.FindProperty("animT");
            playAudio = serializedObject.FindProperty("playAudio");
            audioIndex = serializedObject.FindProperty("audioIndex");
            callOtherAgents = serializedObject.FindProperty("callOtherAgents");
            callRange = serializedObject.FindProperty("callRange");
            showCallRange = serializedObject.FindProperty("showCallRange");
            otherAgentsLayers = serializedObject.FindProperty("otherAgentsLayers");
            callPassesColliders = serializedObject.FindProperty("callPassesColliders");
            randomizeCallPosition = serializedObject.FindProperty("randomizeCallPosition");
            onStateEnter = serializedObject.FindProperty("onStateEnter");
            onStateExit = serializedObject.FindProperty("onStateExit");
        }

        public override void OnInspectorGUI()
        {
            InboraStudioAIEditor.RefreshAnimationStateNames(script.InboraStudio.anim);
            EditorGUILayout.LabelField("Hover on any property below for insights", EditorStyles.helpBox);
            EditorGUILayout.Space(10);

            EditorGUILayout.LabelField("Check Location", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(checkLocation);
            EditorGUILayout.Space(spaceBetween);

            EditorGUILayout.LabelField("Animations", EditorStyles.boldLabel);
            InboraStudioAIEditor.DrawPopupProperty(script.InboraStudio.anim, "On Sight Anim", "onSightAnim", ref script.onSightAnim, scripts);
            EditorGUILayout.PropertyField(onSightDuration);
            EditorGUILayout.Space(5);
            if (script.checkLocation) {
                InboraStudioAIEditor.DrawPopupProperty(script.InboraStudio.anim, "Reached Location Anim", "reachedLocationAnim", ref script.reachedLocationAnim, scripts);
                EditorGUILayout.PropertyField(reachedLocationDuration);
            }
            EditorGUILayout.PropertyField(animT);

            EditorGUILayout.Space(spaceBetween);

            EditorGUILayout.LabelField("Audio", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(playAudio);
            if (script.playAudio) {
                EditorGUILayout.PropertyField(audioIndex);
            }

            EditorGUILayout.Space(spaceBetween);

            EditorGUILayout.LabelField("Call Others", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(callOtherAgents);
            if (script.callOtherAgents) {
                EditorGUILayout.Space();

                EditorGUILayout.PropertyField(callRange);
                EditorGUILayout.PropertyField(showCallRange);
                EditorGUILayout.Space();
                
                EditorGUILayout.PropertyField(otherAgentsLayers);
                EditorGUILayout.PropertyField(callPassesColliders);
                EditorGUILayout.Space();
                
                EditorGUILayout.PropertyField(randomizeCallPosition);
            }
            EditorGUILayout.Space(spaceBetween);
            

            EditorGUILayout.LabelField("State Events", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(onStateEnter);
            EditorGUILayout.PropertyField(onStateExit);
            

            serializedObject.ApplyModifiedProperties();
        }

        #endregion
    }
}