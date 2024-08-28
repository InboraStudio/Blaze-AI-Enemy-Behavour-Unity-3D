using UnityEngine;
using UnityEditor;

namespace InboraStudioAISpace
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(CompanionBehaviour))]
    public class CompanionBehaviourInspector : Editor
    {
        #region SERIALIZED PROPERTIES

        SerializedProperty follow,
        followIfDistance,
        walkIfDistance,
        movingWindow,

        idleAnim,
        runAnim,
        runSpeed,
        walkAnim,
        walkSpeed,
        turnSpeed,
        stayPutAnim,
        animsT,

        wanderAround,
        wanderPointTime,
        wanderMoveSpeed,
        wanderIdleAnims,
        wanderMoveAnim,

        moveAwayOnContact,
        layersToAvoid,
        walkOnMoveAway,

        playAudioForIdleAndWander,
        audioTimer,
        playAudioOnFollowState,

        onStateEnter,
        onStateExit,
        fireEventDistance,
        exceededDistanceEvent;
        
        #endregion
        
        #region EDITOR VARS

        CompanionBehaviour script;
        CompanionBehaviour[] scripts;

        string[] tabs = {"General", "Wander & Contact", "Audios", "Events"};
        int tabSelected = 0;
        int tabIndex = -1;
        int spaceBetween = 20;

        #endregion
        
        #region EDITOR METHODS

        void SetScripts()
        {
            script = (CompanionBehaviour) target;

            Object[] objs = targets;
            scripts = new CompanionBehaviour[objs.Length];
            for (int i = 0; i < objs.Length; i++) {
                scripts[i] = objs[i] as CompanionBehaviour;
            }
        }

        void OnEnable()
        {
            SetScripts();
            GetSelectedTab();

            follow = serializedObject.FindProperty("follow");
            followIfDistance = serializedObject.FindProperty("followIfDistance");
            walkIfDistance = serializedObject.FindProperty("walkIfDistance");
            movingWindow = serializedObject.FindProperty("movingWindow");

            idleAnim = serializedObject.FindProperty("idleAnim");

            runAnim = serializedObject.FindProperty("runAnim");
            runSpeed = serializedObject.FindProperty("runSpeed");

            walkAnim = serializedObject.FindProperty("walkAnim");
            walkSpeed = serializedObject.FindProperty("walkSpeed");

            turnSpeed = serializedObject.FindProperty("turnSpeed");

            stayPutAnim = serializedObject.FindProperty("stayPutAnim");
            animsT = serializedObject.FindProperty("animsT");

            wanderAround = serializedObject.FindProperty("wanderAround");
            wanderPointTime = serializedObject.FindProperty("wanderPointTime");
            wanderMoveSpeed = serializedObject.FindProperty("wanderMoveSpeed");
            wanderIdleAnims = serializedObject.FindProperty("wanderIdleAnims");
            wanderMoveAnim = serializedObject.FindProperty("wanderMoveAnim");

            moveAwayOnContact = serializedObject.FindProperty("moveAwayOnContact");
            layersToAvoid = serializedObject.FindProperty("layersToAvoid");
            walkOnMoveAway = serializedObject.FindProperty("walkOnMoveAway");

            playAudioForIdleAndWander = serializedObject.FindProperty("playAudioForIdleAndWander");
            audioTimer = serializedObject.FindProperty("audioTimer");
            playAudioOnFollowState = serializedObject.FindProperty("playAudioOnFollowState");

            onStateEnter = serializedObject.FindProperty("onStateEnter");
            onStateExit = serializedObject.FindProperty("onStateExit");
            fireEventDistance = serializedObject.FindProperty("fireEventDistance");
            exceededDistanceEvent = serializedObject.FindProperty("exceededDistanceEvent");
        }

        public override void OnInspectorGUI () 
        {
            DrawToolbar();
            EditorGUILayout.LabelField("Hover on any property below for insights", EditorStyles.helpBox);
            EditorGUILayout.Space(10);

            tabIndex = -1;
            InboraStudioAIEditor.RefreshAnimationStateNames(script.InboraStudio.anim);

            switch (tabSelected)
            {
                case 0:
                    DrawGeneralTab();
                    break;
                case 1:
                    DrawWanderingTab(script);
                    break;
                case 2:
                    DrawAudiosTab(script);
                    break;
                case 3:
                    DrawEventsTab();
                    break;
            }

            EditorPrefs.SetInt("InboraStudioCompanionTabSelected", tabSelected);
            serializedObject.ApplyModifiedProperties();
        }

        #endregion

        #region DRAWING

        void GetSelectedTab()
        {
            if (EditorPrefs.HasKey("InboraStudioCompanionTabSelected")) {
                tabSelected = EditorPrefs.GetInt("InboraStudioCompanionTabSelected");
            }
            else {
                tabSelected = 0;
            }   
        }

        void DrawToolbar()
        {   
            GUILayout.BeginHorizontal();
            
            foreach (var item in tabs) {
                tabIndex++;

                if (tabIndex == tabSelected) {
                    // selected button
                    GUILayout.Button(item, InboraStudioAIEditor.ToolbarStyling(true), GUILayout.MinWidth(105), GUILayout.Height(40));
                }
                else {
                    // unselected buttons
                    if (GUILayout.Button(item, InboraStudioAIEditor.ToolbarStyling(false), GUILayout.MinWidth(105), GUILayout.Height(40))) {
                        // this will trigger when a button is pressed
                        tabSelected = tabIndex;
                    }
                }
            }

            GUILayout.EndHorizontal();
        }

        void DrawGeneralTab()
        {
            EditorGUILayout.LabelField("Follow", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(follow);
            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(followIfDistance);
            EditorGUILayout.PropertyField(walkIfDistance);
            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(movingWindow);
            EditorGUILayout.Space(spaceBetween);

            EditorGUILayout.LabelField("Speeds & Anims", EditorStyles.boldLabel);

            // draw Idle Anim (popup and multi-editing)
            InboraStudioAIEditor.DrawPopupProperty(script.InboraStudio.anim, "Idle Anim", "idleAnim", ref script.idleAnim, scripts);
            EditorGUILayout.Space();

            // draw Run Anim (popup and multi-editing)
            InboraStudioAIEditor.DrawPopupProperty(script.InboraStudio.anim, "Run Anim", "runAnim", ref script.runAnim, scripts);
            InboraStudioAIEditor.CheckDisableWithRootMotion(script.InboraStudio, ref runSpeed);
            EditorGUILayout.Space();

            // draw Walk Anim (popup and multi-editing)
            InboraStudioAIEditor.DrawPopupProperty(script.InboraStudio.anim, "Walk Anim", "walkAnim", ref script.walkAnim, scripts);
            InboraStudioAIEditor.CheckDisableWithRootMotion(script.InboraStudio, ref walkSpeed);
            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(turnSpeed);
            EditorGUILayout.Space();

            InboraStudioAIEditor.DrawPopupProperty(script.InboraStudio.anim, "Stay Put Anim", "stayPutAnim", ref script.stayPutAnim, scripts);
            EditorGUILayout.PropertyField(animsT);
        }

        void DrawWanderingTab(CompanionBehaviour script)
        {
            EditorGUILayout.LabelField("Wander Around", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(wanderAround);
            if (script.wanderAround) {
                EditorGUILayout.PropertyField(wanderPointTime);
                InboraStudioAIEditor.CheckDisableWithRootMotion(script.InboraStudio, ref wanderMoveSpeed);
                EditorGUILayout.Space(10);
                EditorGUILayout.PropertyField(wanderIdleAnims);
                EditorGUILayout.Space();

                InboraStudioAIEditor.DrawPopupProperty(script.InboraStudio.anim, "Wander Move Anim", "wanderMoveAnim", ref script.wanderMoveAnim, scripts);
            }

            EditorGUILayout.Space(10);

            EditorGUILayout.LabelField("Skin & Contact", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(moveAwayOnContact);
            if (script.moveAwayOnContact) {
                EditorGUILayout.PropertyField(layersToAvoid);
                EditorGUILayout.PropertyField(walkOnMoveAway);
            }
        }

        void DrawAudiosTab(CompanionBehaviour script)
        {
            EditorGUILayout.LabelField("Audios", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(playAudioForIdleAndWander);
            
            if (script.playAudioForIdleAndWander) {
                EditorGUILayout.PropertyField(audioTimer);
                EditorGUILayout.Space();
            }

            EditorGUILayout.PropertyField(playAudioOnFollowState);
        }

        void DrawEventsTab()
        {
            EditorGUILayout.LabelField("State Events", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(onStateEnter);
            EditorGUILayout.PropertyField(onStateExit);
            EditorGUILayout.Space(10);

            EditorGUILayout.LabelField("Distance Event", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(fireEventDistance);
            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(exceededDistanceEvent);
        }

        #endregion
    }
}