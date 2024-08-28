using UnityEngine;
using UnityEngine.Events;

namespace InboraStudioAISpace
{
    [System.Serializable]
    public class SpareState
    {
        [Header("Set State Name"), Tooltip("Give this state a unique name. Using this name you can call the state.")]
        public string stateName;
        
        [Space(5)]

        [Header("Optional Animations To Play"), Tooltip("A random animation will be selected from the list to play on calling this state.")]
        public string[] animsToPlay;
        [Min(0), Tooltip("The animation transition from current animation to this state animation. a good value would be: 0.25")]
        public float animT = 0.25f;

        [Space(5)]

        [Header("Audio"), Tooltip("Set whether you want an audio to play.")]
        public bool playAudio;
        [Tooltip("A random audio will be selected from the list to play on calling this state.")]
        public AudioClip[] audiosToPlay;

        [Space(5)]

        [Header("Type of Exit Method"), Tooltip("If [Exit After Time] is chosen then the Exit Timer property will be considered and automatically exit after the duration. If [Manual Exit] is chosen then you have to exit manually using the ExitSpareState() API.")]
        public ExitMethod exitMethod;
        [Min(0), Tooltip("The time to pass before exiting this spare state automatically. This is only considered if the Exit Method is set to [Exit After Time]")]
        public float exitTimer = 3;

        [Space(10)]

        [Header("State Events")]
        public UnityEvent enterEvent;
        public UnityEvent exitEvent;

        public enum ExitMethod {
            ExitAfterTime,
            ManualExit
        }
    }
}