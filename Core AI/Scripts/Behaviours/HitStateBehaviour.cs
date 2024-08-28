using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace InboraStudioAISpace
{
    [AddComponentMenu("InboraStudio AI/Hit State Behaviour")]
    public class HitStateBehaviour : InboraStudioBehaviour
    {   
        #region PROPERTIES

        [Header("Hit Properties"), Tooltip("Hit animation names and their durations. One will be chosen at random on every hit.")]
        public List<HitData> hitAnims;
        [Min(0), Tooltip("The animation transition from current animation to the hit animation.")]
        public float hitAnimT = 0.2f;
        [Min(0), Tooltip("The gap time between replaying the hit animations to avoid having the animation play on every single hit which may look bad on very fast and repitive attacks such as a machine gun.")]
        public float hitAnimGap = 0.3f;
        

        [Header("Knock Out"), Min(0.1f), Tooltip("The duration in seconds to stay knocked out before getting up.")]
        public float knockOutDuration = 3;
        [Tooltip("The actual animation clip name of getting up from knock out if face up (lying on back).")]
        public string faceUpStandClipName;
        [Tooltip("The actual animation clip name of getting up from knock out when face down (facing ground).")]
        public string faceDownStandClipName;
        [Min(0), Tooltip("Ragdoll To Stand Speed: The transition speed from the ragdoll to the getting up animation.")]
        public float ragdollToStandSpeed = 0.3f;
        [Tooltip("Set the hip/pelvis bone of the AI so getting up after knock out is accurate.")]
        public Transform hipBone;

        [Header("Knock Out Force"), Tooltip("If enabled, on knock out the ragdoll will use the natural velocity of the rigidbody at that moment in time with no additional force.")]
        public bool useNaturalVelocity;
        [Tooltip("If you don't want to use the natural velocity, you can add your own. This can also be changed dynamically through code before calling KnockOut() to add force to the ragdoll depending on the type of hit/weapon.")]
        public Vector3 knockOutForce;


        [Header("Cancel Attack"), Tooltip("If set to true will cancel the attack if got hit or knocked out.")]
        public bool cancelAttackOnHit;
        
        
        [Header("Audio"), Tooltip("Play audio when hit. Set your audios in the audio scriptable in the General Tab in InboraStudio AI.")]
        public bool playAudio;
        [Tooltip("If enabled, a hit audio will always play when hit. If false, there's a 50/50 chance whether an audio will be played or not.")]
        public bool alwaysPlayAudio = true;

        
        [Header("Call Others"), Min(0), Tooltip("The radius to call other AIs when hit. You use this by calling InboraStudio.Hit(player, true).")]
        public float callOthersRadius = 5;
        [Tooltip("The layers of the agents to call. You use this by calling InboraStudio.Hit(player, true).")]
        public LayerMask agentLayersToCall;
        [Tooltip("Shows the call radius as a cyan wire sphere in the scene view.")]
        public bool showCallRadius;


        public UnityEvent onStateEnter;
        public UnityEvent onStateExit;

        #endregion
        
        #region BEHAVIOUR VARIABLES

        bool isFirstRun = true;
        public InboraStudioAI InboraStudio;

        bool playedAudio;

        float _duration = 0;
        float _gapTimer = 0;
        float hitDuration = 0;

        [System.Serializable]
        public struct HitData 
        {
            [Tooltip("Set the animation name of the hit.")]
            public string animName;
            [Tooltip("Set the duration of the hit state for this animation.")]
            public float duration;
        }

        public enum RagdollState
        {
            None,
            Ragdoll,
            StandingUp,
            ResettingBones
        }

        public RagdollState ragdollState = RagdollState.None;

        class BoneTransform
        {
            public Vector3 Position { get; set; }
            public Quaternion Rotation { get; set; }
        }

        BoneTransform[] standUpBoneTransforms;
        BoneTransform[] ragdollBoneTransforms;
        Transform[] bones;
        Rigidbody hipBoneRb;

        float elapsedResetBonesTime;
        float facePos;
        float standingUpTimer = 0;
        bool getUpYonBul;
        string currGetUpAnim;
        int lastRegisteredKnockOuts = 0;

        #endregion
        
        #region GARBAGE REDUCTION
        
        Collider[] callOthersHitArr = new Collider[20];
        
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

            if (InboraStudio == null) {
                Debug.LogWarning($"No InboraStudio AI component found in the gameobject: {gameObject.name}. AI behaviour will have issues.");
                return;
            }

            if (InboraStudio.hitProps.callOthers) {
                CallOthers();
            }
            
            // enable ragdoll if knock out is registered
            if (InboraStudio.hitProps.knockOutRegister != lastRegisteredKnockOuts) {
                if (hipBone != null) {
                    hipBoneRb = hipBone.GetComponent<Rigidbody>();  
                }

                if (bones == null || bones.Length <= 0) {
                    PrepareBones();
                }
                
                TriggerRagdoll();
            }
        }

        public override void Close()
        {
            ResetTimers();
            lastRegisteredKnockOuts = 0;
            onStateExit.Invoke();
            
            if (InboraStudio == null) {
                return;
            }

            if (InboraStudio.state != InboraStudioAI.State.death) {
                InboraStudio.navmeshAgent.enabled = true;
            }

            InboraStudio.hitProps.hitEnemy = null;
        }
        
        public override void Main()
        {
            if (cancelAttackOnHit) {
                InboraStudio.StopAttack();
            }
            
            if (InboraStudio.hitProps.knockOutRegister != lastRegisteredKnockOuts) {
                Open();
                return;
            }

            switch (ragdollState) 
            {
                case RagdollState.None:
                    HitBehaviour();
                    break;

                case RagdollState.Ragdoll:
                    RagdollBehaviour();
                    break;

                case RagdollState.StandingUp:
                    StandingUpBehaviour();
                    break;

                case RagdollState.ResettingBones:
                    ResettingBonesBehaviour();
                    break;
            }
        }

        void OnDrawGizmosSelected() 
        {
            if (!showCallRadius) {
                return;
            }

            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, callOthersRadius);
        }

        #endregion

        #region BEHAVIOUR METHODS

        public virtual void HitBehaviour()
        {
            // check if a hit was registered
            if (InboraStudio.hitProps.hitRegister) 
            {
                InboraStudio.hitProps.hitRegister = false;
                int chosenHitIndex = -1;

                if (hitAnims.Count > 0) 
                {
                    chosenHitIndex = Random.Range(0, hitAnims.Count);
                    hitDuration = hitAnims[chosenHitIndex].duration;
                }
                else {
                    Debug.LogWarning("No hit animations added.");
                }
                

                if (_duration == 0) {
                    if (chosenHitIndex > -1) InboraStudio.animManager.Play(hitAnims[chosenHitIndex].animName, hitAnimT, true);
                }
                else 
                {
                    if (_gapTimer >= hitAnimGap) 
                    {
                        if (chosenHitIndex > -1) InboraStudio.animManager.Play(hitAnims[chosenHitIndex].animName, hitAnimT, true);
                        _gapTimer = 0;
                    }
                }
                
                _duration = 0;

                // play hit audio
                PlayAudio();
            }

            _gapTimer += Time.deltaTime;

            // hit duration timer
            _duration += Time.deltaTime;

            if (_duration >= hitDuration) {
                FinishHitState();
            }
        }
        
        // exit hit state and turn to either alert or attack state
        public virtual void FinishHitState()   
        {
            ResetTimers();
            ragdollState = RagdollState.None;

            
            // if AI was in cover -> return to cover state
            if (InboraStudio.hitProps.hitWhileInCover && InboraStudio.coverShooterMode) {
                InboraStudio.SetState(InboraStudioAI.State.goingToCover);
                return;
            }


            if (InboraStudio.isFleeing) {
                InboraStudio.SetState(InboraStudioAI.State.attack);
                return;
            }


            // if the enemy that did the hit is passed -> set AI to go to enemy location
            if (InboraStudio.hitProps.hitEnemy) {
                // check the passed enemy isn't the same AI
                if (InboraStudio.hitProps.hitEnemy.transform.IsChildOf(transform)) {
                    InboraStudio.ChangeState("alert");
                    return;
                }
                
                InboraStudio.SetEnemy(InboraStudio.hitProps.hitEnemy);
                return;
            }

            
            // if an enemy is already targeted -> go to attack state
            if (InboraStudio.enemyToAttack) {
                InboraStudio.SetState(InboraStudioAI.State.attack);
                return;
            }


            // if nothing -> turn alert
            InboraStudio.ChangeState("alert");
        }

        // play the hit audio
        void PlayAudio()
        {
            if (!playAudio) {
                return;
            }
            
            if (playedAudio) {
                return;
            }

            if (InboraStudio.IsAudioScriptableEmpty()) {
                return;
            }

            if (!alwaysPlayAudio) {
                int rand = Random.Range(0, 2);
                if (rand == 0) {
                    return;
                }
            }

            if (InboraStudio.PlayAudio(InboraStudio.audioScriptable.GetAudio(AudioScriptable.AudioType.Hit))) {
                playedAudio = true;
            }
        }

        // call others
        public virtual void CallOthers()
        {
            System.Array.Clear(callOthersHitArr, 0, 20);
            int agentsCollNum = Physics.OverlapSphereNonAlloc(transform.position + InboraStudio.vision.visionPosition, callOthersRadius, callOthersHitArr, agentLayersToCall);
        
            for (int i=0; i<agentsCollNum; i++) 
            {
                InboraStudioAI script = callOthersHitArr[i].GetComponent<InboraStudioAI>();

                // if caught collider is that of the same AI -> skip
                if (transform.IsChildOf(callOthersHitArr[i].transform)) {
                    continue;
                }

                // if the caught collider is actually the current AI's enemy (AI vs AI) -> skip
                if (InboraStudio.enemyToAttack != null) {
                    if (InboraStudio.enemyToAttack.transform.IsChildOf(callOthersHitArr[i].transform)) {
                        continue;
                    }
                }

                // if script doesn't exist -> skip
                if (script == null) {
                    continue;
                }

                // reaching this point means current item is a valid AI

                if (InboraStudio.hitProps.hitEnemy) {
                    script.SetEnemy(InboraStudio.hitProps.hitEnemy, true, true);
                    continue;
                }

                // if no enemy has been passed
                // make it a random point within the destination
                Vector3 randomPoint = script.RandomSpherePoint(transform.position);
                
                script.ChangeState("alert");
                script.MoveToLocation(randomPoint);
            }
        }

        // reset the timers of hit duration
        void ResetTimers()
        {
            _duration = 0;
            _gapTimer = 0;
            elapsedResetBonesTime = 0;
            standingUpTimer = 0;
            playedAudio = false;
        }

        #endregion
    
        #region RAGDOLL

        public virtual void TriggerRagdoll()
        {
            lastRegisteredKnockOuts = InboraStudio.hitProps.knockOutRegister;

            elapsedResetBonesTime = 0;
            _duration = 0;
            standingUpTimer = 0;
            
            InboraStudio.animManager.ResetLastState();
            PlayAudio();
            InboraStudio.EnableRagdoll();
            
            if (!useNaturalVelocity) {
                if (hipBoneRb != null) {
                    Vector3 dir = transform.TransformDirection(knockOutForce);
                    hipBoneRb.AddForce(dir, ForceMode.Impulse);
                }
                else {
                    InboraStudio.PrintWarning(InboraStudio.warnAnomaly, "Hip Bone property in hit state behaviour hasn't been set. No force can be applied to the ragdoll unless you set this.");
                }   
            }

            if (InboraStudio.hitProps.callOthers) {
                CallOthers();
            }
            
            ragdollState = RagdollState.Ragdoll;
        }

        public virtual void RagdollBehaviour()
        {
            if (!IsRagdollSleeping()) {
                return;
            }

            _duration += Time.deltaTime;

            if (_duration < knockOutDuration) {
                return;
            }

            AlignRotationToHips();
            AlignPositionToHips();
            PopulateBoneTransforms(ragdollBoneTransforms);

            elapsedResetBonesTime = 0;
            _duration = 0;
            ragdollState = RagdollState.ResettingBones;
        }

        public virtual bool IsRagdollSleeping()
        {
            List<Collider> colls = InboraStudio.GetRagdollColliders();
            int sleepingRB = 0;

            foreach (Collider c in colls) {
                if (Mathf.Abs(c.attachedRigidbody.velocity.x) <= 0.3f && Mathf.Abs(c.attachedRigidbody.velocity.y) <= 0.3f && Mathf.Abs(c.attachedRigidbody.velocity.z) <= 0.3f) {
                    sleepingRB++;
                }
            }

            if (colls.Count > 1) {
                // if half of the rigidbodies are rested then consider ragdoll has finished moving
                if (sleepingRB >= colls.Count / 2) {
                    return true;
                }

                return false;
            }
            
            if (colls.Count == sleepingRB) {
                return true;
            }

            return false;
        }
        
        void StandingUpBehaviour()
        {
            standingUpTimer += Time.deltaTime;
            if (standingUpTimer >= InboraStudio.anim.GetCurrentAnimatorStateInfo(0).length) {
                standingUpTimer = 0;
                FinishHitState();
            }
        }

        void ResettingBonesBehaviour()
        {   
            getUpYonBul = true;
            
            if (getUpYonBul)
            {
                facePos = Vector3.Dot(hipBone.forward, Vector3.up);

                if (facePos > 0) {
                    currGetUpAnim = faceUpStandClipName;
                }
                else {
                    currGetUpAnim = faceDownStandClipName;
                }

                PopulateAnimationStartBoneTransforms(currGetUpAnim, standUpBoneTransforms);
                getUpYonBul = false;
            }
            
            elapsedResetBonesTime += Time.deltaTime;
            float elapsedPercentage = elapsedResetBonesTime / ragdollToStandSpeed;
            
            for (int boneIndex = 0; boneIndex < bones.Length; boneIndex++)
            {
                bones[boneIndex].localPosition = Vector3.Lerp(
                    ragdollBoneTransforms[boneIndex].Position,
                    standUpBoneTransforms[boneIndex].Position,
                    elapsedPercentage
                );


                if (standUpBoneTransforms[boneIndex].Rotation.eulerAngles == Vector3.zero) continue;

                bones[boneIndex].localRotation = Quaternion.Lerp(
                    ragdollBoneTransforms[boneIndex].Rotation,
                    standUpBoneTransforms[boneIndex].Rotation,
                    elapsedPercentage
                );
            }

            if (elapsedPercentage >= 1)
            {
                InboraStudio.navmeshAgent.enabled = true;
                InboraStudio.DisableRagdoll();
                InboraStudio.anim.Play(currGetUpAnim, 0, 0);
                ragdollState = RagdollState.StandingUp;
            }
        }

        void AlignRotationToHips()
        {
            facePos = Vector3.Dot(hipBone.forward, Vector3.up);

            Vector3 originalHipsPosition = hipBone.position;
            Quaternion originalHipsRotation = hipBone.rotation;
            Vector3 desiredDirection = Vector3.zero;

            if (facePos > 0)
            {
                desiredDirection = hipBone.up * -1;
                desiredDirection.y = 0;

                desiredDirection.Normalize();

                Quaternion fromToRotation = Quaternion.FromToRotation(transform.forward, desiredDirection);
                transform.rotation *= fromToRotation;
            }

            hipBone.position = originalHipsPosition;
            hipBone.rotation = originalHipsRotation;
        }

        void AlignPositionToHips()
        {
            Vector3 originalHipsPosition = hipBone.position;
            transform.position = hipBone.position;

            Vector3 positionOffset = standUpBoneTransforms[0].Position;
            positionOffset.y = 0;
            positionOffset = transform.rotation * positionOffset;

            transform.position -= positionOffset;

            if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hitInfo, InboraStudio.groundLayers))
            {
                transform.position = new Vector3(transform.position.x, hitInfo.point.y, transform.position.z);
            }

            hipBone.position = originalHipsPosition;
        }

        void PopulateBoneTransforms(BoneTransform[] boneTransforms)
        {
            for (int boneIndex = 0; boneIndex < bones.Length; boneIndex++)
            {
                boneTransforms[boneIndex].Position = bones[boneIndex].localPosition;
                boneTransforms[boneIndex].Rotation = bones[boneIndex].localRotation;
            }
        }

        void PopulateAnimationStartBoneTransforms(string clipName, BoneTransform[] boneTransforms)
        {
            Vector3 positionBeforeSampling = transform.position;
            Quaternion rotationBeforeSampling = transform.rotation;

            foreach (AnimationClip clip in InboraStudio.anim.runtimeAnimatorController.animationClips)
            {
                if (clip.name == clipName)
                {
                    clip.SampleAnimation(gameObject, 0);
                    PopulateBoneTransforms(standUpBoneTransforms);
                    break;
                }
            }

            transform.position = positionBeforeSampling;
            transform.rotation = rotationBeforeSampling;
        }

        void PrepareBones()
        {
            bones = InboraStudio.GetRagdollTransforms();
            standUpBoneTransforms = new BoneTransform[bones.Length];
            ragdollBoneTransforms = new BoneTransform[bones.Length];

            for (int boneIndex = 0; boneIndex < bones.Length; boneIndex++) {
                standUpBoneTransforms[boneIndex] = new BoneTransform();
                ragdollBoneTransforms[boneIndex] = new BoneTransform();
            }
        }
        
        #endregion
    }
}