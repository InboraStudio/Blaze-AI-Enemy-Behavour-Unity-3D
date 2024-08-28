using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;

namespace InboraStudioAISpace
{
    [AddComponentMenu("InboraStudio AI/Flee Behaviour")]
    public class FleeBehaviour : InboraStudioBehaviour
    {   
        #region PROPERTIES
        [Tooltip("If enabled, the AI will run back and forth within it's own radius as if it's on fire. This mode doesn't require that the AI has a target. If disabled, normal flee behaviour where the AI checks where the target is and runs in the opposite direction.")]
        public bool runAroundMode = false;
        
        [Tooltip("The distance to run to away from the target OR the radius in which the AI runs around if Run Around Mode is enabled.")]
        public float distanceRun = 10;

        public float moveSpeed = 5;
        public float turnSpeed = 5;

        public string moveAnim;
        public float moveAnimT = 0.25f;

        [Tooltip("Go to a specific position.")]
        public bool goToPosition;
        [Tooltip("Set the specific position to go to.")]
        public Vector3 setPosition;
        [Tooltip("Shows the specific position point in the scene view (green circle marked as flee position)")]
        public bool showPosition;
        [Tooltip("Fire an event when the specific position is reached.")]
        public UnityEvent reachEvent;

        [Tooltip("Play an audio when fleeing. Set the audio in the audio scriptable. Fleeing array.")]
        public bool playAudio;
        [Tooltip("If enabled, an audio will always play when fleeing. If set to false, there is a 50/50 chance whether an audio will be played or not.")]
        public bool alwaysPlayAudio;

        public UnityEvent onStateEnter;
        public UnityEvent onStateExit;

        #endregion

        #region BEHAVIOUR VARS

        public InboraStudioAI InboraStudio { private set; get; }
        bool isFirstRun = true;
        GameObject lastEnemy;
        Vector3 fleePosition;

        public Vector3 fleeingTo {
            get { return fleePosition; }
        }

        bool isMoving;
        float cornersDist;
        int _framesElapsed = 0;

        int savedVisionLayers;
        bool savedMovementTurningState;
        bool savedAudioManagerState;


        #endregion

        #region MAIN METHODS

        public virtual void OnStart()
        {
            isFirstRun = false;
            InboraStudio = GetComponent<InboraStudioAI>();
        }

        public override void Open()
        {
            if (isFirstRun) {
                OnStart();
            }

            onStateEnter.Invoke();
            _framesElapsed = 5;

            if (runAroundMode) {
                DisableInboraStudioProperties();
            }
            
            if (InboraStudio == null) {
                Debug.LogWarning($"No InboraStudio AI component found in the gameobject: {gameObject.name}. AI behaviour will have issues.");
                return;
            }

            InboraStudio.isFleeing = true;
            PlayAudio();
        }

        public override void Close()
        {
            if (InboraStudio == null) {
                return;
            }

            // reset flags, except if hit state
            if (InboraStudio.state != InboraStudioAI.State.hit) {
                lastEnemy = null;
                InboraStudio.isFleeing = false;
            }

            onStateExit.Invoke();
            
            if (runAroundMode) {
                ReturnInboraStudioProperties();
            }
        }

        public override void Main()
        {
            if (runAroundMode) {
                RunAround();
                return;
            }

            if (InboraStudio.enemyToAttack != null) {
                lastEnemy = InboraStudio.enemyToAttack;
                Flee();
                return;
            }
            
            if (lastEnemy != null) {
                Flee();
                return;
            }

            InboraStudio.SetState(InboraStudioAI.State.alert);
        }

        void OnValidate()
        {
            if (InboraStudio == null) {
                InboraStudio = GetComponent<InboraStudioAI>();
            }

            if (goToPosition && setPosition == Vector3.zero) {
                setPosition = transform.position;
            }
        }

        #if UNITY_EDITOR
        void OnDrawGizmosSelected()
        {
            if (InboraStudio == null) return;

            if (goToPosition && showPosition) 
            {
                if (InboraStudio.groundLayers.value == 0) {
                    Debug.LogWarning("Ground layers property not set. Make sure to set the ground layers in the main InboraStudio inspector (general tab) in order to see the flee points visually.");
                }

                RaycastHit hit;
                if (Physics.Raycast(setPosition, -Vector3.up, out hit, Mathf.Infinity, InboraStudio.groundLayers)) {
                    Debug.DrawRay(transform.position, setPosition - transform.position, new Color(1f, 0.3f, 0f), 0.1f);
                    Debug.DrawRay(setPosition, hit.point - setPosition, new Color(1f, 0.3f, 0f), 0.1f);

                    UnityEditor.Handles.color = new Color(0.3f, 1f, 0f);
                    UnityEditor.Handles.DrawWireDisc(hit.point, InboraStudio.transform.up, 0.5f);
                    UnityEditor.Handles.Label(hit.point + new Vector3(0, 1, 0), "Flee Position");
                }
            }
        }
        #endif

        #endregion

        #region BEHAVIOUR

        public virtual void RunAround()
        {
            if (isMoving) 
            {
                Move(fleePosition);
                return;
            }
            
            for (int i=0; i<5; i++) {
                fleePosition = InboraStudio.RandomSpherePoint(transform.position, distanceRun);
                if (!InboraStudio.IsPathReachable(fleePosition)) {
                    continue;
                }
            }

            Move(fleePosition);
        }

        // temporarily disable vision to avoid interrupting the behaviour if taking place during normal state
        public virtual void DisableInboraStudioProperties()
        {
            savedVisionLayers = InboraStudio.vision.hostileAndAlertLayers;
            InboraStudio.vision.hostileAndAlertLayers = 0;

            savedMovementTurningState = InboraStudio.waypoints.useMovementTurning;
            InboraStudio.waypoints.useMovementTurning = false;

            if (InboraStudioAIAudioManager.instance != null) 
            {
                List<InboraStudioAI> list = InboraStudioAIAudioManager.instance.ReturnList();
                
                if (list.Contains(InboraStudio)) {
                    savedAudioManagerState = true;
                    InboraStudioAIAudioManager.instance.RemoveFromManager(InboraStudio);
                }
                else {
                    savedAudioManagerState = false;
                }
            }
        }

        public virtual void ReturnInboraStudioProperties()
        {
            InboraStudio.vision.hostileAndAlertLayers = savedVisionLayers;
            InboraStudio.waypoints.useMovementTurning = savedMovementTurningState;
            
            if (InboraStudioAIAudioManager.instance != null && savedAudioManagerState) {
                InboraStudioAIAudioManager.instance.AddToManager(InboraStudio);
            }
        }

        public virtual void Flee()
        {
            if (goToPosition) 
            {
                if (_framesElapsed >= 5) 
                {
                    cornersDist = InboraStudio.CalculateCornersDistanceFrom(transform.position, setPosition);
                    _framesElapsed = 0;
                }
                else {
                    _framesElapsed++;
                }
                
                float radius = InboraStudio.navmeshAgent.radius * 2;
                if (cornersDist <= radius) 
                {
                    reachEvent.Invoke();
                    InboraStudio.SetState(InboraStudioAI.State.alert);
                    return;
                }

                Move(setPosition);
                return;
            }

            
            if (isMoving) 
            {
                Move(fleePosition);
                return;
            }


            float distance = (transform.position - lastEnemy.transform.position).sqrMagnitude;
            if (distance >= distanceRun * distanceRun) 
            {
                if (InboraStudio.enemyToAttack == null) 
                {
                    InboraStudio.SetState(InboraStudioAI.State.alert);
                    return;
                }
            }

            
            Vector3 fleeDir = lastEnemy.transform.position - transform.position;
            fleePosition = transform.position - fleeDir;

            if (!InboraStudio.IsPathReachable(fleePosition)) {
                fleePosition = InboraStudio.RandomSpherePoint(transform.position, distanceRun);
            }

            Move(fleePosition);
        }

        public virtual void Move(Vector3 pos)
        {
            if (InboraStudio.MoveTo(pos, moveSpeed, turnSpeed, moveAnim, moveAnimT)) {
                isMoving = false;
                return;
            }

            isMoving = true;
        }

        void PlayAudio()
        {
            if (InboraStudio == null) return;
            if (InboraStudio.IsAudioScriptableEmpty() || !playAudio) return;

            if (!alwaysPlayAudio) {
                int rand = Random.Range(0, 2);
                if (rand == 0) {
                    return;
                }
            }

            InboraStudio.PlayAudio(InboraStudio.audioScriptable.GetAudio(AudioScriptable.AudioType.Fleeing));
        }

        #endregion
    }
}