using UnityEngine;
using UnityEngine.Events;

namespace InboraStudioAISpace
{
    [AddComponentMenu("InboraStudio AI/Surprised State Behaviour")]
    public class SurprisedStateBehaviour : InboraStudioBehaviour
    {
        #region PROPERTIES

        [Tooltip("The surprised animation to play.")]
        public string anim;
        [Tooltip("The animation transition.")]
        public float animT = 0.25f;
        [Min(0), Tooltip("The duration to stay in this state and playing the animation.")]
        public float duration;

        [Tooltip("The speed of turning to face the target's caught position.")]
        public float turnSpeed = 10;

        [Tooltip("Set your audios in the audio scriptable in the General Tab in InboraStudio AI.")]
        public bool playAudio;

        public UnityEvent onStateEnter;
        public UnityEvent onStateExit;

        #endregion

        #region BEHAVIOUR VARS

        public InboraStudioAI InboraStudio { get; private set; }
        bool isFirstRun = true;
        float _duration = 0f;
        bool playedAudio;
        bool turningDone;

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
            }
        }

        public override void Close()
        {
            Reset();
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
            // only turn if turning hasn't finished
            if (!turningDone) 
            {
                // turn to face enemy -> this function returns true when done
                if (InboraStudio.TurnTo(InboraStudio.enemyPosOnSurprised, InboraStudio.waypoints.leftTurnAnimAlert, InboraStudio.waypoints.rightTurnAnimAlert, InboraStudio.waypoints.turningAnimT, turnSpeed)) {
                    turningDone = true;
                }

                return;
            }
            
            // play animation
            InboraStudio.animManager.Play(anim, animT);
            
            
            // play audio
            if (playAudio && !playedAudio) 
            {
                if (!InboraStudio.IsAudioScriptableEmpty()) {
                    if (InboraStudio.PlayAudio(InboraStudio.audioScriptable.GetAudio(AudioScriptable.AudioType.SurprisedState))) {
                        playedAudio = true;
                    }
                }
            }

            // timer to quit surprised state
            _duration += Time.deltaTime;
            
            if (_duration >= duration) 
            {
                Reset();
                InboraStudio.SetState(InboraStudioAI.State.attack);
            }
        }

        void Reset()
        {
            turningDone = false;
            _duration = 0f;
            playedAudio = false;
        }

        #endregion
    }
}