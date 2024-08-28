using UnityEngine;
using UnityEngine.Events;

namespace InboraStudioAISpace
{
    [AddComponentMenu("InboraStudio AI/Distracted State Behaviour")]
    public class DistractedStateBehaviour : InboraStudioBehaviour
    {
        #region PROPERTIES

        [Header("Reaction Time"), Tooltip("Time to pass (seconds) before turning to distraction location.")]
        public float timeToReact = 0.2f;

        [Header("Checking Location"), Tooltip("If enabled, AI will move to distraction location.")]
        public bool checkLocation = true;
        [Tooltip("Time to pass (seconds) before moving to check location.")]
        public float timeBeforeMovingToLocation = 1f;
        [Tooltip("Animation to play when reaches the distraction destination.")]
        public string checkAnim;
        public float checkAnimT = 0.25f;
        [Tooltip("Amount of time (seconds) to stay in distraction destination before going back to patrolling.")]
        public float timeToCheck = 5f;
        [Tooltip("If enabled, the AI will go to a random point within the distraction location.")]
        public bool randomizePoint;
        [Min(0), Tooltip("Set the randomize radius.")]
        public float randomizeRadius = 5;
        
        [Header("Search Radius"), Tooltip("If enabled, the AI after checking the distraction location will randomly search points within the radius of the distraction location. The radius is the AI's NavMesh Agent Height x 2. Check Location must be enabled for this to work.")]
        public bool searchLocationRadius;
        [Range(1, 10), Tooltip("The amount of random points to search.")]
        public int searchPoints = 3;
        [Tooltip("The animation name to play on each search point.")]
        public string searchPointAnim;
        [Min(0), Tooltip("The amount of time to wait in each search point.")]
        public float pointWaitTime = 3;
        [Tooltip("Animation to play after going through all search points. This is the exiting animation.")]
        public string endSearchAnim;
        [Min(0), Tooltip("Set how long you want the end animation to be playing.")]
        public float endSearchAnimTime = 3;
        public float searchAnimsT = 0.25f;

        [Tooltip("Play audio when AI reaches the distraction location.")]
        public bool playAudioOnCheckLocation;
        [Tooltip("Play an audio when AI begins searching.")]
        public bool playAudioOnSearchStart;
        [Tooltip("Play an audio when AI finishes searching.")]
        public bool playAudioOnSearchEnd;

        public UnityEvent onStateEnter;
        public UnityEvent onStateExit;
        
        #endregion

        #region BEHAVIOUR VARS

        public InboraStudioAI InboraStudio { get; private set; }
        bool isFirstRun = true;
        NormalStateBehaviour normalStateBehaviour;
        AlertStateBehaviour alertStateBehaviour;
        
        float _timeToCheck = 0f;
        float _timeToReact = 0f;
        float _timeBeforeMovingToLocation = 0f;
        float moveSpeed = 0;
        float turnSpeed = 0;

        bool turnedToLocation;
        bool playedLocationAudio;
        bool isIdle;
        bool isSearching;

        string moveAnim = "";
        string leftTurn = "";
        string rightTurn = "";

        int searchIndex = 0;

        Vector3 distractionPoint;
        Vector3 searchPointLocation;
        Vector3 lastPoint;

        #endregion

        #region MAIN METHODS

        public virtual void OnStart()
        {
            isFirstRun = false;
            InboraStudio = GetComponent<InboraStudioAI>();
            normalStateBehaviour = GetComponent<NormalStateBehaviour>();
            alertStateBehaviour = GetComponent<AlertStateBehaviour>();

            if (normalStateBehaviour == null) {
                Debug.Log("Distracted State Behaviour tried to get Normal State Behaviour component but found nothing. It's important to set it manually to get the movement and turning animations and speeds.");
            }

            if (alertStateBehaviour == null && InboraStudio.turnAlertOnDistract) {
                InboraStudio.PrintWarning(InboraStudio.warnAnomaly, "Distracted State Behaviour tried to get Alert State Behaviour component (because [Turn Alert On Distract] is enabled) but found nothing. So AI state has been reset to normal state. It's important to add the alert behaviour to set/get the movement and turning animations and speeds.");
                InboraStudio.SetState(InboraStudioAI.State.normal);
                return;
            }
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
            ResetDistraction();

            if (InboraStudio == null) {
                return;
            }
            
            if (InboraStudio.waypoints.randomize) {
                InboraStudio.RandomNavMeshLocation(InboraStudio.waypoints.minAndMaxLevelDiff.x, InboraStudio.waypoints.minAndMaxLevelDiff.y);
            }
            else {
                InboraStudio.NextWayPoint();
            }

            onStateExit.Invoke();
        }

        void OnValidate()
        {
            if (InboraStudio == null) {
                InboraStudio = GetComponent<InboraStudioAI>();
            }
        }

        public override void Main()
        {   
            if (alertStateBehaviour == null && InboraStudio.turnAlertOnDistract) {
                InboraStudio.PrintWarning(InboraStudio.warnAnomaly, "Distracted State Behaviour tried to get Alert State Behaviour component (because [Turn Alert On Distract] is enabled) but found nothing. So AI state has been reset to normal state. It's important to add the alert behaviour to set/get the movement and turning animations and speeds.");
                InboraStudio.SetState(InboraStudioAI.State.normal);
                return;
            }

            // if forced to stay idle by InboraStudio public method
            if (InboraStudio.stayIdle) {
                ReachedDistractionLocation();
                return;
            }


            GetSpeedsAndTurns();
            
            if (InboraStudio.IfShouldIgnoreOffMesh()) {
                IgnoreOffMeshBehaviour();
                return;
            }

            if (!PreparePoint()) {
                return;
            }


            // turn to distraction first
            if (!turnedToLocation && !isIdle || !turnedToLocation && !checkLocation) {
                _timeToReact += Time.deltaTime;
                
                if (_timeToReact >= timeToReact) {
                    // TurnTo() turns the agent and returns true when fully turned to point
                    if (InboraStudio.TurnTo(distractionPoint, leftTurn, rightTurn, 0.25f)) {
                        turnedToLocation = true;
                        _timeToReact = 0f;
                    }
                }
                else {
                    // play idle anim
                    if (normalStateBehaviour.idleAnim.Length > 0) {
                        InboraStudio.animManager.Play(normalStateBehaviour.idleAnim[0], checkAnimT);
                    }
                }
            }


            // can't go further if haven't completely turned
            if (!turnedToLocation && !isIdle || !turnedToLocation && !checkLocation) {
                return;
            }


            _timeBeforeMovingToLocation += Time.deltaTime;

            if (_timeBeforeMovingToLocation < timeBeforeMovingToLocation) {
                return;
            }


            // AI has reached distraction location and is now searching the radius
            if (isSearching) {
                if (InboraStudio.MoveTo(searchPointLocation, moveSpeed, turnSpeed, moveAnim)) {
                    // stay idle
                    if (!IsSearchPointIdleFinished()) {
                        return;
                    }

                    if (searchIndex < searchPoints) {
                        SetSearchPoint();
                        return;
                    }

                    // reaching this line means the AI has went through all search points and is time to exit
                    EndSearchExit();
                    return;
                }

                return;
            }


            // if should check location
            if (checkLocation) {
                // MoveTo() moves the agent to the destination and returns true when reaches destination
                if (InboraStudio.MoveTo(distractionPoint, moveSpeed, turnSpeed, moveAnim)) {
                    ReachedDistractionLocation();
                }
                else {
                    isIdle = false;
                }
            }
            else {
                turnedToLocation = false;
                isIdle = true;
                _timeBeforeMovingToLocation = 0f;
                InboraStudio.SetState(InboraStudio.previousState);
            }


            SetIdleState();
        }

        #endregion

        #region CHECK LOCATION

        // get move & turn speeds/animations
        void GetSpeedsAndTurns()
        {
            leftTurn = InboraStudio.waypoints.leftTurnAnimNormal;
            rightTurn = InboraStudio.waypoints.rightTurnAnimNormal;
            
            if (InboraStudio.previousState == InboraStudioAI.State.normal) {
                moveSpeed = normalStateBehaviour.moveSpeed;
                turnSpeed = normalStateBehaviour.turnSpeed;
                moveAnim = normalStateBehaviour.moveAnim;

                if (InboraStudio.waypoints.useTurnAnims) return;
                if (normalStateBehaviour.idleAnim.Length <= 0) return;

                leftTurn = normalStateBehaviour.idleAnim[0];
                rightTurn = normalStateBehaviour.idleAnim[0];
            }

            if (InboraStudio.previousState == InboraStudioAI.State.alert) {
                moveSpeed = alertStateBehaviour.moveSpeed;
                turnSpeed = alertStateBehaviour.turnSpeed;
                moveAnim = alertStateBehaviour.moveAnim;

                if (InboraStudio.waypoints.useTurnAnims) return;
                if (alertStateBehaviour.idleAnim.Length <= 0) return;

                leftTurn = alertStateBehaviour.idleAnim[0];
                rightTurn = alertStateBehaviour.idleAnim[0];
            }
        }

        // select a random audio to play when reaching the distraction location
        void PlayAudioOnCheckLocation()
        {
            if (playedLocationAudio) {
                return;
            }


            if (InboraStudio.IsAudioScriptableEmpty()) {
                return;
            }

                
            if (InboraStudio.PlayAudio(InboraStudio.audioScriptable.GetAudio(AudioScriptable.AudioType.DistractionCheckLocation))) {
                playedLocationAudio = true;
            }
        }

        // exit method out of distracted state when reaching location
        public virtual void ReachedDistractionLocation()
        {
            // play audio
            if (playAudioOnCheckLocation) {
                PlayAudioOnCheckLocation();
            }


            // play check location animation
            InboraStudio.animManager.Play(checkAnim, checkAnimT);

            // run the timer
            _timeToCheck += Time.deltaTime;
            

            if (_timeToCheck >= timeToCheck) {
                if (searchLocationRadius) {
                    PlaySearchStartAudio();
                    SetSearchPoint();

                    isSearching = true;
                    return;
                }

                
                ResetDistraction();
                ResetStayIdle();
                
                
                InboraStudio.SetState(InboraStudio.previousState);
            }


            isIdle = true;
            turnedToLocation = false;
        }

        void ResetDistraction()
        {
            _timeToCheck = 0;
            _timeBeforeMovingToLocation = 0;
            _timeToReact = 0;
            searchIndex = 0;
            
            isIdle = false;
            turnedToLocation = false;
            playedLocationAudio = false;
            isSearching = false;
        }

        void ResetStayIdle()
        {
            InboraStudio.stayIdle = false;
        }

        void SetIdleState()
        {
            InboraStudio.isIdle = isIdle;
        }

        bool PreparePoint()
        {
            if (InboraStudio.endDestination == lastPoint) {
                return true;
            }

            lastPoint = InboraStudio.endDestination;

            if (randomizePoint) {
                distractionPoint = InboraStudio.RandomSpherePoint(InboraStudio.endDestination, randomizeRadius, false);
                
                if (InboraStudio.CalculateCornersDistanceFrom(InboraStudio.endDestination, distractionPoint) > randomizeRadius) {
                    lastPoint = Vector3.zero;
                    return false;
                }

                return true;
            }

            distractionPoint = InboraStudio.endDestination;
            return true;
        }

        #endregion

        #region SEARCHING

        // set the next search point
        void SetSearchPoint(int currentIteration = 0)
        {
            const int maxIterations = 25;

            float radiusDistance = (InboraStudio.navmeshAgent.height * 2) + 2;
            searchPointLocation = InboraStudio.RandomSpherePoint(distractionPoint, radiusDistance);
            float calculatedDistance = InboraStudio.CalculateCornersDistanceFrom(transform.position, searchPointLocation);

            // re-create search point if conditions met
            if ((calculatedDistance > radiusDistance || calculatedDistance <= (InboraStudio.navmeshAgent.radius * 2) + 0.4f || searchPointLocation == Vector3.zero) && currentIteration < maxIterations)
            {
                SetSearchPoint(currentIteration + 1);
                return;
            }

            searchIndex++;
            _timeToCheck = 0;
        }


        // play search point idle anim and return a bool whether the time has finished or not
        bool IsSearchPointIdleFinished()
        {
            InboraStudio.animManager.Play(searchPointAnim, searchAnimsT);
            isIdle = true;
            _timeToCheck += Time.deltaTime;
            
            
            if (_timeToCheck >= pointWaitTime) {
                return true;
            }


            return false;
        }


        // play start search audio
        void PlaySearchStartAudio()
        {
            if (!playAudioOnSearchStart) {
                return;
            }


            if (InboraStudio.IsAudioScriptableEmpty()) {
                return;
            }


            InboraStudio.PlayAudio(InboraStudio.audioScriptable.GetAudio(AudioScriptable.AudioType.SearchStart));
        }


        // play search end audio
        void PlaySearchEndAudio()
        {
            if (!playAudioOnSearchEnd) {
                return;
            }


            if (InboraStudio.IsAudioScriptableEmpty()) {
                return;
            }


            InboraStudio.PlayAudio(InboraStudio.audioScriptable.GetAudio(AudioScriptable.AudioType.SearchEnd));
        }


        // exit the search and distracted state
        void EndSearchExit()
        {
            InboraStudio.animManager.Play(endSearchAnim, searchAnimsT);
            PlaySearchEndAudio();

            _timeToCheck += Time.deltaTime;

            if (_timeToCheck >= endSearchAnimTime) {
                ResetDistraction();
                ResetStayIdle();

                InboraStudio.SetState(InboraStudio.previousState);
            }
        }

        #endregion

        #region OFF MESH LINKS
        
        void IgnoreOffMeshBehaviour()
        {
            if (checkLocation) {
                InboraStudio.navmeshAgent.Warp(transform.position);
                ReachedDistractionLocation();
                return;
            }
            
            turnedToLocation = false;
            isIdle = true;
            _timeBeforeMovingToLocation = 0f;

            InboraStudio.SetState(InboraStudio.previousState);
        }
        
        #endregion
    }
}