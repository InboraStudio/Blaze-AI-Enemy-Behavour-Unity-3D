using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.AI;

namespace InboraStudioAISpace
{
    [AddComponentMenu("InboraStudio AI/Attack State Behaviour")]
    public class AttackStateBehaviour : InboraStudioBehaviour
    {
        #region PROPERTIES

        [Min(0), Tooltip("Won't be considered if root motion is used.")]
        public float moveSpeed = 5f;
        [Min(0)] public float turnSpeed = 5f;
        public string idleAnim;
        public string moveAnim;
        [Min(0)] public float idleMoveT = 0.25f;

        [Tooltip("Set the amount of time you want the AI to keep being updated with the target's location when it's (the target) out of view. This helps the AI be smarter and more accurate during chases and corners. So for example: if it's set to 1 and the player suddenly goes out view then the AI will keep being updated with the target's changing location for 1 second, enabling the AI to chase the target with accurate location.")]
        [Min(0)] public float targetLocationUpdateTime = 1;


        [Tooltip("If enabled, the AI will enter an equip weapon sub-state at the beginning of this attack state.")]
        public bool equipWeapon;
        [Tooltip("The animation to play of equipping weapon.")]
        public string equipAnim;
        [Min(0), Tooltip("The duration of equipping weapon.")]
        public float equipDuration = 0.8f;
        [Min(0), Tooltip("The animation transition time of equip and unequip animations.")]
        public float equipAnimT = 0.25f;
        public UnityEvent onEquipEvent;

        [Tooltip("The animation to play of unequipping the weapon.")]
        public string unEquipAnim;
        [Tooltip("The duration of the unequipping of weapon.")]
        public float unEquipDuration = 0.8f;
        public UnityEvent onUnEquipEvent;


        public UnityEvent onStateEnter;
        public UnityEvent onStateExit;


        [Tooltip("If enabled, will adapt the distancing calculations to suit that of a ranged AI. So for example: if the target is unreachable but within attack distance then the AI will attack.")]
        public bool ranged;
        [Min(0), Tooltip("The idle distance between the AI and the target waiting to attack. If distance between the AI and the target is more than this value, the AI will chase the player to close the distance.")]
        public float distanceFromEnemy = 5f;
        [Min(0), Tooltip("The distance between AI and target when actually attacking. For example: if this is a ranged AI you want this value to be far since ranged enemies attack from a distance but if melee then this should be close.")]
        public float attackDistance = 1f;
        [Tooltip("Will check if any of these layers exist before attacking and if so will refrain from attacking until none of these layers exist. You can use this to avoid friendly fire with other AI agents.")]
        public LayerMask layersCheckOnAttacking;
        [Tooltip("Add your attacks. One will be chosen at random on each attack. If only one is set then that one will be launched every time.")]
        public Attacks[] attacks;
        public UnityEvent attackEvent;
        [Tooltip("If enabled, and the AI attacks but target evades (distance to target becomes bigger than Attack Distance) then the AI will stop the attack, chase it's target and launch the attack again. This makes the AI more challenging and responsive. If disabled, and the AI attacks while target evades, the AI will finish it's attack animation first before being able to chase again.")]
        public bool chaseOnEvade = false;
        [Tooltip("If enabled, the AI will attack every certain interval and not wait to be called by the manager.")]
        public bool attackInIntervals;
        [Min(0), Tooltip("The amount of time to pass before attacking. A value will be randomized between the two inputs. For a constant value set the two inputs to the same value.")]
        public Vector2 attackInIntervalsTime = new Vector2(1, 4);


        [Tooltip("The AIs have the ability to call other AIs for help when they see the target. Enabling this will call other agents to the location. If disabled, no AIs will be called.")]
        public bool callOthers;
        [Tooltip("The radius of the call.")]
        [Min(0)] public float callRadius;
        [Tooltip("Show the call radius as a white sphere in the scene view.")]
        public bool showCallRadius;
        [Tooltip("Select the layers of the InboraStudio AI agents you want to call.")]
        public LayerMask agentLayersToCall;
        [Tooltip("If enabled, the call will pass through colliders. If disabled, the call won't pass through the layers set in [Layers To Detect] in Vision.")]
        public bool callPassesColliders = true;
        [Min(0), Tooltip("The call runs once every certain time. Here you can specifiy the amount of time (seconds) to pass everytime before calling other agents.")]
        public float callOthersTime;
        [Tooltip("If enabled, this AI will be called by other agents if they are in attack state. If disabled, this AI won't be called.")]
        public bool receiveCallFromOthers;


        [Tooltip("If enabled, the AI will back away if the target moves in too close.")]
        public bool moveBackwards;
        [Min(0), Tooltip("If the distance between the target and the AI is less than this the AI will back away.")]
        public float moveBackwardsDistance = 5f;
        [Min(0), Tooltip("Won't be considered if root motion is used.")]
        public float moveBackwardsSpeed = 3f;
        public string moveBackwardsAnim;
        public float moveBackwardsAnimT = 0.25f;
        

        [Tooltip("If enabled, will turn to face the target if need be when waiting to attack. Will use the turn speed found in InboraStudio AI > Waypoints > Turn Speed.")]
        public bool turnToTarget = true;
        [Tooltip("If dot product between the AI and it's target is less or equal to this value then turning will occur.")]
        [Range(0, 1)] public float turnSensitivity = 0.7f;
        [Tooltip("Use the alert turning animations found in InboraStudio AI > Waypoints class.")]
        public bool useTurnAnims;


        [Tooltip("Will enable the AI to strafe in either direction when waiting to attack target.")]
        public bool strafe;
        public StrafeDirection strafeDirection = StrafeDirection.LeftAndRight;
        [Min(0), Tooltip("Won't be considered if root motion is used.")]
        public float strafeSpeed = 3f;
        [Min(-1), Tooltip("The amount of time to strafe for. A value will be randomized between the two inputs. For a constant value, set both inputs to the same value. For infinity and never stop strafing set both values to -1.")]
        public Vector2 strafeTime = new Vector2(3, 5);
        [Min(0), Tooltip("The amount of time to wait before strafing again.")]
        public Vector2 strafeWaitTime = new Vector2(0.3f, 1);
        [Tooltip("Left strafe animation name.")]
        public string leftStrafeAnim;
        [Tooltip("Right strafe animation name.")]
        public string rightStrafeAnim;
        [Tooltip("Transition time from current animation to the strafing animation.")]
        public float strafeAnimT = 0.25f;
        [Tooltip("Set all the layers you want to avoid when strafing that includes other InboraStudio AI agents.")]
        public LayerMask strafeLayersToAvoid;


        [Min(0), Tooltip("The speed to rotate to the enemy when reaching attack position.")]
        public float onAttackRotateSpeed = 10f;
        [Min(0), Tooltip("Keep rotating/facing the enemy while attacking. So if the AI throws a punch it'll still keep rotating to it's target while the punch animation is playing. Not good for melee but good for ranged enemies when you always want them to be looking at the target.")]
        public bool onAttackRotate;


        [Tooltip("When the AI moves to player location in attack state like in Hit or in a chase and finds no enemy the AI will search the radius.")]
        public bool searchLocationRadius;
        [Tooltip("The amount of time to pass in seconds before starting the search.")]
        public float timeToStartSearch = 2;
        [Range(1, 10), Tooltip("The number of points to randomly search.")]
        public int searchPoints = 3;
        [Tooltip("The animation name to play when reaching the search point.")]
        public string searchPointAnim;
        [Tooltip("The amount of time to wait in each search point.")]
        public float pointWaitTime = 3;
        [Tooltip("The animation to play when searching has finished.")]
        public string endSearchAnim;
        [Min(0), Tooltip("The amount of time (seconds) the animation should play for.")]
        public float endSearchAnimTime = 3;
        public float searchAnimsT = 0.25f;

        public bool playAudioOnSearchStart;
        public bool playAudioOnSearchEnd;


        [Tooltip("When the AI is in attack state and there's no hostile at the end location, this animation will play and after the animation time passes the AI will return to alert patrolling. Only works if search empty location is disabled.")]
        public string returnPatrolAnim;
        public float returnPatrolAnimT = 0.25f;
        [Min(0), Tooltip("The duration of the animation after target disappearance to return to alert patrolling.")]
        public float returnPatrolTime = 3f;
        public bool playAudioOnReturnPatrol;


        [Tooltip("Play a random audio from the audio scriptable when AI is waiting for it's turn to attack target. Set the audios from the audio scriptable in InboraStudio AI > General tab.")]
        public bool playAttackIdleAudio;
        [Min(0), Tooltip("The amount of time to pass (seconds) to play audio. A random number will be generated between the two set values. For a constant time, set the two properties to the same value.")]
        public Vector2 attackIdleAudioTime = new Vector2(3, 10);


        [Tooltip("Play a random audio when AI is chasing target. Set it in the audio scriptable.")]
        public bool playAudioOnChase;
        [Tooltip("If set to true, on chase the AI will always play an audio on chase. If false, there will be a 50/50 chance whether a chase audio will be played or not.")]
        public bool alwaysPlayOnChase;


        [Tooltip("Play a random audio when AI is moving to attack. Set audio from audio scriptable in the Move To Attack array.")]
        public bool playAudioOnMoveToAttack;
        [Tooltip("If set to true, on moving to attack the AI will always play an audio on chase. If false, there will be a 50/50 chance whether a chase audio will be played or not.")]
        public bool alwaysPlayOnMoveToAttack;
        
        #endregion

        #region BEHAVIOUR VARS

        public InboraStudioAI InboraStudio {private set; get; }
        bool isFirstRun = true;

        AlertStateBehaviour alertStateBehaviour;
        public int chosenAttackIndex { get; private set; }

        int checkPathElapsed = 0;
        int checkPathFrames = 3;
        int strafeDir = 0;
        int strafeCheckPathElapsed = 0;
        int agentPriority;
        int searchIndex = 0;
        
        bool idle;
        bool isMovingBackwards;
        bool startAttackTimer;
        bool startAttackInIntervals;
        bool isStrafing;
        bool isStrafeWait;
        bool turningToTarget;
        bool returnedToAlert;
        bool isSearching;
        bool returnPatrolAudioPlayed;
        bool isReachable;
        bool isReachableCantHit;
        bool attackIdleAudioPlayed;
        bool shouldPlayChaseAudio;
        bool chaseAudioPlayed;
        bool chaseAudioIsReady;
        bool moveToAttackAudioPlayed;
        bool isAddedToManagerInAttacks;
        bool offMeshBypassed;
        bool hasEquipped;
        public bool isEquipping;
        public bool isUnEquipping;
        bool isUnEquipDone;
        
        float attackDuration;
        float attackDurationTimer;
        float _intervalAttackTime;
        float intervalAttackTime;
        float _callOthers;
        float _timeToReturnAlert;
        float _strafeTime;
        float _strafeWaitTime;
        float _strafingTimer;
        float _strafeWaitTimer;
        float searchTimeElapsed;
        float attackIdleAudTimer = 0f;
        float chosenAttackIdleAudTime = -1;
        float collSize = 0f;
        float offMeshTimer = 0;
        float equipTimer;
        float moveBackwardsTimer = 0;
        float getLocationChaseTimer = 0;
        bool finishedGetLocationChase;

        Vector3 targetPosOnAttack;
        Vector3 lastEnemyPos;
        Vector3 searchPointLocation;
        Vector3 closestPointToTarget;
        Vector3 enemyPoint = Vector3.zero;

        AudioClip lastPlayedChaseAudio;
        Transform lastEnemy;

        public enum StrafeDirection 
        {
            Left,
            Right,
            LeftAndRight
        }

        [System.Serializable]
        public struct Attacks 
        {
            public string attackAnim;
            [Min(0.1f)] public float attackDuration;
            public float animT;
            public bool useAudio;
            public int audioIndex;
        }
        
        #endregion
        
        #region GARBAGE REDUCTION
        
        Collider[] callOthersHitArr = new Collider[20];
        RaycastHit[] strafingHitArr = new RaycastHit[10];
        
        #endregion
        
        #region MAIN METHODS
        
        public virtual void OnStart()
        {
            InboraStudio = GetComponent<InboraStudioAI>();
            isFirstRun = false;
            
            alertStateBehaviour = GetComponent<AlertStateBehaviour>();
            agentPriority = InboraStudio.navmeshAgent.avoidancePriority;

            ValidateDistance();
            ReadyChaseAudio();
        }

        public override void Open()
        {
            if (isFirstRun) {
                OnStart();
            }

            onStateEnter.Invoke();
            
            if (InboraStudio == null) {
                Debug.LogWarning($"No InboraStudio AI component found in the gameobject: {gameObject.name}. AI behaviour will have issues.");
                return;
            }
            
            checkPathElapsed = checkPathFrames;

            IdlePosition();
            ReadyChaseAudio();
            CheckIfWeaponShouldEquip();
        }

        public override void Close()
        {
            ResetFlags();
            ResetSearching();
            ResetAttackIdleAudio();
            ResetChaseAudio();
            ResetMoveToAttackAudio();
            ResetEquipConditionsOnDisable();
            ResetChaseGetLocation();

            onStateExit.Invoke();

            if (InboraStudio == null) {
                return;
            }

            if (InboraStudio.navmeshAgent) {
                InboraStudio.navmeshAgent.avoidancePriority = agentPriority;
            }
            
            if (InboraStudio.state != InboraStudioAI.State.attack) {
                InboraStudio.ResetEnemyManager();
            }
        }

        public override void Main()
        {
            #if UNITY_EDITOR
            if (!InboraStudio.navmeshAgent.enabled) {
                InboraStudio.PrintWarning(InboraStudio.warnAnomaly, "The Attack State Behaviour is trying to run but the NavMesh Agent component is disabled.");
                return;
            }
            #endif
            

            if (InboraStudio.enemyToAttack) {
                lastEnemy = InboraStudio.enemyToAttack.transform;
            }

            if (isEquipping) {
                EquipWeapon();
                return;
            }

            if (isUnEquipping) {
                UnEquipWeapon();
                return;
            }

            // exit state if AI turned friendly
            if (InboraStudio.friendly) {
                NoTarget();
                return;
            }
            

            // track the attack timer
            AttackTimer();
            
            // if interval attacks are enabled
            IntervalAttackTimer();

            // force the strafing flags to false if strafe is not enabled
            ValidateFlags();

            
            // if target exists
            if (InboraStudio.enemyToAttack) 
            {
                ResetChaseGetLocation();

                CallOthers();
                ResetSearching();
                
                // if currently turning to face target
                if (turningToTarget && !InboraStudio.isAttacking) {
                    IdlePosition();
                    return;
                }                
                
                Engage(InboraStudio.enemyToAttack.transform);

                if (strafe) {
                    if (isStrafing) {
                        Strafe();
                        return;
                    }
                    
                    StrafeWaitTimer();
                    return;
                }

                return;
            }

            
            // REACHING THIS POINT MEANS ENEMY DOESN'T EXIST -> GOT OUT OF VISION

    
            StopStrafe();

            // if mid-attack -> let it continue
            if (startAttackTimer) return;


            // if mid turn -> continue turning and quit
            if (turningToTarget) {
                IdlePosition();
                return;
            }
            

            // if in companion mode -> return to normal
            if (InboraStudio.companionMode) {
                InboraStudio.SetState(InboraStudioAI.State.normal);
                return;
            }


            // check if another agent called
            if (InboraStudio.checkEnemyPosition != Vector3.zero) {
                GoToLocation(InboraStudio.checkEnemyPosition);
                return;
            }


            // search within radius of empty location
            if (isSearching) {
                if (InboraStudio.MoveTo(searchPointLocation, alertStateBehaviour.moveSpeed, alertStateBehaviour.turnSpeed, alertStateBehaviour.moveAnim)) {
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

                if (!InboraStudio.isPathReachable) {
                    EndSearchExit();
                }

                return;
            }
            
            
            // if target changed tag to something non-hostile
            if (InboraStudio.isTargetTagChanged) {
                NoTarget();
                return;
            }


            if (lastEnemy == null) {
                NoTarget();
                return;
            }


            if (!finishedGetLocationChase && (getLocationChaseTimer <= targetLocationUpdateTime)) 
            {
                getLocationChaseTimer += Time.deltaTime;
                lastEnemyPos = InboraStudio.ValidateYPoint(lastEnemy.position);
                
                if (getLocationChaseTimer >= targetLocationUpdateTime) {
                    lastEnemyPos = InboraStudio.RandomizePosition(lastEnemyPos, InboraStudio.navmeshAgent.radius + 1.5f);
                    finishedGetLocationChase = true;
                    getLocationChaseTimer = 0;
                }
            }
            

            // if last enemy position isn't reachable -> exit
            if (!InboraStudio.IsPathReachable(lastEnemyPos)) {
                NoTarget();
                return;
            }
            
            GoToLocation(lastEnemyPos);
        }

        void OnDrawGizmosSelected() 
        {
            // show the call radius
            if (callOthers && showCallRadius) {
                Gizmos.color = Color.white;
                Gizmos.DrawWireSphere(transform.position, callRadius);
            }

            // fire a red ray to target
            if (InboraStudio) {
                if (InboraStudio.enemyToAttack) {
                    Vector3 dir = InboraStudio.enemyColPoint - (transform.position + InboraStudio.vision.visionPosition);
                    Debug.DrawRay(transform.position + InboraStudio.vision.visionPosition, dir, Color.red, 0.1f);
                }
            }
        }

        void OnValidate()
        {   
            if (distanceFromEnemy < moveBackwardsDistance) {
                moveBackwardsDistance = distanceFromEnemy - 0.5f;
            }

            if (moveBackwardsDistance >= distanceFromEnemy) {
                moveBackwardsDistance = distanceFromEnemy - 0.5f;
            }

            if (InboraStudio == null) {
                InboraStudio = GetComponent<InboraStudioAI>();
            }

            if (InboraStudio == null) return;

            ValidateDistance();
        }

        #endregion

        #region ATTACK
        
        // engage the target
        public virtual void Engage(Transform target)
        {
            _timeToReturnAlert = 0;
            returnedToAlert = false;

            AddManagerAndSetInterval(target);

            // check if enemy reachable every 5 frames
            if (checkPathElapsed >= checkPathFrames) 
            {
                checkPathElapsed = 0;
                CheckHeightAndReachable();
            }

            checkPathElapsed++;
            
            // InboraStudio keeps track of distance to the current targeted enemy using sqr magnitude in this property
            float distance = InboraStudio.distanceToEnemySqrMag;
            float[] setDistances = SetDistances();
            float minDistance = setDistances[0];
            float backupDist = setDistances[1];

            if (InboraStudio.IfShouldIgnoreOffMesh()) {
                IgnoreOffMeshBehaviour();
                return;
            }

            if (!isReachable) {
                UnreachableBehaviour(target, distance, minDistance, backupDist);
                return;
            }
            
            ReachableBehaviour(target, distance, minDistance, backupDist);
        }

        // idle position -> waiting to attack
        public virtual void IdlePosition()
        {
            idle = true;
            isMovingBackwards = false;
            moveBackwardsTimer = 0;

            // play audios
            PlayAttackIdleAudio();
            ReadyChaseAudio();

            // check if turn to target is enabled
            if (turnToTarget) 
            {
                Vector3 enemyPos = new Vector3(InboraStudio.enemyColPoint.x, transform.position.y, InboraStudio.enemyColPoint.z);
                Vector3 toOther = (enemyPos - transform.position).normalized;
                float dotProd = Vector3.Dot(toOther, transform.forward);

                // if AI is currently turning to target
                if (turningToTarget) {
                    // TurnTo() turns the AI and returns true when dot == 0.97f and false otherwise
                    if (!InboraStudio.TurnTo(enemyPos, InboraStudio.waypoints.leftTurnAnimAlert, InboraStudio.waypoints.rightTurnAnimAlert, InboraStudio.waypoints.turningAnimT, 7, useTurnAnims)) {
                        return;
                    }

                    turningToTarget = false;
                }
                else {
                    // if turning should be done
                    if (dotProd <= turnSensitivity) {
                        turningToTarget = true;
                        return;
                    }
                }
            }

            // trigger strafing or idle anim
            if (strafe) 
            {
                TriggerStrafe();
                return;
            }
            
            InboraStudio.animManager.Play(idleAnim, idleMoveT);
        }

        // move the AI to target position
        public virtual void MoveToTarget(Vector3 pos, bool runIdle=false)
        {
            if (InboraStudio.isAttacking) {
                if (chaseOnEvade) {
                    if ((attackDurationTimer <= (attackDuration * 0.4f))) {
                        startAttackTimer = false;
                        attackDurationTimer = 0;
                    }
                }
                else {
                    if (startAttackTimer) return;
                }
            }
            

            if (InboraStudio.MoveTo(pos, moveSpeed, turnSpeed, moveAnim, idleMoveT)) {
                if (runIdle) {
                    IdlePosition();
                    return;
                }
            }


            idle = false;
            isStrafing = false;
            isMovingBackwards = false;

            if (!InboraStudio.isAttacking) {
                PlayChaseAudio();
                return;
            }
            
            PlayMoveToAttackAudio();
        }

        // launch attack
        public virtual void Attack()
        {
            if (attacks.Length <= 0) {
                InboraStudio.PrintWarning(InboraStudio.warnAnomaly, "Attack operation skipped because Attack list (property) is empty in Attack State Behaviour. Please add an item to the list. You can even leave the animation empty but atleast set the duration otherwise the duration is unknown and can't attack.");
                InboraStudio.isAttacking = false;
                return;
            }
            
            if (startAttackTimer) return;

            
            Vector3 startDir = transform.position + InboraStudio.vision.visionPosition;
            Vector3 targetDir = InboraStudio.enemyColPoint - (transform.position + InboraStudio.vision.visionPosition);

            int layers = layersCheckOnAttacking | InboraStudio.vision.hostileAndAlertLayers;
            float rayDistance = Vector3.Distance(InboraStudio.enemyColPoint, transform.position + InboraStudio.vision.visionPosition) + 5;

            // check if there's a layer between AI and target before attack
            if (!InboraStudio.CheckTargetVisibleWithRay(InboraStudio.enemyToAttack.transform, startDir, targetDir, rayDistance, layers)) {
                IdlePosition();
                return;
            }


            // choose a random attack
            int index = Random.Range(0, attacks.Length);
            chosenAttackIndex = index;

            // play attack animation and invoke attack event
            InboraStudio.animManager.Play(attacks[index].attackAnim, attacks[index].animT);
            attackEvent.Invoke();
            
            // play attack audio if exists
            if (attacks[index].useAudio) {
                if (!InboraStudio.IsAudioScriptableEmpty()) {
                    InboraStudio.PlayAudio(InboraStudio.audioScriptable.GetAudio(AudioScriptable.AudioType.Attacks, attacks[index].audioIndex));
                }
            }

            // save the target position on the attack frame to rotate to
            targetPosOnAttack = InboraStudio.enemyToAttack.transform.position;

            // these vars are used in AttackTimer() method
            attackDuration = attacks[index].attackDuration;
            startAttackTimer = true;

            idle = false;
        }

        // set the min distance for the AI according to conditions
        float[] SetDistances()
        {
            float[] arr = new float[2];
            float minDistance = 0;
            float backupDistTemp = 0;

            // set the min distance according to attack and/or enemy manager call            
            if (InboraStudio.enemyManager.callEnemies) 
            {
                if (InboraStudio.isAttacking) {
                    minDistance = attackDistance;
                    backupDistTemp = 0;

                    arr[0] = minDistance;
                    arr[1] = backupDistTemp;
                    
                    return arr;
                }
            }
            else {
                InboraStudio.StopAttack();
            }

            if (isStrafing) minDistance = distanceFromEnemy + 1;
            else minDistance = distanceFromEnemy;

            if (moveBackwards) backupDistTemp = moveBackwardsDistance;
        
            arr[0] = minDistance;
            arr[1] = backupDistTemp;
            return arr;
        }

        // the attack duration timer
        void AttackTimer()
        {
            if (!InboraStudio.isAttacking) {
                startAttackTimer = false;
                attackDurationTimer = 0;
            }


            if (startAttackTimer) {
                // set priority to the highest to avoid being pushed by other agents while attacking
                InboraStudio.navmeshAgent.avoidancePriority = 0;

                // rotate to target
                if (onAttackRotate) {
                    InboraStudio.RotateTo(InboraStudio.enemyColPoint, onAttackRotateSpeed);
                }
                else {
                    if (attackDurationTimer <= 0.1) {
                        InboraStudio.RotateTo(targetPosOnAttack, onAttackRotateSpeed);
                    }
                }
                
                // attack timer
                attackDurationTimer += Time.deltaTime;

                if (attackDurationTimer >= attackDuration) {
                    StopAttack();
                }
            }
            else {
                // return to the original priority level after attack
                InboraStudio.navmeshAgent.avoidancePriority = agentPriority;
            }
        }

        // time before attacking (if attack in Intervals mode is enabled)
        void IntervalAttackTimer()
        {
            if (InboraStudio.enemyToAttack == null) return;
            if (!startAttackInIntervals) return;

            _intervalAttackTime += Time.deltaTime;
            if (_intervalAttackTime < intervalAttackTime) return;
            
            InboraStudio.Attack();
            PlayMoveToAttackAudio();
            
            startAttackInIntervals = false;
            _intervalAttackTime = 0f;
        }

        public virtual void StopAttack()
        {
            InboraStudio.isAttacking = false;
            startAttackTimer = false;
            attackDurationTimer = 0f;
            ResetMoveToAttackAudio();
        }

        void AddManagerAndSetInterval(Transform target)
        {
            if (!attackInIntervals) {
                InboraStudio.AddEnemyManager(target);
                return;
            }
            
            InboraStudio.AddEnemyManager(target, false);
    
            if (startAttackInIntervals) return;
            if (InboraStudio.isAttacking) return;
            
            intervalAttackTime = Random.Range(attackInIntervalsTime.x, attackInIntervalsTime.y);
            startAttackInIntervals = true;
        }

        void CheckHeightAndReachable()
        {
            RaycastHit heightHit;
            Collider enemyColl = GetCollider(InboraStudio.enemyToAttack);
            isReachableCantHit = false;

            if (Physics.Raycast(enemyColl.bounds.min, -Vector3.up, out heightHit, Mathf.Infinity, InboraStudio.groundLayers)) 
            {
                isReachable = InboraStudio.IsPathReachable(heightHit.point);
                
                if (heightHit.distance >= InboraStudio.navmeshAgent.height) {
                    if (!ranged) {
                        isReachableCantHit = true;
                    }
                    else {
                        if (InboraStudio.distanceToEnemy <= attackDistance) isReachableCantHit = false;
                        else isReachableCantHit = true;
                    }

                    return;
                }

                return;
            }

            // if didn't catch anything from the previous operation -> (from the bottom) -> it could mean the target's min point is clipping the ground
            if (Physics.Raycast(InboraStudio.enemyColPoint, -Vector3.up, out heightHit, Mathf.Infinity, InboraStudio.groundLayers)) 
            {   
                isReachable = InboraStudio.IsPathReachable(InboraStudio.enemyColPoint);
                return;
            }
            
            isReachable = InboraStudio.IsPointOnNavMesh(enemyColl.bounds.min, 0.3f);

            if (ranged) {
                enemyPoint = InboraStudio.ValidateYPoint(InboraStudio.enemyColPoint);
            }
        }
        
        public virtual void ReachableBehaviour(Transform target, float distance, float minDistance, float backupDist)
        {
            if (!CheckMoveBackwardsDone() && !InboraStudio.isAttacking) {
                return;
            }

            if (isReachableCantHit) {
                InboraStudio.StopAttack();
            }

            // check not too close to target
            if (distance + 0.3f > (backupDist * backupDist)) 
            {
                // if distance is bigger than min distance -> move
                if (distance > (minDistance * minDistance)) 
                {
                    // if already in idle position don't move if distance difference is 2f or less to avoid bad frozen movement 
                    if (!InboraStudio.isAttacking && idle) 
                    {
                        float tempDist = distance - (minDistance * minDistance);
                        
                        if (tempDist <= minDistance + 2f) {
                            IdlePosition();
                            return;
                        }
                    }

                    MoveToTarget(InboraStudio.enemyColPoint);
                    return;
                }
                
                // if reached min distance and is attacking true -> launch attack
                if (InboraStudio.isAttacking) 
                {
                    Attack();
                    return;
                }

                IdlePosition();
                return;
            }
            
            // reaching this point means target is too close and should move backwards
            MoveBackwards(InboraStudio.enemyColPoint);
        }

        public virtual void UnreachableBehaviour(Transform target, float distance, float minDistance, float backupDist)
        {
            if (!CheckMoveBackwardsDone() && !InboraStudio.isAttacking) {
                return;
            }

            if (startAttackTimer) {
                return;
            }
            
            if (ranged) 
            {
                if (distance > (backupDist * backupDist)) 
                {
                    if (distance > (minDistance * minDistance)) 
                    {
                        if (InboraStudio.ClosestNavMeshPoint(enemyPoint, collSize, out closestPointToTarget)) 
                        {
                            MoveToTarget(closestPointToTarget, true);
                            return;
                        }

                        IdlePosition();
                        return;
                    }

                    if (InboraStudio.isAttacking) 
                    {
                        Attack();
                        return;
                    }

                    IdlePosition();
                    return;
                }

                MoveBackwards(InboraStudio.enemyColPoint);
                return;
            }

            InboraStudio.StopAttack();                
            
            if (distance + 0.3f > (backupDist * backupDist)) 
            {
                IdlePosition();
                return;
            }

            MoveBackwards(InboraStudio.enemyColPoint);
            return;
        }

        #endregion
        
        #region CALL OTHERS
        
        // call nearby agents to target location
        public virtual void CallOthers()
        {
            // if call others isn't enabled or no target
            if (!callOthers || InboraStudio.enemyToAttack == null) {
                return;
            }

            // time to pass before firing
            _callOthers += Time.deltaTime;
            if (_callOthers < callOthersTime) {
                return;
            }
            _callOthers = 0;

            System.Array.Clear(callOthersHitArr, 0, 20);
            int callOthersNum = Physics.OverlapSphereNonAlloc(transform.position, callRadius, callOthersHitArr, agentLayersToCall);
            
            for (int i=0; i<callOthersNum; i+=1) 
            {
                InboraStudioAI script = callOthersHitArr[i].GetComponent<InboraStudioAI>();
                AttackStateBehaviour attackBehaviour = callOthersHitArr[i].GetComponent<AttackStateBehaviour>();
                CoverShooterBehaviour coverShooterBehaviour = callOthersHitArr[i].GetComponent<CoverShooterBehaviour>();
                

                if (callOthersHitArr[i].transform.IsChildOf(lastEnemy.transform)) {
                    continue;
                }

                // if current item is the AI itself -> skip
                if (callOthersHitArr[i].transform.IsChildOf(transform)) {
                    continue;
                }

                // if doesn't have InboraStudio AI
                if (!script) continue;
                if (script.friendly) continue;


                // if doesn't have this AttackStateBehaviour script
                if (!attackBehaviour && !coverShooterBehaviour) continue;


                // check if doesn't receive calls
                if (attackBehaviour) {
                    if (!attackBehaviour.receiveCallFromOthers) {
                        continue;
                    }
                }
                else {
                    if (!coverShooterBehaviour.receiveCallFromOthers) {
                        continue;
                    }
                }


                // if agent already has a target then don't call
                if (script.enemyToAttack) {
                    continue;
                }


                // if other agent has seen the target after this agent then don't call
                if (script.captureEnemyTimeStamp > InboraStudio.captureEnemyTimeStamp) {
                    continue;
                }


                // check there are no obstacles hindering the call
                if (!callPassesColliders) 
                {
                    RaycastHit rayHit;
                    
                    Transform target = callOthersHitArr[i].transform;
                    Collider currentColl = callOthersHitArr[i];

                    Vector3 currentCenter = new Vector3(transform.position.x, transform.position.y + InboraStudio.vision.visionPosition.y, transform.position.z);
                    Vector3 enemyCenter = currentColl.ClosestPoint(currentCenter);
                    Vector3 dir = (enemyCenter - currentCenter);

                    float rayDistance = Vector3.Distance(target.position, transform.position) + 3;

                    if (Physics.Raycast(currentCenter, dir, out rayHit, rayDistance, InboraStudio.vision.layersToDetect)) {
                        if (!rayHit.transform.IsChildOf(target) && !target.IsChildOf(rayHit.transform)) {
                            continue;
                        }
                    }
                }

                
                // set the check enemy position of the other agents to target position
                script.checkEnemyPosition = script.ValidateYPoint(InboraStudio.enemyColPoint);

                // // turn the agents to attack state
                script.TurnToAttackState();
            }
        }

        #endregion
    
        #region GETTING CALLED BY OTHERS
        
        public virtual void GoToLocation(Vector3 location)
        {
            // moves AI to location and returns true when reaches location
            if (InboraStudio.MoveTo(location, moveSpeed, turnSpeed, moveAnim, idleMoveT)) {
                NoTarget();
                return;
            }
            
            idle = false;
            ResetSearching();
            PlayChaseAudio();
        }
        
        #endregion
    
        #region STRAFING

        // this gets called first in IdlePosition() to trigger the strafe
        void TriggerStrafe()
        {   
            // quit if already waiting or strafing
            if (isStrafeWait || isStrafing) {
                return;
            } 

            
            // strafe direction
            if (strafeDirection == StrafeDirection.Left) {
                strafeDir = 0;
            }
            else if (strafeDirection == StrafeDirection.Right) {
                strafeDir = 1;
            }
            else {
                strafeDir = Random.Range(0, 2);
            }


            // set strafe time
            if (strafeTime.x == -1 && strafeTime.y == -1) {
                _strafeTime = Mathf.Infinity;
            }
            else {
                _strafeTime = Random.Range(strafeTime.x, strafeTime.y);
            }


            // set strafe wait time and start wait timer
            _strafeWaitTime = Random.Range(strafeWaitTime.x, strafeWaitTime.y);

            // strafe wait timer used in StrafeWaitTimer() 
            isStrafeWait = true;
        }
        
        // check if can strafe move and trigger the movement 
        void Strafe()
        {
            if (!idle) {
                StopStrafe();
                return;
            }

            StrafeMovement(strafeDir);
        }

        // the actual strafing movement
        public virtual void StrafeMovement(int direction)
        {   
            int layersToHit = InboraStudio.vision.hostileAndAlertLayers | strafeLayersToAvoid | InboraStudio.vision.layersToDetect;
            
            Vector3 strafePoint = Vector3.zero;
            Vector3 offsetPlayer;
            Vector3 transformDir;
            Vector3 offset;

            string strafeAnim;
            string moveDir;


            // if direction is left
            if (direction == 0) {
                offsetPlayer = InboraStudio.enemyColPoint - transform.position;
                offsetPlayer = Vector3.Cross(offsetPlayer, Vector3.up);

                strafePoint = InboraStudio.ValidateYPoint(offsetPlayer);
                strafePoint = transform.position + new Vector3(strafePoint.x, 0, strafePoint.z + 0.5f);
                strafePoint = strafePoint + (transform.right * (InboraStudio.distanceToEnemy - 1));

                transformDir = -transform.right;

                // to check from an offset if enemy will not be visible
                offset = transform.TransformPoint(new Vector3(-(InboraStudio.navmeshAgent.radius + 0.7f), 0f, 0f) + InboraStudio.vision.visionPosition);

                // set the anim and move dir
                strafeAnim = leftStrafeAnim;
                moveDir = "left";
            }
            else {
                offsetPlayer = transform.position - InboraStudio.enemyColPoint;
                offsetPlayer = Vector3.Cross(offsetPlayer, Vector3.up);

                strafePoint = InboraStudio.ValidateYPoint(offsetPlayer);
                strafePoint = transform.position + new Vector3(strafePoint.x, 0, strafePoint.z + 0.5f);
                strafePoint = strafePoint + (-transform.right * (InboraStudio.distanceToEnemy - 1));

                transformDir = transform.right;

                // to check from an offset if enemy will not be visible
                offset = transform.TransformPoint(new Vector3((InboraStudio.navmeshAgent.radius + 0.7f), 0f, 0f) + InboraStudio.vision.visionPosition);
                
                // set the anim and move dir
                strafeAnim = rightStrafeAnim;
                moveDir = "right";
            }
            
            
            // check if point reachable and has navmesh every 5 frames
            if (strafeCheckPathElapsed >= 5) {
                strafeCheckPathElapsed = 0;

                // if agent isn't on navmesh
                if (!InboraStudio.IsPointOnNavMesh(InboraStudio.ValidateYPoint(transform.position), InboraStudio.navmeshAgent.radius)) {
                    StopStrafe();
                    return;
                }
                
                Vector3 goToPoint = InboraStudio.ValidateYPoint((transform.position) + (transformDir * (InboraStudio.navmeshAgent.radius * 2 + (InboraStudio.navmeshAgent.height / 2))));
                if (!InboraStudio.IsPathReachable(goToPoint)) {
                    StopOrChangeStrafeDirection();
                    return;
                }
            }

            strafeCheckPathElapsed++;


            // check if there's an obstacle in strafe direction
            System.Array.Clear(strafingHitArr, 0, 10);
            int hits = Physics.SphereCastNonAlloc(transform.position + InboraStudio.vision.visionPosition, 0.15f, transformDir, strafingHitArr, (InboraStudio.navmeshAgent.radius * 2) + InboraStudio.navmeshAgent.height/2, layersToHit);
            int hitIndex = -1;

            // filter
            for (int i=0; i<hits; i++) {
                // if sphere cast hits the same AI collider -> ignore
                if (transform.IsChildOf(strafingHitArr[i].transform) || strafingHitArr[i].transform.IsChildOf(transform)) {
                    continue;
                }

                if (strafingHitArr[i].distance == 0 || strafingHitArr[i].point == Vector3.zero) {
                    continue;
                }

                hitIndex = i;
            }

            if (hitIndex > -1) {
                StopOrChangeStrafeDirection();
                return;
            }
            
            layersToHit = InboraStudio.vision.hostileAndAlertLayers | InboraStudio.vision.layersToDetect;
            
            // to check from an offset if enemy will not be visible
            if (!InboraStudio.CheckTargetVisibleWithRay(InboraStudio.enemyToAttack.transform, offset, InboraStudio.enemyColPoint - offset, Vector3.Distance(InboraStudio.enemyColPoint, transform.position) + 5, layersToHit)) {
                StopOrChangeStrafeDirection();
                return;
            }
        
            isStrafing = true;
            InboraStudio.RotateTo(InboraStudio.enemyToAttack.transform.position, 20);
            InboraStudio.MoveTo(strafePoint, strafeSpeed, 0, strafeAnim, strafeAnimT, moveDir);
        
            _strafingTimer += Time.deltaTime;
            if (_strafingTimer >= _strafeTime) {
                StopStrafe();
            }
        }

        void StopStrafe()
        {
            isStrafing = false;
            _strafingTimer = 0f;
        }

        // wait to strafe
        void StrafeWaitTimer()
        {
            if (!idle) {
                isStrafeWait = false;
                _strafeWaitTimer = 0f;
                return;
            }

            if (isStrafeWait) {
                InboraStudio.animManager.Play(idleAnim, idleMoveT);  
                _strafeWaitTimer += Time.deltaTime;
                
                if (_strafeWaitTimer >= _strafeWaitTime) {
                    _strafeWaitTimer = 0f;

                    // in Update() if isStrafing fires Strafe()
                    isStrafing = true;
                    isStrafeWait = false;
                }
            }
        }

        void StopOrChangeStrafeDirection()
        {
            if (strafeDirection == StrafeDirection.LeftAndRight) {
                if (strafeDir == 0) strafeDir = 1;
                else strafeDir = 0;

                InboraStudio.animManager.Play(idleAnim, idleMoveT);
            }
            else {
                StopStrafe();
            }
        }
        
        #endregion
    
        #region MOVE BACKWARDS
        
        // back away from target
        public virtual void MoveBackwards(Vector3 target)
        {
            Vector3 targetPosition = transform.position - (transform.forward * (InboraStudio.navmeshAgent.height - 0.25f));
            Vector3 backupPoint = InboraStudio.ValidateYPoint(targetPosition);
            RaycastHit hit;

            backupPoint = new Vector3 (backupPoint.x, transform.position.y, backupPoint.z + 0.5f);
            int layers = InboraStudio.vision.layersToDetect | agentLayersToCall | strafeLayersToAvoid;


            // check if obstacle is behind
            if (Physics.Raycast(transform.position + InboraStudio.vision.visionPosition, -transform.forward, out hit, InboraStudio.navmeshAgent.radius * 2 + 0.3f, layers)) {
                IdlePosition();
                return;
            }

            
            // if point isn't reachable
            if (!InboraStudio.IsPathReachable(backupPoint)) {
                IdlePosition();
                return;
            }

            
            // if strafing we need further precision to check if moving backwards is possible
            // to prevent disabling strafing to find the AI moves backwards a very neglibile distance which makes it look very bad
            if (isStrafing) 
            {
                if (Physics.Raycast(transform.position + InboraStudio.vision.visionPosition, -transform.forward, out hit, moveBackwardsDistance / 1.5f + InboraStudio.navmeshAgent.radius, layers)) {
                    IdlePosition();
                    return;
                }
                else {
                    Vector3 checkPoint = new Vector3(backupPoint.x, transform.position.y, backupPoint.z + (moveBackwardsDistance / 2f));

                    if (!InboraStudio.IsPathReachable(checkPoint)) {
                        IdlePosition();
                        return;
                    }
                }
            }
            

            // back away
            InboraStudio.RotateTo(target, turnSpeed);
            InboraStudio.MoveTo(backupPoint, moveBackwardsSpeed, 0f, moveBackwardsAnim, moveBackwardsAnimT, "backwards");

            
            // cancel strafing when backing away
            idle = false;
            isStrafing = false;
            isStrafeWait = false;
            isMovingBackwards = true;
        }

        // avoids jittering animation between moving backwards and idle stance if target is very slowly closing in the distance
        bool CheckMoveBackwardsDone()
        {
            if (!isMovingBackwards) return true;
            if (InboraStudio.isAttacking) return true;
            
            moveBackwardsTimer += Time.deltaTime;
            if (moveBackwardsTimer < 0.3f) {
                MoveBackwards(InboraStudio.enemyColPoint);
                return false;
            }

            return true;
        }
        
        #endregion
    
        #region MISC

        // force the strafing flags to false if strafe is not enabled
        void ValidateFlags()
        {
            if (!strafe) {
                isStrafing = false;
                isStrafeWait = false;
            }
        }

        void ResetFlags()
        {
            returnPatrolAudioPlayed = false;
            InboraStudio.isTargetTagChanged = false;
            offMeshBypassed = false;
            offMeshTimer = 0;
            moveBackwardsTimer = 0;
        }

        void ValidateDistance()
        {
            if (InboraStudio.vision == null) return;

            if (moveBackwards)
            {
                if (moveBackwardsDistance >= InboraStudio.vision.visionDuringAttackState.sightRange) 
                {
                    if (InboraStudio.vision.visionDuringAttackState.sightRange - 3 <= 0) 
                    {
                        moveBackwardsDistance = 0;
                        Debug.LogWarning("Move Backwards Distance can't be bigger or equal to Sight Range in Vision During Attack State. Has been reset to 0.");
                        return;
                    }
                    
                    moveBackwardsDistance = InboraStudio.vision.visionDuringAttackState.sightRange - 3;
                    Debug.LogWarning("Move Backwards Distance can't be bigger or equal to Sight Range in Vision During Attack State.");
                }
            }
        }

        Collider GetCollider(GameObject go)
        {
            return go.GetComponent<Collider>();
        }
        
        void IgnoreOffMeshBehaviour()
        {
            if (InboraStudio.enemyToAttack != null) 
            {
                if (!offMeshBypassed) {
                    InboraStudio.navmeshAgent.Warp(transform.position);
                    offMeshBypassed = true;
                }

                IdlePosition();
                offMeshTimer += Time.deltaTime;
                
                if (offMeshTimer >= 1) {
                    offMeshTimer = 0;
                    offMeshBypassed = false;
                }

                return;
            }
            
            NoTarget();
        }

        void ResetChaseGetLocation()
        {
            finishedGetLocationChase = false;
            getLocationChaseTimer = 0;
        }

        #endregion

        #region RETURN PATROL/ALERT && SEARCHING
        
        // reached location and no hostile found
        public virtual void NoTarget()
        {
            if (InboraStudio.companionMode) {
                ReturnToAlert();
            }

            // prevent chase audio from playing
            if (playAudioOnChase) {
                ResetChaseAudio();
            }

            // search empty location
            if (searchLocationRadius) 
            {
                InboraStudio.animManager.Play(searchPointAnim, returnPatrolAnimT);
                searchTimeElapsed += Time.deltaTime;

                if (searchTimeElapsed >= timeToStartSearch) {
                    PlaySearchStartAudio();
                    SetSearchPoint();
                    InboraStudio.checkEnemyPosition = Vector3.zero;
                    return;
                }

                return;
            }

            if (!returnedToAlert) {
                InboraStudio.SetState(InboraStudioAI.State.returningToAlert);
            }

            returnedToAlert = true;
            turningToTarget = false;

            PlayReturnPatrolAudio();
            ReturnToAlertIdle();
            
            _timeToReturnAlert += Time.deltaTime;
            if (_timeToReturnAlert < returnPatrolTime) return;

            ReturnToAlert();
        }
        
        // play return animation
        void ReturnToAlertIdle()
        {
            if (returnPatrolAnim.Length == 0) {
                InboraStudio.animManager.Play(idleAnim, returnPatrolAnimT);
            }
            else {
                InboraStudio.animManager.Play(returnPatrolAnim, returnPatrolAnimT);
            }

            idle = true;
        }
        
        // exit attack state and return to alert
        void ReturnToAlert()
        {
            if (equipWeapon) 
            {
                UnEquipWeapon();
                if (!isUnEquipDone) return;
            }

            _timeToReturnAlert = 0;
            InboraStudio.SetState(InboraStudioAI.State.alert);
        }

        // play start search audio
        void PlayReturnPatrolAudio()
        {
            // if audio already played -> return
            if (returnPatrolAudioPlayed) return;
            if (!playAudioOnReturnPatrol) return;
            if (InboraStudio.IsAudioScriptableEmpty()) return;
            
            InboraStudio.PlayAudio(InboraStudio.audioScriptable.GetAudio(AudioScriptable.AudioType.ReturnPatrol));
            returnPatrolAudioPlayed = true;
        }

        // set the next search point
        void SetSearchPoint()
        {
            float radiusDistance = (InboraStudio.navmeshAgent.height * 2) + 2;
            searchPointLocation = InboraStudio.RandomSpherePoint(InboraStudio.enemyColPoint, radiusDistance);
            float calculatedDistance = InboraStudio.CalculateCornersDistanceFrom(transform.position, searchPointLocation);
            
            // re-create search point if conditions met
            if (calculatedDistance > radiusDistance || calculatedDistance <= (InboraStudio.navmeshAgent.radius * 2) + 0.4f || searchPointLocation == Vector3.zero) {
                searchPointLocation = InboraStudio.ValidateYPoint(InboraStudio.enemyColPoint);
            }
            
            searchIndex++;
            searchTimeElapsed = 0;
            isSearching = true;
        }

        // returns whether the idle time has finished in the search point or not
        bool IsSearchPointIdleFinished()
        {
            InboraStudio.animManager.Play(searchPointAnim, searchAnimsT);

            searchTimeElapsed += Time.deltaTime;
            if (searchTimeElapsed >= pointWaitTime) return true;

            return false;
        }

        // play start search audio
        void PlaySearchStartAudio()
        {
            if (!playAudioOnSearchStart) return;
            if (InboraStudio.IsAudioScriptableEmpty()) return;

            InboraStudio.PlayAudio(InboraStudio.audioScriptable.GetAudio(AudioScriptable.AudioType.SearchStart));
        }

        // play search end audio
        void PlaySearchEndAudio()
        {
            if (!playAudioOnSearchEnd) return;
            if (InboraStudio.IsAudioScriptableEmpty()) return;

            InboraStudio.PlayAudio(InboraStudio.audioScriptable.GetAudio(AudioScriptable.AudioType.SearchEnd));
        }

        // exit the search and distracted state
        void EndSearchExit()
        {
            InboraStudio.animManager.Play(endSearchAnim, searchAnimsT);
            PlaySearchEndAudio();

            searchTimeElapsed += Time.deltaTime;
            if (searchTimeElapsed >= endSearchAnimTime) {
                idle = true;
                ReturnToAlert();
            }
        }

        void ResetSearching()
        {
            searchIndex = 0;
            isSearching = false;
            searchTimeElapsed = 0f;
        }
        
        #endregion

        #region AUDIOS

        // play audios when AI is waiting for it's turn to attack
        void PlayAttackIdleAudio()
        {
            if (!playAttackIdleAudio || InboraStudio.IsAudioScriptableEmpty()) {
                return;
            }


            // when the attack idle audio finishes -> reset
            if (attackIdleAudioPlayed && !InboraStudio.agentAudio.isPlaying) {
                ResetAttackIdleAudio();
                return;
            }


            // if audio is playing -> return
            if (attackIdleAudioPlayed) {
                return;
            }


            // generate a random time for when passed -> audio is played
            if (chosenAttackIdleAudTime == -1) {
                chosenAttackIdleAudTime = Random.Range(attackIdleAudioTime.x, attackIdleAudioTime.y);
            }


            // timer to pass before playing audio
            attackIdleAudTimer += Time.deltaTime;
            if (attackIdleAudTimer < chosenAttackIdleAudTime) {
                return;
            }


            // make the InboraStudio audio play a random audio and returns true if passed audio is valid and is playing
            if (InboraStudio.PlayAudio(InboraStudio.audioScriptable.GetAudio(AudioScriptable.AudioType.AttackIdle))) {
                attackIdleAudioPlayed = true;
                return;
            }


            // reaching this means the passed audio clip to play is null 
            ResetAttackIdleAudio();
        }

        // reset all the timers and flags of playing attack idle audio
        void ResetAttackIdleAudio()
        {
            attackIdleAudTimer = 0;
            chosenAttackIdleAudTime = -1;
            attackIdleAudioPlayed = false;
        }

        // ready the audio to play on chase
        void ReadyChaseAudio()
        {
            if (InboraStudio == null) return;


            if (InboraStudio.IsAudioScriptableEmpty() || !playAudioOnChase) {
                shouldPlayChaseAudio = false;
                chaseAudioPlayed = false;
                return;
            }

            
            if (chaseAudioIsReady) {
                return;
            }


            chaseAudioPlayed = false;
            chaseAudioIsReady = true;


            if (alwaysPlayOnChase) {
                shouldPlayChaseAudio = true;
                return;
            }


            // randomize chance whether to play or not
            int rand = Random.Range(0, 2);
            
            if (rand == 0) {
                shouldPlayChaseAudio = false;
                return;
            }

            shouldPlayChaseAudio = true;
        }

        // play the chosen chase audio
        void PlayChaseAudio()
        {
            if (InboraStudio.IsAudioScriptableEmpty() || !shouldPlayChaseAudio) {
                chaseAudioIsReady = false;
                return;
            }


            if (chaseAudioPlayed) {
                return;
            }


            lastPlayedChaseAudio = InboraStudio.audioScriptable.GetAudio(AudioScriptable.AudioType.Chase);

            if (InboraStudio.PlayAudio(lastPlayedChaseAudio)) {
                chaseAudioPlayed = true;
            }

            chaseAudioIsReady = false;
        }

        // reset all chase audio flags
        void ResetChaseAudio()
        {
            chaseAudioIsReady = false;
            chaseAudioPlayed = false;
            shouldPlayChaseAudio = false;
        }

        // play audio when moving to attack
        void PlayMoveToAttackAudio()
        {
            if (InboraStudio.IsAudioScriptableEmpty() || !playAudioOnMoveToAttack || moveToAttackAudioPlayed) {
                return;
            }

            moveToAttackAudioPlayed = true;

            if (alwaysPlayOnMoveToAttack) {
                InboraStudio.PlayAudio(InboraStudio.audioScriptable.GetAudio(AudioScriptable.AudioType.MoveToAttack));
                return;
            }

            int rand = Random.Range(0, 2);
            if (rand == 0) {
                return;
            }

            InboraStudio.PlayAudio(InboraStudio.audioScriptable.GetAudio(AudioScriptable.AudioType.MoveToAttack));
        }

        void ResetMoveToAttackAudio()
        {
            moveToAttackAudioPlayed = false;
        }

        #endregion

        #region EQUIP WEAPON
        
        void CheckIfWeaponShouldEquip()
        {
            if (!equipWeapon || hasEquipped) return;
            EquipWeapon();
        }

        public virtual void EquipWeapon()
        {
            if (!isEquipping) {
                onEquipEvent.Invoke();
            }

            isEquipping = true;
            hasEquipped = true;

            InboraStudio.animManager.Play(equipAnim, equipAnimT);
            InboraStudio.RotateTo(InboraStudio.enemyColPoint, turnSpeed);
            
            EquipTimer();
        }

        void EquipTimer()
        {
            equipTimer += Time.deltaTime;
            if (equipTimer < equipDuration) return;
            FinishEquip();
        }

        void FinishEquip()
        {
            equipTimer = 0;
            isEquipping = false;
        }

        void ResetEquipConditionsOnDisable()
        {
            if (!equipWeapon) return;

            if (InboraStudio.state == InboraStudioAI.State.death || InboraStudio.state == InboraStudioAI.State.alert) {
                ResetEquip();
            }
        }

        public virtual void UnEquipWeapon()
        {
            if (!equipWeapon || isUnEquipDone) return;

            if (!isUnEquipping) {
                onUnEquipEvent.Invoke();
            }

            isUnEquipping = true;
            isUnEquipDone = false;

            InboraStudio.animManager.Play(unEquipAnim, equipAnimT);
            UnEquipTimer();
        }

        void UnEquipTimer()
        {
            if (InboraStudio.enemyToAttack) {
                ResetEquip();
                CheckIfWeaponShouldEquip();
                return;
            }

            equipTimer += Time.deltaTime;
            if (equipTimer < unEquipDuration) return;

            FinishUnEquip();
        }

        void FinishUnEquip()
        {
            equipTimer = 0;
            isUnEquipping = false;
            isUnEquipDone = true;
        }

        void ResetEquip()
        {
            hasEquipped = false;
            equipTimer = 0;
            isEquipping = false;
            isUnEquipping = false;
            isUnEquipDone = false;
        }
        
        #endregion
    }
}