using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;

namespace InboraStudioAISpace
{
    [AddComponentMenu("InboraStudio AI/Cover Shooter Behaviour")]
    public class CoverShooterBehaviour : InboraStudioBehaviour
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


        [Min(0), Tooltip("The safe distance between the AI and the target. If the distance difference between the AI and it's target gets greater than this, the AI will chase down the target to close the distance.")]
        public float distanceFromEnemy = 20f;
        [Min(0), Tooltip("The distance between AI and target when actually attacking. For example: if this is a ranged AI you want this value to be far since ranged enemies attack from a distance but if melee then this should be close.")]
        public float attackDistance = 20f;
        [Tooltip("Will check if any of these layers exist before attacking and if so will refrain from attacking until none of these layers exist. You can use this to avoid friendly fire with other AI agents.")]
        public LayerMask layersCheckOnAttacking;
        [Tooltip("Set the animation name of the attack.")]
        public string shootingAnim;
        public float shootingAnimT = 0.1f;
        [Tooltip("The event to trigger during a shot. This should be your shoot method.")]
        public UnityEvent shootEvent;
        [Min(0), Tooltip("The AI will shoot every cycle in a randomized time (seconds) between the two inputs. For a constant value set the two inputs to the same value.")]
        public Vector2 shootEvery = new Vector2(3, 5);
        [Min(0), Tooltip("The duration of the single shot.")] 
        public float singleShotDuration = 0.5f;
        [Min(0), Tooltip("The single shot event is triggered several times. Here you can set the time to pass between each shot until the total time of attack is finished.")]
        public float delayBetweenEachShot = 0.2f;
        [Min(0), Tooltip("The overall total time of shooting.")]
        public Vector2 totalShootTime = new Vector2(2, 4);


        [Tooltip("The chance of attacking the enemy on first sight. If set to [Always Attack] the agent will always attack first. If set to [Take Cover] then the agent will always go to cover first. If set to [Randomize] there's a 50/50 chance either going to cover or attacking.")]
        public FirstSightDecision firstSightDecision = FirstSightDecision.Randomize;
        [Tooltip("Do you want the agent to attack if it's cover is blown and an enemy can see it? If set to [Always Attack], the agent will leave it's cover and attack. If set to [Take Cover] it will refrain from attacking and find another cover. If no cover found it'll attack. If set to [Randomize] there's a 50/50 chance for either taking cover or attacking.")]
        public CoverBlownDecision coverBlownDecision = CoverBlownDecision.Randomize;
        [Tooltip("Should the AI open fire on the cover obstacle the enemy is hiding behind or only attack at the actual enemy. This property will only be taken to account if there's an actual cover the enemy is hiding behind.")]
        public AttackEnemyCover attackEnemyCover = AttackEnemyCover.Randomize;


        [Range(0, 10), Tooltip("How brave do you want the AI? The higher the number, the lower the CHANCE for the AI to go to cover after shooting is done. If the AI is brave enough and finished shooting, it'll wait for it's next attack in the open without going to cover. If set to 0 the AI will always return to cover after every shooting cycle. If set to 10, it'll never go to cover.")]
        public int braveMeter = 0;
        [Range(0, 10), Tooltip("How often do you want the AI to change it's nearest go-to cover? The higher the number, the bigger the chance the AI will change covers and not go to the same cover it took last time. If set to 10, the AI will never go back to the same cover twice in a row. It'll always change after it's shoot cycle. If set to 0, the AI will always choose the best and nearest cover even if it's the same cover as last time.")]
        public int changeCoverFrequency = 0;
        [Range(0, 10), Tooltip("Set the chance you want the AI to shoot again if no covers found. If set to 0, it'll never shoot but rather wait for it's next natural shoot cycle. If set to 10, the AI will always shoot it's target if no covers are found.")]
        public int noCoverShootChance = 5;


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


        [Tooltip("If enabled, the AI will move forward while attacking it's target in different scenarios. Making the AI more dominating.")]
        public bool moveForwards;
        [Min(0), Tooltip("The AI will move forward, making the distance between it and it's target the value set. So for example: if this field is set to 3, the AI will go in close enough to make the distance to the target only 3. So the smaller the number, the more forward the AI advances.")]
        public float moveForwardsToDistance = 5;
        [Min(0), Tooltip("The speed of moving forward.")]
        public float moveForwardsSpeed = 3;
        [Tooltip("The animation name to play when moving forward.")]
        public string moveForwardsAnim;


        [Tooltip("If enabled, the AI will back away if the target moves in too close.")]
        public bool moveBackwards;
        [Min(0), Tooltip("If the distance between the target and the AI is less than this the AI will back away.")]
        public float moveBackwardsDistance = 3f;
        [Min(0)]
        public float moveBackwardsSpeed = 3f;
        public string moveBackwardsAnim;
        [Tooltip("Allow to move backwards while attacking.")]
        public bool moveBackwardsAttack = true;


        [Min(0), Tooltip("The animation transition time for moving forwards & backwards")]
        public float forwardAndBackAnimT = 0.25f;


        [Tooltip("Will enable the AI to strafe in either direction when waiting to attack target.")]
        public bool strafe;
        public float strafeSpeed = 5f;
        [Min(-1), Tooltip("The amount of time to pass strafing before waiting and strafing again. A value will be randomized between the two inputs. For a constant value, set both inputs to the same value. For infinity and never stop strafing set both values to -1.")]
        public Vector2 strafeTime = new Vector2(3, 5);
        [Min(0), Tooltip("The amount of time to wait before strafing again.")]
        public Vector2 strafeWaitTime = new Vector2(0.2f, 1);
        [Tooltip("Left strafe animation name.")]
        public string leftStrafeAnim;
        [Tooltip("Right strafe animation name.")]
        public string rightStrafeAnim;
        [Tooltip("Transition time from current animation to the strafing animation.")]
        public float strafeAnimT = 0.25f;
        [Tooltip("Set all the layers you want to avoid when strafing that includes other InboraStudio AI agents.")]
        public LayerMask strafeLayersToAvoid;


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


        [Tooltip("Play a random audio when AI is chasing target. Set it in the audio scriptable.")]
        public bool playAudioOnChase;
        [Tooltip("If set to true, on chase the AI will always play an audio on chase. If false, there will be a 50/50 chance whether a chase audio will be played or not.")]
        public bool alwaysPlayOnChase;

        [Tooltip("If enabled, will play an audio when AI is shooting. This isn't the firing audio of the gun. But, an audio to play while shooting, like screaming for example.")]
        public bool playAudioDuringShooting;
        [Tooltip("If set to true, on the AI will always play an audio when shooting. If false, there will be a 50/50 chance whether an audio will be played or not.")]
        public bool alwaysPlayDuringShooting;

        [Tooltip("Play audio when AI is moving out of cover and into shoot distance. A random audio will be chosen from the audio scriptable, Move To Shoot array.")]
        public bool playAudioOnMoveToShoot;
        [Tooltip("If set to true, on the AI will always play an audio when moving to shoot distance. If false, there will be a 50/50 chance whether an audio will be played or not.")]
        public bool alwaysPlayOnMoveToShoot;

        #endregion

        #region BEHAVIOUR VARS

        public InboraStudioAI InboraStudio {private set; get; }
        bool isFirstRun = true;
        AlertStateBehaviour alertStateBehaviour;
        GoingToCoverBehaviour gtcBehaviour;
        Collider enemyColl;
        
        public Transform enemyCover { get; private set; }
        public Vector3 hitPoint { get; private set; }
        public GameObject shootingAt { get; private set; }
        public bool isEquipping { get; private set; }
        public bool hasEquipped { get; private set; }

        bool abortStrafe;
        bool sawEnemyOnce;
        bool startTimeUntilAttack;
        bool isStrafeWait;
        bool isStrafing;
        bool isAttacking;
        bool singleShotPass;
        bool shouldAttackCover;
        bool isMovingBackwards;
        bool changeStrafeDir;
        bool braveWait;
        bool shootAnimTriggered;
        bool isSearching;
        bool returnPatrolAudioPlayed;
        bool isReachable;
        bool hitFriendlyFireLayer;
        bool isMoving;
        bool shouldPlayChaseAudio;
        bool chaseAudioIsReady;
        bool chaseAudioPlayed;
        bool duringShootingAudioPlayed;
        bool offMeshBypassed;
        bool isUnEquipping;
        bool isUnEquipDone;
        bool isFoundNoCoverFired;
        bool finishedGetLocationChase;

        float _callOthers = 0;
        float _timeToReturnAlert = 0;
        float _timeUntilAttack;
        float timeUntilAttackElapsed;
        float _totalShootTime;
        float _totalShootTimer = 0;
        float _strafeTime;
        float _strafeTimer;
        float _strafeWaitTime;
        float _strafeWaitTimer = 0;
        float _singleShotTimer = 0;
        float _shootAnimTCompleteTimer = 0;
        float searchTimeElapsed = 0;
        float offMeshTimer = 0;
        float equipTimer = 0;
        float getLocationChaseTimer = 0;
    
        int checkPathFrames = 5;
        int checkPathElapsed = 0;
        int strafingDir = 0;
        int strafeCheckPathElapsed = 0;
        int agentPriority;
        int searchIndex = 0;
        int enemySeenFrames = 0;

        Vector3 lastEnemyPos;
        Vector3 searchPointLocation;

        Transform lastEnemy;

        public enum FirstSightDecision 
        {
            AlwaysAttack,
            TakeCover,
            Randomize
        }

        public enum CoverBlownDecision 
        {
            AlwaysAttack,
            TakeCover,
            Randomize
        }

        public enum AttackEnemyCover 
        {
            AlwaysAttackCover,
            AlwaysAttackEnemy,
            Randomize
        }

        #endregion
        
        #region GARBAGE REDUCTION
        
        Collider[] getTargetCoverHitArr = new Collider[5];
        RaycastHit[] canSeeTargetHitArr = new RaycastHit[10];
        RaycastHit[] strafingHitArr = new RaycastHit[10];
        Collider[] callOthersHitArr = new Collider[20];
        RaycastHit[] checkTargetSeenHitArr = new RaycastHit[12];

        #endregion
        
        #region MAIN METHODS

        public virtual void OnStart()
        {
            isFirstRun = false;
            InboraStudio = GetComponent<InboraStudioAI>();
            alertStateBehaviour = GetComponent<AlertStateBehaviour>();
            gtcBehaviour = GetComponent<GoingToCoverBehaviour>();
            agentPriority = InboraStudio.navmeshAgent.avoidancePriority;

            ReadyChaseAudio();
        }

        public override void Open()
        {
            if (isFirstRun) {
                OnStart();
            }

            onStateEnter.Invoke();
            checkPathElapsed = checkPathFrames;

            if (InboraStudio == null) {      
                Debug.LogWarning($"No InboraStudio AI component found in the gameobject: {gameObject.name}. AI behaviour will have issues.");
                return;
            }
            
            ReadyChaseAudio();
            CheckIfWeaponShouldEquip();
        }

        public override void Close()
        {
            OnDisableReset();
            AllAttackStopReset();
            ResetSearching();
            ResetChaseAudio();
            ResetEquipConditionsOnDisable();
            ResetChaseGetLocation();

            onStateExit.Invoke();

            if (InboraStudio == null) {
                return;
            }

            InboraStudio.navmeshAgent.avoidancePriority = agentPriority;
            
            if (InboraStudio.state != InboraStudioAI.State.goingToCover) {
                InboraStudio.ResetEnemyManager();
            }
        }

        public override void Main()
        {
            // this behaviour should always remove occupation from cover
            gtcBehaviour.RemoveCoverOccupation();

            #if UNITY_EDITOR
            if (!InboraStudio.navmeshAgent.enabled) {
                InboraStudio.PrintWarning(InboraStudio.warnAnomaly, "The Cover Shooter Behaviour is trying to run but the NavMesh Agent component is disabled.");
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
            
            if (InboraStudio.friendly) {
                NoTarget();
                return;
            }


            // run timer to attack
            TimeUntilAttack();
            
            // attack duration timers
            AttackingTimers();

            // force the strafing and moving backwards flags are always false if properties not enabled
            ValidateFlags();


            if (!singleShotPass) {
                shootAnimTriggered = false;
            }


            // if enemy exists -> engage
            if (InboraStudio.enemyToAttack) 
            {
                lastEnemy = InboraStudio.enemyToAttack.transform;
                ResetChaseGetLocation();
                CallOthers();
                Engage(InboraStudio.enemyToAttack.transform, braveWait);
                return;
            }
            
            
            // REACHING THIS POINT MEANS ENEMY DOESN'T EXIST

            NoTargetInView();
            

            // if in companion mode -> return to normal
            if (InboraStudio.companionMode) {
                InboraStudio.SetState(InboraStudioAI.State.normal);
                return;
            }


            // check if called by another agent
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

            
            // check if target has changed the tag to something non-hostile
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


            if (!InboraStudio.isPathReachable) {
                NoTarget();
                return;
            }
            

            GoToLocation(lastEnemyPos);
        }

        void OnDrawGizmosSelected()
        {
            if (showCallRadius) {
                ShowCallRadius();
            }
        }

        void OnValidate()
        {
            if (InboraStudio == null) {
                InboraStudio = GetComponent<InboraStudioAI>();
            }

            if (distanceFromEnemy < moveBackwardsDistance) {
                moveBackwardsDistance = distanceFromEnemy - 0.5f;
            }

            if (moveBackwardsDistance >= distanceFromEnemy) {
                moveBackwardsDistance = distanceFromEnemy - 0.5f;
            }

            if (moveForwards && moveBackwards) {
                if (moveBackwardsDistance >= moveForwardsToDistance) {
                    moveBackwardsDistance = moveForwardsToDistance - 1;
                }
            }

            if (moveBackwardsDistance < 0) {
                moveBackwardsDistance = 0;
            }
        }

        #endregion

        #region ATTACK
        
        public virtual void Engage(Transform target, bool foundNoCoverPass=false)
        {
            _timeToReturnAlert = 0;

            // add the enemy manager to the hostile
            InboraStudio.AddEnemyManager(target, false);

            // check path is reachable every 5 frames
            if (checkPathElapsed >= checkPathFrames) {
                checkPathElapsed = 0;
                CheckHeightAndReachable();
            }

            checkPathElapsed++;

            float minDistance = 0f;
            float backupDistTemp = 0f;

            // set the min distance according to attack and/or enemy manager call
            float[] setDistances = SetDistances();
            minDistance = setDistances[0];
            backupDistTemp = setDistances[1];


            if (InboraStudio.IfShouldIgnoreOffMesh()) {
                IgnoreOffMeshBehaviour();
                return;
            }

            if (!isReachable) {
                UnreachableBehaviour(minDistance);
                return;
            }
            
            ReachableBehaviour(minDistance, backupDistTemp, foundNoCoverPass);
        }
        
        // what to do when in good distance to the enemy
        public virtual void ReachDistance()
        {
            if (sawEnemyOnce) 
            {
                // if brave meter is less or equal to zero -> go to cover
                if (braveMeter == 0) 
                {
                    braveWait = false;
                    InboraStudio.SetState(InboraStudioAI.State.goingToCover);
                    return;
                }

                // if braveness meter is at 10 -> go to brave wait no need to calculate chance
                if (braveMeter == 10) 
                {
                    braveWait = true;
                    return;
                }

                // calculate the odds and chance of going to cover
                int odds = 10 - braveMeter;
                int chanceOfBeingBrave = Random.Range(1, 11);

                if (chanceOfBeingBrave > odds) 
                {
                    braveWait = true;
                    return;
                }

                if (chanceOfBeingBrave <= odds) 
                {
                    braveWait = false;
                    InboraStudio.SetState(InboraStudioAI.State.goingToCover);
                    return;
                }

                return;
            }
            
            sawEnemyOnce = true;
            InboraStudio.animManager.Play(idleAnim, idleMoveT);

            // first sight decision making
            if (firstSightDecision == FirstSightDecision.AlwaysAttack) 
            {
                TurnToAttack();
            }
            else if (firstSightDecision == FirstSightDecision.TakeCover) {
                InboraStudio.SetState(InboraStudioAI.State.goingToCover);
            }
            else 
            {
                int rand = Random.Range(0, 2);

                if (rand == 0) {
                    InboraStudio.SetState(InboraStudioAI.State.goingToCover);
                    return;
                }
                
                TurnToAttack();
            }
        }

        public virtual void IdleStance()
        {
            isMovingBackwards = false;
            
            if (strafe) 
            {
                Strafe();
                return;
            }

            InboraStudio.RotateTo(InboraStudio.enemyToAttack.transform.position, 7);
            InboraStudio.animManager.Play(idleAnim, idleMoveT);
        }

        // move the AI to target position
        public virtual void MoveToTarget(Vector3 pos)
        {
            InboraStudio.MoveTo(pos, moveSpeed, turnSpeed, moveAnim, idleMoveT, "front", distanceFromEnemy);
            
            changeStrafeDir = false;
            _shootAnimTCompleteTimer = 0;
            isMoving = true;

            if (!InboraStudio.isAttacking) {
                PlayChaseAudio();
            }
        }

        // set the min distance for the AI according to conditions
        float[] SetDistances()
        {
            float[] arr = new float[2];
            float minDistance = 0f;
            float backupDistTemp = 0f;


            // set the min distance according to attack and/or enemy manager call            
            if (InboraStudio.enemyManager.callEnemies) {
                if (InboraStudio.isAttacking) {
                    minDistance = attackDistance;
                    
                    if (moveBackwards && moveBackwardsAttack) backupDistTemp = moveBackwardsDistance;
                    else backupDistTemp = 0f;

                    arr[0] = minDistance;
                    arr[1] = backupDistTemp;

                    return arr;
                }
            }
            else {
                InboraStudio.isAttacking = false;
                AllAttackStopReset();
            }


            if (isStrafing) minDistance = distanceFromEnemy + 1;
            else minDistance = distanceFromEnemy;


            if (moveBackwards) backupDistTemp = moveBackwardsDistance;

        
            arr[0] = minDistance;
            arr[1] = backupDistTemp;

            return arr;
        }

        // timer to attack
        public void TimeUntilAttack()
        {
            // if no enemy, InboraStudio not flagged to attack or enemy manager set to not call enemies -> return
            if (!InboraStudio.enemyToAttack || InboraStudio.isAttacking) {
                startTimeUntilAttack = false;
                timeUntilAttackElapsed = 0;
                return;
            }

            
            if (InboraStudio.enemyManager) {
                if (!InboraStudio.enemyManager.callEnemies) {
                    return;
                }
            }


            // set the random time until attack to count up to
            if (!startTimeUntilAttack) {
                _timeUntilAttack = Random.Range(shootEvery.x, shootEvery.y);
                startTimeUntilAttack = true;
            }


            // increment time to attack timer
            timeUntilAttackElapsed += Time.deltaTime;
            if (timeUntilAttackElapsed >= _timeUntilAttack) {
                TurnToAttack();
            }
        }

        // tell this AI to attack
        void TurnToAttack()
        {
            startTimeUntilAttack = false;
            timeUntilAttackElapsed = 0;
            AttackCoverDecision();
            InboraStudio.Attack();
            PlayMoveToShootAudio();
        }

        void AttackCoverDecision()
        {
            // attack cover or not
            if (attackEnemyCover == AttackEnemyCover.AlwaysAttackCover) 
            {
                shouldAttackCover = true;
            }
            else if (attackEnemyCover == AttackEnemyCover.AlwaysAttackEnemy) 
            {
                shouldAttackCover = false;
            }
            else 
            {
                int rand = Random.Range(0, 2);
                
                if (rand == 0) {
                    shouldAttackCover = true;
                    return;
                }
                
                shouldAttackCover = false;
            }
        }
        
        // actual attack (shooting) method
        public virtual void Attack(bool movingBackwards = false, bool movingForwards = false)
        {
            isMoving = false;
            isFoundNoCoverFired = false;

            if (movingForwards) {
                MoveForwards(InboraStudio.enemyColPoint);
                _shootAnimTCompleteTimer += Time.deltaTime;
                if (_shootAnimTCompleteTimer < forwardAndBackAnimT + 0.15f) {
                    return;
                }
            }

            // make sure the shoot anim transition is complete
            if (!isStrafing && !movingForwards) {
                _shootAnimTCompleteTimer += Time.deltaTime;
                if (_shootAnimTCompleteTimer < shootingAnimT + 0.15f) {
                    ShootAnim();
                    return;
                }
            }

            // if not attacking while moving backwards or forwards
            if (!movingBackwards && !movingForwards) {
                // if strafe enabled 
                if (strafe) {
                    Strafe();
                }
            }


            if (singleShotPass) {
                // if waiting to strafe -> (NOT STRAFING YET) then play the shooting anim
                if (!isStrafing && !movingBackwards && !movingForwards) {
                    ShootAnim();
                }
                
                return;
            }
            else {
                if (!isAttacking) {
                    _totalShootTime = Random.Range(totalShootTime.x, totalShootTime.y);
                }
                shootAnimTriggered = false;
            }

            // invoke the selected attack functions
            shootEvent.Invoke();
            PlayDuringShootingAudio();

            
            if (!isStrafing && !movingForwards) {
                ShootAnim();
            }

            isAttacking = true;
            // flags the single shot (we do several shots per attack)
            singleShotPass = true;
        }
        
        // track the timers responsible for attacks
        void AttackingTimers()
        {
            // if not attacking return
            if (!InboraStudio.isAttacking || !InboraStudio.enemyToAttack) {
                AllAttackStopReset(false);
                return;
            }

            // return -> if attack hasn't started or is moving
            if (!isAttacking || isMoving) {
                return;
            }

            InboraStudio.RotateTo(InboraStudio.enemyColPoint, 7);

            // total attack duration timer
            _totalShootTimer += Time.deltaTime;
            
            if (_totalShootTimer >= _totalShootTime) {
                _totalShootTimer = 0;
                StopAttack();
                return;
            }
            
            if (!singleShotPass) {
                return;
            }

            // single shot timer
            _singleShotTimer += Time.deltaTime;

            if (_singleShotTimer >= singleShotDuration) {
                singleShotPass = false;
                _singleShotTimer = 0;
            }
        }

        // stop the shooting cycle
        void StopAttack()
        {
            InboraStudio.isAttacking = false;
            ResetAttackFlags();
            braveWait = false;
            duringShootingAudioPlayed = false;
        }

        // reset all the attack flags back to normal
        void ResetAttackFlags(bool stopStrafing=true)
        {
            isAttacking = false;
            singleShotPass = false;

            if (stopStrafing) {
                isStrafing = false;
                isStrafeWait = false;
            }
            
            _shootAnimTCompleteTimer = 0;
        }

        // stop and reset attack if InboraStudio.StopAttack() used or enemy manager Call Enemies -> set to false
        void AllAttackStopReset(bool stopStrafing=true) 
        {
            ResetAttackFlags(stopStrafing);
            singleShotPass = false;
            _totalShootTimer = 0f;
            _singleShotTimer = 0f;
            _shootAnimTCompleteTimer = 0;
        }

        // get the target's collider
        Collider GetTargetCollider(Transform currentEnemy)
        {
            if (currentEnemy == null) return null;

            if (enemyColl != null && currentEnemy == InboraStudio.previousEnemy) {
                return enemyColl;
            }

            Collider coll = currentEnemy.GetComponent<Collider>();
            return coll;
        }

        // get the cover the target is hiding behind
        public Transform GetTargetCover()
        {
            // cache the target's collider
            enemyColl = GetTargetCollider(InboraStudio.enemyToAttack.transform);
            if (!enemyColl) return null;


            System.Array.Clear(getTargetCoverHitArr, 0, 5);
            int collNum = Physics.OverlapSphereNonAlloc(InboraStudio.enemyColPoint, (enemyColl.bounds.size.x + enemyColl.bounds.size.z) + 0.3f, getTargetCoverHitArr, gtcBehaviour.coverLayers);

            float bestDotProd = -1;
            Transform bestCover = null;

            // get the best cover using dot product
            for (int i=0; i<collNum; i+=1) {
                float dotProd = Vector3.Dot((getTargetCoverHitArr[i].transform.position - transform.position).normalized, transform.forward);
                if (dotProd > bestDotProd) {
                    bestDotProd = dotProd;
                    bestCover = getTargetCoverHitArr[i].transform;
                }
            }

            // get the closest one if dot product way didn't work
            if (bestCover == null && collNum > 0) {
                bestCover = getTargetCoverHitArr[0].transform;
            }

            return bestCover;
        }

        // check whether target is seen or not and cache the hit point
        bool CanSeeTarget(bool onAttacking = false)
        {
            int layers;

            if (onAttacking) {
                layers = layersCheckOnAttacking | InboraStudio.vision.layersToDetect | InboraStudio.vision.hostileAndAlertLayers | gtcBehaviour.coverLayers;
            }
            else {
                layers = InboraStudio.vision.layersToDetect | InboraStudio.vision.hostileAndAlertLayers | gtcBehaviour.coverLayers;
            }

            Collider enemyCollider = GetTargetCollider(InboraStudio.enemyToAttack.transform);
            Vector3 targetDir = enemyCollider.ClosestPoint(transform.position + InboraStudio.vision.visionPosition) - transform.position;

            System.Array.Clear(canSeeTargetHitArr, 0, 10);
            int hits = Physics.SphereCastNonAlloc(transform.position + InboraStudio.vision.visionPosition, 0.05f, (targetDir - InboraStudio.vision.visionPosition).normalized, canSeeTargetHitArr, InboraStudio.distanceToEnemy + 5f, layers);

            // sort the list by distance
            float smallestDist = Mathf.Infinity;
            int bestIndex = 0;

            for (int i=0; i<hits; i++) 
            {
                // if sphere cast hits the same AI collider -> ignore
                if (transform.IsChildOf(canSeeTargetHitArr[i].transform) || canSeeTargetHitArr[i].transform.IsChildOf(transform)) {
                    continue;
                }

                if (canSeeTargetHitArr[i].distance == 0 || canSeeTargetHitArr[i].point == Vector3.zero) {
                    continue;
                }

                if (canSeeTargetHitArr[i].distance <= smallestDist) {
                    smallestDist = canSeeTargetHitArr[i].distance;
                    bestIndex = i;
                }
            }

            Transform item = canSeeTargetHitArr[bestIndex].transform;

            // check not hitting the last cover of AI
            if (gtcBehaviour.lastCover != null) 
            {
                if (item != null) {
                    if (gtcBehaviour.lastCover.IsChildOf(canSeeTargetHitArr[bestIndex].transform) || canSeeTargetHitArr[bestIndex].transform.IsChildOf(gtcBehaviour.lastCover)) {
                        return false;
                    }
                }
            }

            // check for friendly fire
            if (onAttacking) 
            {
                if (item != null) {
                    if ((!InboraStudio.enemyToAttack.transform.IsChildOf(canSeeTargetHitArr[bestIndex].transform) && !canSeeTargetHitArr[bestIndex].transform.IsChildOf(InboraStudio.enemyToAttack.transform)) && (layersCheckOnAttacking & (1 << canSeeTargetHitArr[bestIndex].transform.gameObject.layer)) != 0) {
                        hitFriendlyFireLayer = true;
                        return false;
                    }
                }
            }

            hitFriendlyFireLayer = false;
            hitPoint = canSeeTargetHitArr[bestIndex].point;

            if (canSeeTargetHitArr[bestIndex].transform != null) {
                shootingAt = canSeeTargetHitArr[bestIndex].transform.gameObject;
            }

            int detectionHits = 2;
            if (InboraStudio.distanceToEnemy < InboraStudio.navmeshAgent.height + 1)
            {
                detectionHits = 3;
            }

            // check if target's cover should be a target
            if (shouldAttackCover) 
            {
                if (item != null) {
                    if (canSeeTargetHitArr[bestIndex].distance > InboraStudio.distanceToEnemy/2) {
                        enemyCover = GetTargetCover();
                    }
                    else {
                        // remove cover if distance is too small
                        // or else the AI will shoot cover at point blank
                        enemyCover = null;
                    }
                }
                else {
                    enemyCover = null;
                }

                
                if (enemyCover) {
                    if (item != null) {
                        if (enemyCover.IsChildOf(canSeeTargetHitArr[bestIndex].transform) || canSeeTargetHitArr[bestIndex].transform.IsChildOf(enemyCover)) {
                            return true;
                        }
                    }

                    return InboraStudio.CheckObjectCompletelyVisible(InboraStudio.enemyToAttack, layers, detectionHits);
                }

                return InboraStudio.CheckObjectCompletelyVisible(InboraStudio.enemyToAttack, layers, detectionHits);
            }
            
            return InboraStudio.CheckObjectCompletelyVisible(InboraStudio.enemyToAttack, layers, detectionHits);
        }

        void CheckHeightAndReachable()
        {
            RaycastHit heightHit;
            
            if (Physics.Raycast(InboraStudio.enemyColPoint, -Vector3.up, out heightHit, Mathf.Infinity, InboraStudio.groundLayers)) 
            {   
                Vector3 closestPoint;
                InboraStudio.ClosestNavMeshPoint(heightHit.point, InboraStudio.navmeshAgent.radius * 2, out closestPoint);
                isReachable = InboraStudio.IsPathReachable(closestPoint);
                return;
            }
            
            Collider enemyColl = GetTargetCollider(InboraStudio.enemyToAttack.transform);
            isReachable = InboraStudio.IsPointOnNavMesh(enemyColl.bounds.min, InboraStudio.navmeshAgent.radius * 2);
        }
        
        public virtual void ReachableBehaviour(float minDistance, float backupDistTemp, bool foundNoCoverPass)
        {
            abortStrafe = false;

            // check if AI doesn't need to back away
            if (InboraStudio.distanceToEnemySqrMag > (backupDistTemp * backupDistTemp)) 
            {
                isMovingBackwards = false;
        
                // if distance is bigger than minimum -> move to close the distance
                if (InboraStudio.distanceToEnemySqrMag > (minDistance * minDistance)) 
                {
                    MoveToTarget(InboraStudio.enemyColPoint);
                    return;
                }

                // check if the distance between AI and enemy is good enough to move forward
                bool canMoveForward = false;
                if (moveForwards) {
                    if (InboraStudio.distanceToEnemySqrMag - ((backupDistTemp * backupDistTemp)) > (moveForwardsToDistance * moveForwardsToDistance)) {
                        canMoveForward = true;
                    }
                }
                
                // if reached min distance and is attacking true -> launch attack
                if (InboraStudio.isAttacking) 
                {
                    if (CanSeeTarget(true)) 
                    {
                        if (enemySeenFrames < 6) {
                            enemySeenFrames++;
                        }
                        
                        if (enemySeenFrames >= 6) {
                            Attack(false, canMoveForward);
                            enemySeenFrames = 6;
                            return;
                        }

                        if (canMoveForward) {
                            MoveForwards(InboraStudio.enemyColPoint);
                        }
                        else {
                            IdleStance();
                        }
                        
                        return;
                    }
                    
                    // sphere cast has hit a layersCheckOnAttacking property (friendly fire)
                    if (isAttacking && hitFriendlyFireLayer) {
                        IdleStance();
                        return;
                    }

                    if (enemySeenFrames > 0) {
                        enemySeenFrames -= 1;

                        if (moveForwards) {
                            MoveForwards(InboraStudio.enemyColPoint);
                        }
                        else {
                            Attack(false, canMoveForward);
                        }
                    }
                    else {
                        MoveToTarget(InboraStudio.enemyColPoint);
                    }

                    return;
                }
                
                abortStrafe = false;

                if (foundNoCoverPass) 
                {
                    IdleStance();
                    return;
                }
                
                ReachDistance();
                return;
            }

            
            // STARTING FROM HERE MEANS THE AI WILL BACK AWAY

            
            // only backup if seeing target
            if (!CanSeeTarget(InboraStudio.isAttacking)) 
            {
                if (!InboraStudio.isAttacking) {
                    ReachDistance();
                    return;
                }

                if (enemySeenFrames > 0) {
                    enemySeenFrames -= 2;
                }
                else {
                    MoveToTarget(InboraStudio.enemyColPoint);
                }
                
                return;
            }

            if (enemySeenFrames < 6) 
            {
                enemySeenFrames++;
                IdleStance();
                return;
            }
            
            // check if target will be seen from backup point
            if (CheckIfTargetSeenFromPoint(transform.position - (transform.forward * (InboraStudio.navmeshAgent.height - 0.25f)))) 
            {
                MoveBackwards(InboraStudio.enemyColPoint);
            }
            else {
                isMovingBackwards = false;

                if (!InboraStudio.isAttacking) {
                    if (strafe) {
                        Strafe();
                    }
                }
            }

            if (InboraStudio.isAttacking) 
            {
                Attack(isMovingBackwards);
                return;
            }

            ReachDistance();
        }

        public virtual void UnreachableBehaviour(float minDistance)
        {
            if (InboraStudio.distanceToEnemySqrMag <= minDistance * minDistance) 
            {
                if (InboraStudio.isAttacking) {
                    abortStrafe = false;
                    
                    if (CanSeeTarget(true)) {
                        Attack();
                        return;
                    }
        
                    MoveToTarget(InboraStudio.enemyColPoint);
                    return;
                }

                ReachDistance();
                return;
            }
            
            InboraStudio.isAttacking = false;
            abortStrafe = true;
            IdleStance();
        }

        #endregion
        
        #region STRAFING
        
        // prepare the strafe
        void Strafe()
        {
            if (abortStrafe) {
                if (!singleShotPass) {
                    IdleAnim();
                }

                return;
            }

            // if waiting for strafe wait (time to pass before strafing)
            if (isStrafeWait) {
                // rotate to enemy always and play idle anim
                if (!singleShotPass) {
                    IdleAnim();
                }
                
                // count up the strafe wait timer
                StrafeWait();
                return;
            }

            // if strafing continue on with strafe movement
            if (isStrafing) {
                StrafeMovement(strafingDir);
                return;
            }

            
            // IF REACHED THIS POINT THEN THIS IS THE FIRST RUN
            
            
            // set the time of strafing
            if (strafeTime.x == -1 && strafeTime.y == -1) {
                _strafeTime = Mathf.Infinity;
            }
            else {
                _strafeTime = Random.Range(strafeTime.x, strafeTime.y);
            }

            // set the wait time
            _strafeWaitTime = Random.Range(strafeWaitTime.x, strafeWaitTime.y);

            // check for flag to change current dir
            if (changeStrafeDir) {
                if (strafingDir == 0) strafingDir = 1;
                else strafingDir = 0;

                changeStrafeDir = false;
            }
            else {
                strafingDir = Random.Range(0, 2);
            }

            isStrafeWait = true;
        }

        // the actual strafing movement
        public virtual void StrafeMovement(int direction)
        {   
            int layersToHit = InboraStudio.vision.hostileAndAlertLayers | strafeLayersToAvoid | InboraStudio.vision.layersToDetect;

            Vector3 strafePoint = Vector3.zero;
            Vector3 offsetPlayer;
            Vector3 transformDir;
            Vector3 shoulderOffset;

            string strafeAnim;
            string moveDir;

            Vector3 enemyToAttackPos = new Vector3(InboraStudio.enemyColPoint.x, transform.position.y, InboraStudio.enemyColPoint.z);
            float distanceDiff = Vector3.Distance(enemyToAttackPos, transform.position);
            

            // if direction is left
            if (direction == 0) {
                offsetPlayer = enemyToAttackPos - transform.position;
                offsetPlayer = Vector3.Cross(offsetPlayer, Vector3.up);

                strafePoint = InboraStudio.ValidateYPoint(offsetPlayer);
                strafePoint = transform.position + new Vector3(strafePoint.x, 0, strafePoint.z + 0.5f);
                strafePoint = strafePoint + (transform.right * (distanceDiff - 1));

                transformDir = -transform.right;

                // to check from an offset if enemy will not be visible
                shoulderOffset = transform.TransformPoint(new Vector3(-(InboraStudio.navmeshAgent.radius + 0.7f), 0f, 0f) + InboraStudio.vision.visionPosition);

                // set the anim and move dir
                strafeAnim = leftStrafeAnim;
                moveDir = "left";
            }
            else {
                offsetPlayer = transform.position - enemyToAttackPos;
                offsetPlayer = Vector3.Cross(offsetPlayer, Vector3.up);

                strafePoint = InboraStudio.ValidateYPoint(offsetPlayer);
                strafePoint = transform.position + new Vector3(strafePoint.x, 0, strafePoint.z + 0.5f);
                strafePoint = strafePoint + (-transform.right * (distanceDiff - 1));

                transformDir = transform.right;

                // to check from an offset if enemy will not be visible
                shoulderOffset = transform.TransformPoint(new Vector3((InboraStudio.navmeshAgent.radius + 0.7f), 0f, 0f) + InboraStudio.vision.visionPosition);
                
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

                Vector3 goToPoint = InboraStudio.ValidateYPoint((transform.position + InboraStudio.vision.visionPosition) + (transformDir * (InboraStudio.navmeshAgent.radius * 2 + (InboraStudio.navmeshAgent.height / 2) )));
                if (!InboraStudio.IsPathReachable(goToPoint)) {
                    ChangeStrafeDirection();
                    return;
                }
            }

            strafeCheckPathElapsed++;


            // check if there's an obstacle in strafe direction
            System.Array.Clear(strafingHitArr, 0, 10);
            int hits = Physics.SphereCastNonAlloc(transform.position + InboraStudio.vision.visionPosition, 0.15f, transformDir, strafingHitArr, (InboraStudio.navmeshAgent.radius * 2) + InboraStudio.navmeshAgent.height/2, layersToHit);
            int hitIndex = -1;
            
            // filter
            for (int i=0; i<hits; i++) 
            {
                // if sphere cast hits the same AI collider -> ignore
                if (transform.IsChildOf(strafingHitArr[i].transform) || strafingHitArr[i].transform.IsChildOf(transform)) {
                    continue;
                }

                if (strafingHitArr[i].distance == 0 || strafingHitArr[i].point == Vector3.zero) {
                    continue;
                }

                hitIndex = i;
                break;
            }

            if (hitIndex > -1) {
                ChangeStrafeDirection();
                return;
            }

            layersToHit = InboraStudio.vision.hostileAndAlertLayers | InboraStudio.vision.layersToDetect;
            float rayDistance =  Vector3.Distance(InboraStudio.enemyColPoint, transform.position) + 5;
            
            if (shouldAttackCover) 
            {
                if (enemyCover) {
                    if (!InboraStudio.CheckTargetVisibleWithRay(enemyCover.transform, shoulderOffset, InboraStudio.enemyColPoint - shoulderOffset, rayDistance, layersToHit) 
                        && !InboraStudio.CheckTargetVisibleWithRay(InboraStudio.enemyToAttack.transform, shoulderOffset, InboraStudio.enemyColPoint - shoulderOffset, rayDistance, layersToHit)) {
                        ChangeStrafeDirection();
                        return;
                    }
                }
                else {
                    if (!InboraStudio.CheckTargetVisibleWithRay(InboraStudio.enemyToAttack.transform, shoulderOffset, InboraStudio.enemyColPoint - shoulderOffset, rayDistance, layersToHit)) {
                        ChangeStrafeDirection();
                        return;
                    }
                }
            }
            else {
                if (!InboraStudio.CheckTargetVisibleWithRay(InboraStudio.enemyToAttack.transform, shoulderOffset, InboraStudio.enemyColPoint - shoulderOffset, rayDistance, layersToHit)) {
                    ChangeStrafeDirection();
                    return;
                }
            }


            isStrafing = true;
            InboraStudio.RotateTo(InboraStudio.enemyToAttack.transform.position, 20f);
            InboraStudio.MoveTo(strafePoint, strafeSpeed, 0, strafeAnim, strafeAnimT, moveDir);
        

            _strafeTimer += Time.deltaTime;
            if (_strafeTimer >= _strafeTime) {
                StopStrafe();
            }
        }

        // wait before strafing
        void StrafeWait()
        {
            _strafeWaitTimer += Time.deltaTime;
            if (_strafeWaitTimer >= _strafeWaitTime) {
                isStrafing = true;
                isStrafeWait = false;
                _strafeWaitTimer = 0;
            }
        }

        // stop the strafing
        void StopStrafe()
        {
            isStrafeWait = false;
            _strafeWaitTimer = 0f;
            isStrafing = false;
            _strafeTimer = 0f;
        }

        // change the strafing to the opposite direction
        void ChangeStrafeDirection()
        {   
            changeStrafeDir = true;
            StopStrafe();
        }
        
        #endregion

        #region MOVE FORWARDS & BACKWARDS
        
        public virtual void MoveForwards(Vector3 location)
        {
            // moves AI to location and returns true when reaches location
            if (InboraStudio.MoveTo(location, moveForwardsSpeed, turnSpeed, moveForwardsAnim, forwardAndBackAnimT)) {
                IdleStance();
                return;
            }

            isStrafing = false;
            isStrafeWait = false;
        }

        // back away from target
        public virtual void MoveBackwards(Vector3 target)
        {
            Vector3 targetPosition = transform.position - (transform.forward * (InboraStudio.navmeshAgent.height - 0.25f));
            Vector3 backupPoint = InboraStudio.ValidateYPoint(targetPosition);
            RaycastHit hit;


            backupPoint = new Vector3 (backupPoint.x, transform.position.y, backupPoint.z + 0.5f);
            int layers = InboraStudio.vision.layersToDetect | agentLayersToCall | gtcBehaviour.coverLayers | strafeLayersToAvoid;


            // check if obstacle is behind
            if (Physics.Raycast(transform.position + InboraStudio.vision.visionPosition, -transform.forward, out hit, InboraStudio.navmeshAgent.radius * 2 + 0.3f, layers)) {
                IdleStance();
                return;
            }

            
            // if point isn't reachable
            if (!InboraStudio.IsPathReachable(backupPoint)) {
                IdleStance();
                return;
            }

            
            // if strafing we need further precision to check if moving backwards is possible
            // to prevent disabling strafing to find the AI moves backwards a very neglibile distance which makes it look very bad
            if (isStrafing) 
            {
                if (Physics.Raycast(transform.position + InboraStudio.vision.visionPosition, -transform.forward, out hit, moveBackwardsDistance / 1.5f + InboraStudio.navmeshAgent.radius, layers)) 
                {
                    IdleStance();
                    return;
                }
                else {
                    Vector3 checkPoint = new Vector3(backupPoint.x, transform.position.y, backupPoint.z + (moveBackwardsDistance / 2f));
                    if (!InboraStudio.IsPathReachable(checkPoint)) {
                        IdleStance();
                        return;
                    }
                }
            }


            // cancel strafing when backing away
            StopStrafe();
            

            // back away
            InboraStudio.RotateTo(target, turnSpeed);
            InboraStudio.MoveTo(backupPoint, moveBackwardsSpeed, 0f, moveBackwardsAnim, forwardAndBackAnimT, "backwards");


            isMovingBackwards = true;
        }
        
        #endregion

        #region CALL OTHERS
        
        // call nearby agents to target location
        public virtual void CallOthers()
        {
            // if call others isn't enabled or no target
            if (!callOthers || !InboraStudio.enemyToAttack) {
                return;
            }


            // time to pass before firing
            _callOthers += Time.deltaTime;
            if (_callOthers < callOthersTime) return;
            _callOthers = 0;


            System.Array.Clear(callOthersHitArr, 0, 20);
            int callOthersNum = Physics.OverlapSphereNonAlloc(transform.position, callRadius, callOthersHitArr, agentLayersToCall);

            for (int i=0; i<callOthersNum; i+=1) {
                InboraStudioAI script = callOthersHitArr[i].GetComponent<InboraStudioAI>();
                AttackStateBehaviour attackBehaviour = callOthersHitArr[i].GetComponent<AttackStateBehaviour>();
                CoverShooterBehaviour coverShooterBehaviour = callOthersHitArr[i].GetComponent<CoverShooterBehaviour>();

                // if the caught object is actually the enemy of the AI (AI vs AI) -> don't call
                if (callOthersHitArr[i].transform.IsChildOf(InboraStudio.enemyToAttack.transform)) {
                    continue;
                }


                if (callOthersHitArr[i].transform.IsChildOf(transform)) {
                    continue;
                }


                // if doesn't have InboraStudio AI
                if (!script) {
                    continue;
                }


                if (script.friendly) {
                    continue;
                }


                // if doesn't have this AttackStateBehaviour script
                if (!attackBehaviour && !coverShooterBehaviour) {
                    continue;
                }


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

                // turn the agents to attack state
                script.TurnToAttackState();
            }
        }

        #endregion

        #region GETTING CALLED BY OTHERS
        
        public virtual void GoToLocation(Vector3 location)
        {
            sawEnemyOnce = true;
            
            // moves AI to location and returns true when reaches location
            if (InboraStudio.MoveTo(location, moveSpeed, turnSpeed, moveAnim, idleMoveT)) {
                NoTarget();
                return;
            }

            PlayChaseAudio();
        }
        
        #endregion

        #region MISC

        // fired by the GoingToCoverBehaviour script when no covers found
        public virtual void FoundNoCover()
        {
            if (noCoverShootChance == 0) {
                IdleFoundNoCover();
                return;
            }

            if (noCoverShootChance == 10) {
                InboraStudio.ResetVisionPosition();
                InboraStudio.Attack();
                return;
            }

            if (isFoundNoCoverFired) {
                if (!isAttacking) {
                    IdleFoundNoCover();
                }
                return;
            }

            isFoundNoCoverFired = true;

            // calculate the chance of going to cover if value is between 1-9
            int odds = 10 - noCoverShootChance;
            int chanceOfShooting = Random.Range(1, 11);

            if (chanceOfShooting > odds) 
            {
                InboraStudio.ResetVisionPosition();
                InboraStudio.Attack();
                return;
            }

            if (chanceOfShooting <= odds) 
            {
                IdleFoundNoCover();
                return;
            }
        }

        public virtual void IdleFoundNoCover()
        {
            Engage(InboraStudio.enemyToAttack.transform, true);
            CallOthers();
            
            if (gtcBehaviour.onlyAttackAfterCover) {
                TimeUntilAttack();
            }

            InboraStudio.ResetVisionPosition();
        }

        // show the call others radius in scene view
        void ShowCallRadius()
        {
            if (!callOthers) return;

            Gizmos.color = Color.grey;
            Gizmos.DrawWireSphere(transform.position, callRadius);
        }

        void ResetTimers()
        {
            _callOthers = 0;
            _totalShootTimer = 0;
            _singleShotTimer = 0;
            timeUntilAttackElapsed = 0;
        }

        void NoTargetInView()
        {
            StopAttack();
            StopStrafe();

            sawEnemyOnce = false;
            gtcBehaviour.RemoveCoverOccupation();
        }

        public void ResetTimeUntilAttack()
        {
            timeUntilAttackElapsed = 0;
        }

        // check if target is seen from a certain position
        public bool CheckIfTargetSeenFromPoint(Vector3 point, bool useNewCenterPos=false, float visionPosH = -1)
        {
            int layers = InboraStudio.vision.layersToDetect | InboraStudio.vision.hostileAndAlertLayers | gtcBehaviour.coverLayers;
            float visionPosHeight;

            if (useNewCenterPos) {
                visionPosHeight = visionPosH;
            }
            else {
                visionPosHeight = InboraStudio.defaultVisionPos.y;
            }

            Vector3 finalCenterPos = new Vector3(InboraStudio.vision.visionPosition.x, visionPosHeight, InboraStudio.vision.visionPosition.z);
            Vector3 dir = (InboraStudio.enemyColPoint - (point + finalCenterPos)).normalized;
            
            System.Array.Clear(checkTargetSeenHitArr, 0, 12);
            int sphereCastHits = Physics.SphereCastNonAlloc(point + finalCenterPos, 0.05f, dir, checkTargetSeenHitArr, Vector3.Distance(point, InboraStudio.enemyColPoint) + 3, layers);
            float smallestDist = Mathf.Infinity;
            int bestIndex = -1;

            // filter the results
            for (int i=0; i<sphereCastHits; i++) 
            {
                if (transform.IsChildOf(checkTargetSeenHitArr[i].transform) || checkTargetSeenHitArr[i].transform.IsChildOf(transform)) {
                    continue;
                }

                // get the smallest distance
                if (checkTargetSeenHitArr[i].distance <= smallestDist) {
                    smallestDist = checkTargetSeenHitArr[i].distance;
                    bestIndex = i;
                }
            }

            if (bestIndex == -1) return false;

            if (InboraStudio.enemyToAttack.transform.IsChildOf(checkTargetSeenHitArr[bestIndex].transform) || checkTargetSeenHitArr[bestIndex].transform.IsChildOf(InboraStudio.enemyToAttack.transform)) {
                return true;
            }
            
            return false;
        }

        // force the strafing and moving backwards flags are always false if properties not enabled
        void ValidateFlags()
        {
            if (!strafe) {
                isStrafeWait = false;
                isStrafing = false;
            }

            if (!moveBackwards) {
                isMovingBackwards = false;
            }
        }

        void OnDisableReset()
        {
            returnPatrolAudioPlayed = false;
            isReachable = false;
            hitFriendlyFireLayer = false;
            duringShootingAudioPlayed = false;
            offMeshBypassed = false;
            InboraStudio.isTargetTagChanged = false;
            isFoundNoCoverFired = false;
            enemySeenFrames = 0;
            offMeshTimer = 0;
        }

        void IgnoreOffMeshBehaviour()
        {
            if (InboraStudio.enemyToAttack != null) 
            {
                if (!offMeshBypassed) {
                    InboraStudio.navmeshAgent.Warp(transform.position);
                    offMeshBypassed = true;
                }

                IdleStance();
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

        #region ANIMATIONS
        
        // idle animation
        void IdleAnim()
        {
            InboraStudio.RotateTo(InboraStudio.enemyToAttack.transform.position, 7);
            InboraStudio.animManager.Play(idleAnim, idleMoveT);
        }

        // shoot animation
        void ShootAnim()
        {
            InboraStudio.RotateTo(InboraStudio.enemyToAttack.transform.position, 7);

            if (shootAnimTriggered) return;

            InboraStudio.animManager.Play(shootingAnim, shootingAnimT, true);
            shootAnimTriggered = true;
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
            if (searchLocationRadius) {
                InboraStudio.animManager.Play(idleAnim, returnPatrolAnimT);
                searchTimeElapsed += Time.deltaTime;

                if (searchTimeElapsed >= timeToStartSearch) {
                    PlaySearchStartAudio();
                    SetSearchPoint();
                    InboraStudio.checkEnemyPosition = Vector3.zero;
                    return;
                }

                return;
            }

            PlayReturnPatrolAudio();
            ReturnToAlertIdle();
            ResetTimers();
            
            _timeToReturnAlert += Time.deltaTime;
            if (_timeToReturnAlert < returnPatrolTime) return;
            
            ReturnToAlert();
        }

        // play return animation
        void ReturnToAlertIdle()
        {
            InboraStudio.SetState(InboraStudioAI.State.returningToAlert);
            
            if (returnPatrolAnim.Length == 0) {
                InboraStudio.animManager.Play(idleAnim, returnPatrolAnimT);
            }
            else {
                InboraStudio.animManager.Play(returnPatrolAnim, returnPatrolAnimT);
            }
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
            if (returnPatrolAudioPlayed) {
                return;
            }


            if (!playAudioOnReturnPatrol) {
                return;
            }


            if (InboraStudio.IsAudioScriptableEmpty()) {
                return;
            }


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

            searchTimeElapsed += Time.deltaTime;

            if (searchTimeElapsed >= endSearchAnimTime) {
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

        void PlayChaseAudio()
        {
            if (InboraStudio.IsAudioScriptableEmpty() || !shouldPlayChaseAudio) {
                chaseAudioIsReady = false;
                return;
            }

            if (chaseAudioPlayed) {
                return;
            }

            if (InboraStudio.PlayAudio(InboraStudio.audioScriptable.GetAudio(AudioScriptable.AudioType.Chase))) {
                chaseAudioPlayed = true;
            }

            chaseAudioIsReady = false;
        }

        void ResetChaseAudio()
        {
            chaseAudioIsReady = false;
            chaseAudioPlayed = false;
            shouldPlayChaseAudio = false;
        }

        void PlayDuringShootingAudio()
        {
            if (InboraStudio.IsAudioScriptableEmpty() || !playAudioDuringShooting) {
                duringShootingAudioPlayed = false;
                return;
            }


            if (duringShootingAudioPlayed) {
                return;
            }


            if (alwaysPlayDuringShooting) {
                if (InboraStudio.PlayAudio(InboraStudio.audioScriptable.GetAudio(AudioScriptable.AudioType.DuringShooting))) {
                    duringShootingAudioPlayed = true;
                }

                return;
            }


            duringShootingAudioPlayed = true;
            int rand = Random.Range(0, 2);
            
            if (rand == 0) {
                return;
            }

            InboraStudio.PlayAudio(InboraStudio.audioScriptable.GetAudio(AudioScriptable.AudioType.DuringShooting));
        }

        void PlayMoveToShootAudio()
        {
            if (InboraStudio.IsAudioScriptableEmpty() || !playAudioOnMoveToShoot) {
                return;
            }

            if (alwaysPlayOnMoveToShoot) {
                InboraStudio.PlayAudio(InboraStudio.audioScriptable.GetAudio(AudioScriptable.AudioType.MoveToShoot));
                return;
            }

            int rand = Random.Range(0, 2);
            
            if (rand == 0) {
                return;
            }

            InboraStudio.PlayAudio(InboraStudio.audioScriptable.GetAudio(AudioScriptable.AudioType.MoveToShoot));
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