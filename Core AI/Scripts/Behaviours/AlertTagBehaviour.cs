using UnityEngine;
using UnityEngine.Events;

namespace InboraStudioAISpace 
{
    [AddComponentMenu("InboraStudio AI/Alert Tag Behaviour")]
    public class AlertTagBehaviour : InboraStudioBehaviour
    {
        #region PROPERTIES

        public bool checkLocation;

        [Tooltip("Animation name to play the moment the AI sees the alert tag. Will play before moving to location (if checked). If this is empty, it'll be ignored.")]
        public string onSightAnim;

        [Min(0), Tooltip("The amount of time to pass playing the animation on sight.")]
        public float onSightDuration = 3f;

        [Tooltip("If check location is true, this animation will play when reaching the location. If check location is false, the animation will play instantly.")]
        public string reachedLocationAnim;
        
        [Min(0), Tooltip("The amount of time to pass playing the reached location animation.")]
        public float reachedLocationDuration = 2f;
        
        [Min(0), Tooltip("The transition time from current animation to either move/finished animations.")]
        public float animT = 0.25f;
        
        [Tooltip("Set your audios in the audio scriptable in the General Tab in InboraStudio AI.")]
        public bool playAudio;
        public int audioIndex;
        
        [Tooltip("Will call other agents to the location of the alert object.")]
        public bool callOtherAgents;
        [Min(0)]
        public float callRange = 10f;
        [Tooltip("Shows the call range as a white wire sphere in scene view.")]
        public bool showCallRange;
        public LayerMask otherAgentsLayers;
        [Tooltip("If enabled, the call will pass through colliders. If disabled, the call won't pass through the layers set in [Layers To Detect] in Vision.")]
        public bool callPassesColliders;
        [Tooltip("If enabled, the AI will call others not to the exact point but a randomized position within the destination radius. This is to avoid having the AIs all move to the exact same point.")]
        public bool randomizeCallPosition;

        public UnityEvent onStateEnter;
        public UnityEvent onStateExit;

        #endregion

        #region BEHAVIOUR VARS

        public InboraStudioAI InboraStudio { private set; get; }
        AlertStateBehaviour alertStateBehaviour;
        bool isFirstRun = true;
        
        bool audioPlayed;
        bool calledAgents;
        bool onSightDone;
        bool isOffMeshBypassed;
        
        float _durationTimer;
        float onSightTimer = 0;

        #endregion
        
        #region GARBAGE REDUCTION
        
        Collider[] callOthersHitArr = new Collider[20];
        
        #endregion

        #region MAIN METHODS

        public virtual void OnStart()
        {
            isFirstRun = false;
            InboraStudio = GetComponent<InboraStudioAI>();
            alertStateBehaviour = GetComponent<AlertStateBehaviour>();
        }

        public override void Open()
        {
            if (isFirstRun) {
                OnStart();
            }

            onStateEnter.Invoke();

            if (InboraStudio == null) {
                Debug.LogWarning($"No InboraStudio AI component found in the gameobject: {gameObject.name}. AI behaviour will have issues.");
            }
        }

        public override void Close()
        {
            audioPlayed = false;
            calledAgents = false;
            onSightDone = false;
            isOffMeshBypassed = false;
            _durationTimer = 0;
            onSightTimer = 0;
            onStateExit.Invoke();
        }

        public override void Main()
        {
            // check if alert state behaviour isn't added
            if (alertStateBehaviour == null) {
                Debug.Log("Alert State Behaviour must be added for Alert Tag behaviour to function. Please add the alert state behaviour.");
                return;
            }

            if (OffMeshToBypass()) {
                return;
            }

            // play audio
            if (playAudio) {
                PlayAudio();
            }

            // call nearby agents
            if (callOtherAgents) {
                CallAgents();
            }

            // play the on sight anim -> don't continue until it returns true
            if (!onSightDone) {
                if (!PlayOnSightAnim()) {
                    return;
                }
            }

            // see if location needs to be checked
            if (!checkLocation) {
                InboraStudio.animManager.Play(reachedLocationAnim, animT);
                DurationTimer();
                return;
            }
            
            // go to location if check location is true
            if (InboraStudio.MoveTo(InboraStudio.sawAlertTagPos, alertStateBehaviour.moveSpeed, alertStateBehaviour.turnSpeed, alertStateBehaviour.moveAnim, alertStateBehaviour.animT)) {
                InboraStudio.animManager.Play(reachedLocationAnim, animT);
                DurationTimer();
            }
        }

        void OnDrawGizmosSelected()
        {
            if (!showCallRange || !callOtherAgents) {
                return;
            }

            Gizmos.color = Color.white;
            Gizmos.DrawWireSphere(transform.position, callRange);
        }

        void OnValidate()
        {
            if (InboraStudio == null) {
                InboraStudio = GetComponent<InboraStudioAI>();
            }
        }

        #endregion

        #region BEHAVIOUR METHODS

        // play the audio
        public virtual void PlayAudio()
        {
            if (!playAudio) return;
            if (audioPlayed) return;
            if (InboraStudio.IsAudioScriptableEmpty()) return;

            if (InboraStudio.PlayAudio(InboraStudio.audioScriptable.GetAudio(AudioScriptable.AudioType.AlertTags, audioIndex))) {
                audioPlayed = true;
            }
        }

        // call agents to position
        public virtual void CallAgents()
        {
            if (calledAgents) return;
            
            System.Array.Clear(callOthersHitArr, 0, 20);
            int agentsNum = Physics.OverlapSphereNonAlloc(transform.position, callRange, callOthersHitArr, otherAgentsLayers);
        
            for (int i=0; i<agentsNum; i++) {
                // if this same object then -> skip to next iteration
                if (callOthersHitArr[i].transform.IsChildOf(transform) || transform.IsChildOf(callOthersHitArr[i].transform)) {
                    continue;
                }

                InboraStudioAI InboraStudioScript = callOthersHitArr[i].transform.GetComponent<InboraStudioAI>();
                if (InboraStudioScript == null) continue;

                // set the end point that we'll call the other AIs to
                Vector3 point = Vector3.zero;
                Collider objColl = InboraStudio.sawAlertTagObject.GetComponent<Collider>();
                float range = objColl.bounds.size.x + objColl.bounds.size.z;

                if (randomizeCallPosition) {
                    point = InboraStudio.RandomSpherePoint(InboraStudio.sawAlertTagPos, range);
                }
                else {
                    point = InboraStudio.sawAlertTagPos;
                }

                // check there are no obstacles hindering the call
                if (!callPassesColliders) 
                {
                    RaycastHit rayHit;
                    
                    Transform target = callOthersHitArr[i].transform;
                    Collider currentColl = callOthersHitArr[i];

                    Vector3 currentCenter = new Vector3(transform.position.x, transform.position.y + InboraStudio.vision.visionPosition.y, transform.position.z);
                    Vector3 otherAgentCenter = currentColl.ClosestPoint(currentCenter);
                    Vector3 dir = (otherAgentCenter - currentCenter);

                    float rayDistance = Vector3.Distance(otherAgentCenter, transform.position) + 3;
                    if (Physics.Raycast(currentCenter, dir, out rayHit, rayDistance, InboraStudio.vision.layersToDetect)) {
                        if (!rayHit.transform.IsChildOf(target) && !target.IsChildOf(rayHit.transform)) {
                            continue;
                        }
                    }
                }
                
                InboraStudioScript.ChangeState("alert");
                InboraStudioScript.MoveToLocation(point);
            }

            calledAgents = true;
        }

        // play the on sight animation
        public virtual bool PlayOnSightAnim()
        {
            if (onSightAnim.Length <= 0 || onSightAnim == null) {
                onSightDone = true;
                return true;
            }

            InboraStudio.animManager.Play(onSightAnim, animT);

            onSightTimer += Time.deltaTime;
            if (onSightTimer >= onSightDuration) {
                onSightDone = true;
                onSightTimer = 0;
                return true;
            }

            return false;
        }

        // the duration before exiting this state
        void DurationTimer()
        {
            _durationTimer += Time.deltaTime;
            if (_durationTimer >= reachedLocationDuration) {
                _durationTimer = 0;
                InboraStudio.ChangeState("alert");
            }
        }

        bool OffMeshToBypass()
        {
            if (!InboraStudio.IfShouldIgnoreOffMesh()) {
                return false;
            }

            if (isOffMeshBypassed) return true;            
            isOffMeshBypassed = true;
            
            InboraStudio.navmeshAgent.Warp(transform.position);
            InboraStudio.ChangeState("alert");
            
            return true;
        }

        #endregion
    }
}