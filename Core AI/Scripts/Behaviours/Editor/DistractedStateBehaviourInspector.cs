using UnityEditor;
using UnityEngine;

namespace InboraStudioAISpace
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(DistractedStateBehaviour))]

    public class DistractedStateBehaviourInspector : Editor
    {
        #region SERIALZED PROPERTIES

        SerializedProperty timeToReact,

        checkLocation,
        timeBeforeMovingToLocation,
        checkAnim,
        checkAnimT,
        timeToCheck,
        randomizePoint,
        randomizeRadius,

        searchLocationRadius,
        searchPoints,
        searchPointAnim,
        pointWaitTime,
        endSearchAnim,
        endSearchAnimTime,
        searchAnimsT,

        playAudioOnCheckLocation,
        playAudioOnSearchStart,
        playAudioOnSearchEnd,

        onStateEnter,
        onStateExit;

        #endregion

        #region VARIABLES

        DistractedStateBehaviour script;
        DistractedStateBehaviour[] scripts;
        int spaceBetween = 15;

        #endregion

        #region METHODS

        void SetScripts()
        {
            script = (DistractedStateBehaviour) target;

            Object[] objs = targets;
            scripts = new DistractedStateBehaviour[objs.Length];
            for (int i = 0; i < objs.Length; i++) {
                scripts[i] = objs[i] as DistractedStateBehaviour;
            }
        }

        void OnEnable()
        {
            SetScripts();

            timeToReact = serializedObject.FindProperty("timeToReact");
            
            checkLocation = serializedObject.FindProperty("checkLocation");
            timeBeforeMovingToLocation = serializedObject.FindProperty("timeBeforeMovingToLocation");
            checkAnim = serializedObject.FindProperty("checkAnim");
            checkAnimT = serializedObject.FindProperty("checkAnimT");
            timeToCheck = serializedObject.FindProperty("timeToCheck");
            randomizePoint = serializedObject.FindProperty("randomizePoint");
            randomizeRadius = serializedObject.FindProperty("randomizeRadius");

            searchLocationRadius = serializedObject.FindProperty("searchLocationRadius");
            searchPoints = serializedObject.FindProperty("searchPoints");
            searchPointAnim = serializedObject.FindProperty("searchPointAnim");
            pointWaitTime = serializedObject.FindProperty("pointWaitTime");
            endSearchAnim = serializedObject.FindProperty("endSearchAnim");
            endSearchAnimTime = serializedObject.FindProperty("endSearchAnimTime");
            searchAnimsT = serializedObject.FindProperty("searchAnimsT");

            playAudioOnCheckLocation = serializedObject.FindProperty("playAudioOnCheckLocation");
            playAudioOnSearchStart = serializedObject.FindProperty("playAudioOnSearchStart");
            playAudioOnSearchEnd = serializedObject.FindProperty("playAudioOnSearchEnd");

            onStateEnter = serializedObject.FindProperty("onStateEnter");
            onStateExit = serializedObject.FindProperty("onStateExit");
        }

        public override void OnInspectorGUI () 
        {
            EditorGUILayout.LabelField("Hover on any property below for insights", EditorStyles.helpBox);
            EditorGUILayout.Space(10);

            InboraStudioAIEditor.RefreshAnimationStateNames(script.InboraStudio.anim);

            EditorGUILayout.PropertyField(timeToReact);
            EditorGUILayout.Space(spaceBetween);
            
            EditorGUILayout.PropertyField(checkLocation);
            if (script.checkLocation) {
                EditorGUILayout.PropertyField(timeBeforeMovingToLocation);
                EditorGUILayout.Space();

                EditorGUILayout.PropertyField(timeToCheck);
                InboraStudioAIEditor.DrawPopupProperty(script.InboraStudio.anim, "Check Anim", "checkAnim", ref script.checkAnim, scripts);
                EditorGUILayout.PropertyField(checkAnimT);
                EditorGUILayout.Space();

                EditorGUILayout.PropertyField(randomizePoint);
                if (script.randomizePoint) {
                    EditorGUILayout.PropertyField(randomizeRadius);
                    EditorGUILayout.Space();
                }

                EditorGUILayout.PropertyField(playAudioOnCheckLocation);
                EditorGUILayout.Space(spaceBetween);

                EditorGUILayout.PropertyField(searchLocationRadius);
                EditorGUILayout.Space();
                if (script.searchLocationRadius) 
                {
                    EditorGUILayout.PropertyField(searchPoints);

                    // search point animation
                    InboraStudioAIEditor.DrawPopupProperty(script.InboraStudio.anim, "Search Point Anim", "searchPointAnim", ref script.searchPointAnim, scripts);

                    EditorGUILayout.PropertyField(pointWaitTime);
                    EditorGUILayout.Space();

                    // end search animation
                    InboraStudioAIEditor.DrawPopupProperty(script.InboraStudio.anim, "End Search Anim", "endSearchAnim", ref script.endSearchAnim, scripts);
                    
                    EditorGUILayout.PropertyField(endSearchAnimTime);
                    EditorGUILayout.Space();

                    EditorGUILayout.PropertyField(searchAnimsT);
                    EditorGUILayout.Space();
                    
                    EditorGUILayout.PropertyField(playAudioOnSearchStart);
                    EditorGUILayout.PropertyField(playAudioOnSearchEnd);
                }
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