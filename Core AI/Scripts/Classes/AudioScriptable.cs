using UnityEngine;
using System.Collections.Generic;

namespace InboraStudioAISpace
{
    [CreateAssetMenu(fileName = "InboraStudioAudioScriptable", menuName = "InboraStudio AI/Audio Scriptable")]
    public class AudioScriptable : ScriptableObject {
        [Header("Normal state behaviour"), Tooltip("Audios to play when patrolling in normal state.")]
        public AudioClip[] normalState;


        [Space(10), Header("Alert State Behaviour")]
        [Tooltip("Audios to play when patrolling in alert state.")]
        public AudioClip[] alertState;
        [Tooltip("Audios to play when in alert state and returning to normal state.")]
        public AudioClip[] returningToNormalState;


        [Space(10), Header("Surprised State Behaviour")]
        [Tooltip("Audios to play on surprised state. (seeing enemy when in normal state)")]
        public AudioClip[] surprisedState;


        [Space(10), Header("Attack State Behaviour")]
        [Tooltip("Audios to play on attacking target.")]
        public AudioClip[] attacks;
        [Tooltip("Audios to play when AI is in attack state waiting for it's turn to attack the player.")]
        public AudioClip[] attackIdle;
        [Tooltip("Audios to play when AI is moving to attack distance.")]
        public AudioClip[] moveToAttack;


        [Space(10), Header("Attack Behaviour & Cover Shooter")]
        [Tooltip("Audios to play when AI is chasing target.")]
        public AudioClip[] chase;
        [Tooltip("Audios to play when in attack state and is returning to patrol in alert state.")]
        public AudioClip[] returnPatrol;


        [Space(10), Header("Cover Shooter & Going To Cover")]
        [Tooltip("Audios to play when AI is shooting. This isn't the firing audio of the gun. But, an audio to play while shooting, like screaming for example.")]
        public AudioClip[] duringShooting;
        [Tooltip("Audios to play when AI is getting out of cover and moving to shoot target.")]
        public AudioClip[] moveToShoot;
        [Tooltip("Audios to play when AI is going to target.")]
        public AudioClip[] goingToCover;


        [Space(10), Header("Distracted Behaviour")]
        [Tooltip("Audios to play on getting distracted.")]
        public AudioClip[] distracted;
        [Tooltip("Audios to play when checking distraction location.")]
        public AudioClip[] distractionCheckLocation;


        [Space(10), Header("Searching")]
        [Tooltip("Audios to play when beginning to search.")]
        public AudioClip[] searchStart;
        [Tooltip("Audios to play when search ends.")]
        public AudioClip[] searchEnd;


        [Space(10), Header("Hit & Death")]
        [Tooltip("Audios to play when hit.")]
        public AudioClip[] hit;
        [Tooltip("Audios to play on death.")]
        public AudioClip[] death;

        
        [Space(10), Header("Alert Tags")]
        [Tooltip("Audios to play when vision catches an alert tag game object.")]
        public AudioClip[] alertTags;


        [Space(10), Header("Fleeing")]
        [Tooltip("Audios to play when fleeing behaviour is active.")]
        public AudioClip[] fleeing;

        
        [Space(10), Header("Companion Mode")]
        [Tooltip("Audios to play when companion AI has reached the target distance and is either idle or wandering.")]
        public AudioClip[] companionIdleAndWander;
        [Tooltip("Audios to play when companion AI is commanded to follow.")]
        public AudioClip[] companionOnFollow;
        [Tooltip("Audios to play when companion AI is commanded to not follow.")]
        public AudioClip[] companionOnStop;


        public enum AudioType : int {
            NormalState = 0,
            AlertState,
            SurprisedState,
            Attacks,
            AttackIdle,
            MoveToAttack,
            Chase,
            DuringShooting,
            MoveToShoot,
            GoingToCover,
            ReturningToNormalState,
            Distracted,
            DistractionCheckLocation,
            SearchStart,
            SearchEnd,
            ReturnPatrol,
            Hit,
            Death,
            AlertTags,
            Fleeing,
            CompanionIdleAndWander,
            CompanionOnFollow,
            CompanionOnStop
        }
        
        [HideInInspector]
        public Dictionary<int, AudioClip[]> audios = new Dictionary<int, AudioClip[]>();
        

        public virtual void OnEnable()
        {
            audios.Clear();

            audios.Add((int)AudioType.NormalState, normalState);
            audios.Add((int)AudioType.AlertState, alertState);
            audios.Add((int)AudioType.SurprisedState, surprisedState);
            audios.Add((int)AudioType.Attacks, attacks);
            audios.Add((int)AudioType.AttackIdle, attackIdle);
            audios.Add((int)AudioType.MoveToAttack, moveToAttack);
            audios.Add((int)AudioType.Chase, chase);
            audios.Add((int)AudioType.DuringShooting, duringShooting);
            audios.Add((int)AudioType.MoveToShoot, moveToShoot);
            audios.Add((int)AudioType.GoingToCover, goingToCover);
            audios.Add((int)AudioType.ReturningToNormalState, returningToNormalState);
            audios.Add((int)AudioType.Distracted, distracted);
            audios.Add((int)AudioType.DistractionCheckLocation, distractionCheckLocation);
            audios.Add((int)AudioType.SearchStart, searchStart);
            audios.Add((int)AudioType.SearchEnd, searchEnd);
            audios.Add((int)AudioType.ReturnPatrol, returnPatrol);
            audios.Add((int)AudioType.Hit, hit);
            audios.Add((int)AudioType.Death, death);
            audios.Add((int)AudioType.AlertTags, alertTags);
            audios.Add((int)AudioType.Fleeing, fleeing);
            audios.Add((int)AudioType.CompanionIdleAndWander, companionIdleAndWander);
            audios.Add((int)AudioType.CompanionOnFollow, companionOnFollow);
            audios.Add((int)AudioType.CompanionOnStop, companionOnStop);
        }

        public virtual AudioClip GetAudio(AudioType type)
        {
            if (audios[(int)type].Length <= 0) {
                return null;
            }


            int randomSound = 0;
            randomSound = Random.Range(0, audios[(int)type].Length);

            
            if (audios[(int)type][randomSound] == null) {
                return null;
            }

            
            return audios[(int)type][randomSound];
        }

        public virtual AudioClip GetAudio(AudioType type, int index)
        {
            if (audios[(int)type].Length <= 0) {
                return null;
            }

            if (index > audios[(int)type].Length - 1) {
                return null;
            }

            if (audios[(int)type][index] == null) {
                return null;
            }

            return audios[(int)type][index];
        }

        public virtual AudioClip[] GetAudios(AudioType type)
        {
            if (audios[(int)type].Length <= 0) {
                return null;
            }
            
            return audios[(int)type];
        }
    }
}