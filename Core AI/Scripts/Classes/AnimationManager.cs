using UnityEngine;

namespace InboraStudioAISpace
{
    public class AnimationManager
    {
        Animator anim;
        InboraStudioAI InboraStudio;
        string currentState;

        // constructor
        public AnimationManager (Animator animator, InboraStudioAI InboraStudioAI)
        {
            anim = animator;
            InboraStudio = InboraStudioAI;
        }

        void GetAnimNameAndLayer(string name, out string animName, out int layer)
        {
            layer = 0;
            animName = name;
            var animParts = animName.Split('.');
            
            if (animParts.Length > 1)
            {
                if (!System.Int32.TryParse(animParts[0], out layer)) {
                    Debug.LogWarning("Failed to Parse Layer given animation " + name);
                }
                else
                {
                    animName = animParts[1];
                }
            }
        }

        // actual animation playing function
        public virtual void Play(string state, float time = 0.25f, bool overplay = false)
        {   
            if (state == currentState) return;

            
            // check if passed animation name doesn't exist in the Animator
            if (!CheckAnimExists(state)) 
            {
                if (string.IsNullOrEmpty(state)) {
                    anim.enabled = false;
                    return;
                }
            
                anim.enabled = true;
                return;
            }

            
            anim.enabled = true;

            string animName;
            int layer;
            GetAnimNameAndLayer(state, out animName, out layer);
            
            anim.CrossFadeInFixedTime(animName, time, layer);


            if (overplay) currentState = "";
            else currentState = state;
        }

        // check whether the passed animation name exists or not
        public bool CheckAnimExists(string stateName)
        {
            if (string.IsNullOrEmpty(stateName)) 
            {
                #if UNITY_EDITOR
                if (InboraStudio.warnEmptyAnimations) {
                    Debug.LogWarning("Empty animation.");
                }
                #endif

                return false;
            }

            string animName;
            int layer;
            GetAnimNameAndLayer(stateName, out animName, out layer);
            bool animCheck = anim.HasState(layer, Animator.StringToHash(animName));

            if (!animCheck) 
            {
                #if UNITY_EDITOR
                if (InboraStudio.warnEmptyAnimations) {
                    Debug.LogWarning($"The animation name: {stateName} - doesn't exist and has been ignored. Please re-check your animation names.");
                }
                #endif

                return false;
            }

            return animCheck;
        }

        public void ResetLastState()
        {
            currentState = "";
        }
    }
}