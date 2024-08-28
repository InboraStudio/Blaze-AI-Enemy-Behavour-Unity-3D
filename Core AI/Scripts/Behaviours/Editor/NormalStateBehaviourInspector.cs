using UnityEditor;
using UnityEngine;
using InboraStudioAISpace;

namespace InboraStudioAISpace 
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(NormalStateBehaviour))]
    public class NormalStateBehaviourInspector : Editor
    {
        #region SERIALIZED PROPERTIES

        SerializedProperty moveSpeed,
        turnSpeed,
        idleAnim,
        moveAnim,
        animT,
        idleTime,
        playPatrolAudio,
        avoidFacingObstacles,
        obstacleLayers,
        obstacleRayDistance,
        obstacleRayOffset,
        showObstacleRay,
        onStateEnter,
        onStateExit;

        #endregion

        #region VARIABLES

        NormalStateBehaviour script;
        NormalStateBehaviour[] scripts;
        int spaceBetween = 20;

        #endregion

        #region METHODS

        void SetScripts()
        {
            script = (NormalStateBehaviour) target;

            Object[] objs = targets;
            scripts = new NormalStateBehaviour[objs.Length];
            for (int i = 0; i < objs.Length; i++) {
                scripts[i] = objs[i] as NormalStateBehaviour;
            }
        }

        void OnEnable()
        {
            SetScripts();

            moveSpeed = serializedObject.FindProperty("moveSpeed");
            turnSpeed = serializedObject.FindProperty("turnSpeed");

            idleAnim = serializedObject.FindProperty("idleAnim");
            moveAnim = serializedObject.FindProperty("moveAnim");
            animT = serializedObject.FindProperty("animT");

            idleTime = serializedObject.FindProperty("idleTime");

            playPatrolAudio = serializedObject.FindProperty("playPatrolAudio");

            avoidFacingObstacles = serializedObject.FindProperty("avoidFacingObstacles");
            obstacleLayers = serializedObject.FindProperty("obstacleLayers");
            obstacleRayDistance = serializedObject.FindProperty("obstacleRayDistance");
            obstacleRayOffset = serializedObject.FindProperty("obstacleRayOffset");
            showObstacleRay = serializedObject.FindProperty("showObstacleRay");

            onStateEnter = serializedObject.FindProperty("onStateEnter");
            onStateExit = serializedObject.FindProperty("onStateExit");
        }

        public override void OnInspectorGUI () 
        {
            EditorGUILayout.LabelField("Hover on any property below for insights", EditorStyles.helpBox);
            EditorGUILayout.Space(10);

            EditorGUILayout.LabelField("Speeds", EditorStyles.boldLabel);
            InboraStudioAIEditor.CheckDisableWithRootMotion(script.InboraStudio, ref moveSpeed);
            EditorGUILayout.PropertyField(turnSpeed);
            EditorGUILayout.Space(spaceBetween);

            EditorGUILayout.LabelField("Animations", EditorStyles.boldLabel);

            InboraStudioAIEditor.RefreshAnimationStateNames(script.InboraStudio.anim);
            EditorGUILayout.PropertyField(idleAnim);
            EditorGUILayout.Space();
            
            // drawing Move Anim (popup and multi-editing)
            InboraStudioAIEditor.DrawPopupProperty(script.InboraStudio.anim, "Move Anim", "moveAnim", ref script.moveAnim, scripts);
            
            EditorGUILayout.PropertyField(animT);
            EditorGUILayout.Space(spaceBetween);

            EditorGUILayout.LabelField("Time at waypoint", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(idleTime);
            EditorGUILayout.Space(spaceBetween);

            EditorGUILayout.LabelField("Audio", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(playPatrolAudio);
            EditorGUILayout.Space(spaceBetween);

            EditorGUILayout.LabelField("Obstacles", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(avoidFacingObstacles);
            if (script.avoidFacingObstacles) {
                EditorGUILayout.PropertyField(obstacleLayers);
                EditorGUILayout.PropertyField(obstacleRayDistance);
                EditorGUILayout.PropertyField(obstacleRayOffset);
                EditorGUILayout.PropertyField(showObstacleRay);
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