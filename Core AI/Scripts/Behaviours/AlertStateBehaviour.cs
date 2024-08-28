using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace InboraStudioAISpace
{
    [AddComponentMenu("InboraStudio AI/Alert State Behaviour")]
    public class AlertStateBehaviour : InboraStudioBehaviour
    {
        #region PROPERTIES

        [Min(0), Tooltip("Won't be considered if root motion is used.")]
        public float moveSpeed = 3f;
        [Min(0)]
        public float turnSpeed = 5f;

        [Tooltip("Add animations for idle. One will be chosen at random. If only one is added then only that will play.")]
        public string[] idleAnim;
        [Tooltip("Movement animation.")]
        public string moveAnim;
        [Min(0), Tooltip("Animation transition time from idle to move and vice versa.")]
        public float animT = 0.25f;
        
        [Min(0), Tooltip("Time in seconds to stay in idle before going to the next waypoint. Will generate a random number between the two values. For a fixed value make both inputs the same.")]
        public Vector2 idleTime = new Vector2(5f, 5f);

        [Tooltip("Will tell the Audio Manager component to play patrol audios in this state.")]
        public bool playPatrolAudio;

        [Tooltip("Make the AI return to normal state after a specific amount of time. If disabled, the AI will never go back to normal state once it's out of it.")]
        public bool returnToNormal;
        [Tooltip("The amount of time (seconds) to pass in alert state before returning to normal state.")]
        public float timeToReturnNormal;
        [Tooltip("The time duration (seconds) transitioning to normal state.")]
        public float returningDuration;
        [Tooltip("The animation name to play when returning to normal state.")]
        public string returningAnim;
        [Tooltip("Animation transition time from current animation to the returning animation.")]
        public float returningAnimT = 0.25f;
        [Tooltip("If true will play an audio when returning to normal. Set the audios in the audio scriptable in the General tab in InboraStudio AI.")]
        public bool playAudioOnReturn;
        
        [Tooltip("Avoid facing so closesly to an obstacle when reaching waypoint.")]
        public bool avoidFacingObstacles;
        [Tooltip("The layers of the obstacles.")]
        public LayerMask obstacleLayers = Physics.AllLayers;
        [Tooltip("How far to check for an obstacle.")]
        public float obstacleRayDistance = 3f;
        [Tooltip("Position the ray relative to character.")]
        public Vector3 obstacleRayOffset;
        [Tooltip("Will be shown in scene view as a yellow ray.")]
        public bool showObstacleRay;

        public UnityEvent onStateEnter;
        public UnityEvent onStateExit;

        #endregion

        #region BEHAVIOUR VARS

        public InboraStudioAI InboraStudio {private set; get; }
        bool isFirstRun = true;

        Vector3 waypoint;

        bool isIdle;
        bool isReturningToNormal;
        bool movedToLocation;
        bool isOffMeshByPassed;

        float returnToNormalTimer = 0f;
        float _turnToWP = 0f;

        #endregion

        #region MAIN METHODS
        
        public virtual void OnStart() 
        {   
            InboraStudio = GetComponent<InboraStudioAI>();
            isFirstRun = false;
            SetEndDestination();
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
            ResetReturnToNormal();
            isIdle = false;
            isOffMeshByPassed = false;
            onStateExit.Invoke();

            if (InboraStudio == null) {
                return;
            }
            
            InboraStudio.stayAlertUntilPos = false;
        }

        public override void Main()
        {
            // end destination is set by InboraStudio.NextWayPoint or InboraStudio.RandomNavmeshLocation
            // OR if forced to move to a specific location using MoveToLocation() inside InboraStudio.cs
            waypoint = InboraStudio.endDestination;
            SetIdleState();

            // if forced to stay idle
            if (InboraStudio.stayIdle) {
                StayIdle();
                ReturnToNormalTimer();
                return;
            }

            if (OffMeshToBypass()) {
                return;
            }

            // check if InboraStudio has been called to move to a certain location
            movedToLocation = InboraStudio.movedToLocation;
            if (movedToLocation && isIdle) {
                ForceMove();
            }

            // correct waypoint if move to location cancelled
            CorrectWaypoint();
            MoveToPoint();
            ReturnToNormalTimer();

            if (avoidFacingObstacles) {
                ObstacleRay();
            }
        }

        void OnDrawGizmosSelected()
        {
            if (showObstacleRay) {
                Debug.DrawRay(transform.position + obstacleRayOffset, transform.TransformDirection(Vector3.forward) * obstacleRayDistance, Color.yellow);
            }
        }

        void OnValidate()
        {
            if (InboraStudio == null) {
                InboraStudio = GetComponent<InboraStudioAI>();
            }
        }

        #endregion
        
        #region BEHAVIOUR
        
        // move AI to waypoint
        void MoveToPoint()
        {
            // if AI reached waypoint and is idle -> return
            if (isIdle || isReturningToNormal) return;


            // check if using randomized waypoints
            if (InboraStudio.waypoints.randomize) {
                RandomizedWaypointsMove();
                return;
            }

            // if using normal pre-set waypoints
            PreSetWaypointsMove();
        }

        // move to the pre-set waypoints
        public virtual void PreSetWaypointsMove()
        {   
            // MoveTo() moves to point and returns true when reaches destination -> false if not
            if (InboraStudio.MoveTo(waypoint, moveSpeed, turnSpeed, moveAnim, animT)) 
            {
                // if was moving to a certain location then there's no waypoint rotation -> go idle instantly
                if (movedToLocation) 
                {
                    StartCoroutine("Idle");
                    return;
                }

                // CheckWayPointRotation() returns true if there is a waypoint rotation
                if (InboraStudio.CheckWayPointRotation()) 
                {
                    _turnToWP += Time.deltaTime;

                    // play idle anim while waiting for time before turn
                    if (_turnToWP < InboraStudio.waypoints.timeBeforeTurning) {
                        string waitAnim = "";

                        if (idleAnim.Length > 0) waitAnim = idleAnim[0];

                        InboraStudio.animManager.Play(waitAnim, animT);
                        return;
                    }
                }

                // WaypointTurning() turns AI to waypoint rotation and returns true when done
                if (InboraStudio.WayPointTurning()) 
                {
                    StartCoroutine("Idle");
                }

                return;
            }
            
            // code below runs if not reached position yet 
            
            if (isIdle) {
                ForceMove();
            }

            // checks if the passed location in MoveTo() is reachable
            if (!InboraStudio.isPathReachable) {
                InboraStudio.NextWayPoint();
            }
        }

        // move to random point
        public virtual void RandomizedWaypointsMove()
        {
            // MoveTo() moves to point and returns true when reaches destination
            if (InboraStudio.MoveTo(waypoint, moveSpeed, turnSpeed, moveAnim, animT)) 
            {
                StartCoroutine("Idle");
                return;
            }
            
            // code below runs if not reached position yet 

            if (isIdle) {
                ForceMove();
            }

            if (!InboraStudio.isPathReachable) {
                SetEndDestination();
            }
        }

        // reached waypoint location so turn idle for a time
        public virtual IEnumerator Idle()
        {
            isIdle = true;
            movedToLocation = false;
            _turnToWP = 0f;

            // play the idle anim
            string animToPlay = "";

            if (idleAnim.Length > 0) {
                int animIndex = Random.Range(0, idleAnim.Length);
                animToPlay = idleAnim[animIndex];
            }

            InboraStudio.animManager.Play(animToPlay, animT);
            if (InboraStudio.stayIdle) yield break;

            // set the wait time
            float _idleTime = Random.Range(idleTime.x, idleTime.y);
            SetEndDestination();

            // reset this value -> used for if AI has been called to move to a location 
            // so don't run the return to normal timer until it reaches position
            InboraStudio.stayAlertUntilPos = false;

            yield return new WaitForSeconds(_idleTime);
            isIdle = false;
        }

        public virtual void SetEndDestination()
        {
            // * check API flags in order not to avoid choosing next destination in waypoint *

            // if forced to move to a location
            if (InboraStudio.movedToLocation) {
                ForceMove();
                return;
            }

            // if randomize then get a random navmesh location
            if (InboraStudio.waypoints.randomize) {
                InboraStudio.RandomNavMeshLocation(InboraStudio.waypoints.minAndMaxLevelDiff.x, InboraStudio.waypoints.minAndMaxLevelDiff.y);
                return;
            }
            
            // if reached this point -> means randomize is off so sets the waypointIndex var to the next waypoint
            InboraStudio.NextWayPoint();
        }

        // count down to return to normal state
        void ReturnToNormalTimer()
        {
            if (!returnToNormal || isReturningToNormal || InboraStudio.stayAlertUntilPos) return;

            returnToNormalTimer += Time.deltaTime;
            if (returnToNormalTimer < timeToReturnNormal) return;

            if (!isReturningToNormal) {
                StartCoroutine("ReturnToNormal");
            }
        }

        // play animation and audio and wait for returning duration
        public virtual IEnumerator ReturnToNormal()
        {
            isReturningToNormal = true;
            isIdle = true;
            returnToNormalTimer = 0;

            // play return anim
            InboraStudio.animManager.Play(returningAnim, returningAnimT);
            
            // play return audios
            if (playAudioOnReturn) {
                if (!InboraStudio.IsAudioScriptableEmpty()) {
                    InboraStudio.PlayAudio(InboraStudio.audioScriptable.GetAudio(AudioScriptable.AudioType.ReturningToNormalState));
                }
            }

            yield return new WaitForSeconds(returningDuration);

            // change the state to normal
            InboraStudio.SetState(InboraStudioAI.State.normal);

            isReturningToNormal = false;
            isIdle = false;
        }

        void ResetReturnToNormal()
        {
            isReturningToNormal = false;
            returnToNormalTimer = 0;
        }

        // fire ray to avoid AI standing too close facing obstacles
        void ObstacleRay()
        {
            float distance = (waypoint - transform.position).sqrMagnitude;
            float minDistance = obstacleRayDistance * obstacleRayDistance;
            
            if (distance <= minDistance) {
                // AI should be facing the waypoint
                if (Vector3.Dot((waypoint - transform.position).normalized, transform.forward) < 0.8f) {
                    return;
                }

                RaycastHit hit;
                if (Physics.Raycast(transform.position + obstacleRayOffset, transform.TransformDirection(Vector3.forward), out hit, obstacleRayDistance, obstacleLayers))
                {
                    isIdle = true;
                    StartCoroutine("Idle");
                }
            }
        }

        // correct waypoint if move to location cancelled
        void CorrectWaypoint()
        {
            if (!InboraStudio.ignoreMoveToLocation) return;
            
            SetEndDestination();
            InboraStudio.ignoreMoveToLocation = false;
        }

        bool OffMeshToBypass()
        {
            if (!InboraStudio.IfShouldIgnoreOffMesh()) {
                return false;
            }

            if (isOffMeshByPassed) {
                return false;
            }

            isOffMeshByPassed = true;
            
            if (InboraStudio.waypoints.waypoints.Count <= 1 && !isIdle) {
                StartCoroutine("Idle");
                return true;
            }

            if (isIdle) {
                return false;
            }
            
            InboraStudio.NextWayPoint();
            InboraStudio.navmeshAgent.Warp(transform.position);
            isOffMeshByPassed = false;
            
            return true;
        }
        
        #endregion

        #region USED WITH PUBLIC METHODS
        
        // force the AI behaviour to quit idle and move
        public virtual void ForceMove()
        {
            StopAllCoroutines();
            isIdle = false;
        }

        // tell the AI behaviour to stay idle
        public virtual void StayIdle()
        {
            if (isIdle) return;
            StartCoroutine("Idle");
        }

        // returns true or false whether the AI is idle
        void SetIdleState()
        {
            if (isIdle || InboraStudio.stayIdle) {
                InboraStudio.isIdle = true;
                return;
            }

            InboraStudio.isIdle = false;
        }
        
        #endregion   
    }
}