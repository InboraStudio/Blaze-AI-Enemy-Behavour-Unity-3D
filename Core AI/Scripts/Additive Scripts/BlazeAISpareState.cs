using UnityEngine;
using InboraStudioAISpace;

[AddComponentMenu("InboraStudio AI/Additive Scripts/InboraStudio AI Spare State")]
public class InboraStudioAISpareState : MonoBehaviour
{
    public SpareState[] spareStates;
    [HideInInspector] public InboraStudioAI InboraStudio;
    public SpareState chosenState { private set; get; }

    #region SYSTEM VARIABLES

    bool isExitOnTimer = false;
    float stateTimer = 0;
    InboraStudioAI.State previousInboraStudioState;

    #endregion

    #region UNITY METHODS

    void OnValidate()
    {
        ValidateStateNames();
    }

    #endregion

    #region SYSTEM METHODS

    // set the state
    public void SetState(string stateName, int animIndex = -1, int audioIndex = -1)
    {
        int max = spareStates.Length;
        for (int i=0; i<max; i++) {
            SpareState state = spareStates[i];

            if (state.stateName == stateName) {
                TriggerState(state, animIndex, audioIndex);
                break;
            }
        }
    }

    public virtual void TriggerState(SpareState state, int passedAnimIndex = -1, int passedAudioIndex = -1)
    {
        // set the previous state of InboraStudio to exit to
        if (InboraStudio.state != InboraStudioAI.State.spareState) 
        {
            if (InboraStudio.state == InboraStudioAI.State.distracted) {
                previousInboraStudioState = InboraStudio.previousState;
            }
            else if (InboraStudio.state == InboraStudioAI.State.hit) {
                if (InboraStudio.enemyToAttack) previousInboraStudioState = InboraStudioAI.State.attack;
                else previousInboraStudioState = InboraStudioAI.State.alert;
            }
            else {
                previousInboraStudioState = InboraStudio.state;
            }
        }

        InboraStudio.SetState(InboraStudioAI.State.spareState);
        state.enterEvent.Invoke();
        
        // choose & play animation
        PlayAnimation(state, passedAnimIndex);
        
        // choose & play audio
        if (state.playAudio) {
            PlayAudio(state, passedAudioIndex);
        }

        stateTimer = 0;
        chosenState = state;
        
        if (state.exitMethod == SpareState.ExitMethod.ExitAfterTime) {
            isExitOnTimer = true;
            return;
        }

        isExitOnTimer = false;
    }

    public virtual void ExitState()
    {
        isExitOnTimer = false;
        stateTimer = 0;
        InboraStudio.animManager.ResetLastState();
        InboraStudio.SetState(previousInboraStudioState);
        chosenState.exitEvent.Invoke();
    }

    public void StateTimer()
    {
        if (!isExitOnTimer) return;
        
        stateTimer += Time.deltaTime;
        if (stateTimer >= chosenState.exitTimer) {
            ExitState();
        }
    }

    void ValidateStateNames()
    {
        if (spareStates == null) return;
        
        int max = spareStates.Length;
        for (int i=0; i<max; i++) {
            // remove start and end spaces from state names
            string name = spareStates[i].stateName;
            spareStates[i].stateName = name.Trim();
        }
    }

    void PlayAnimation(SpareState state, int passedAnimIndex)
    {
        if (state.animsToPlay == null) return; 
        if (state.animsToPlay.Length == 0) return;
            
        string animName = "";
        
        if (passedAnimIndex < 0) {
            int randAnimIndex = Random.Range(0, state.animsToPlay.Length);
            animName = state.animsToPlay[randAnimIndex].Trim();
        }
        else {
            if (passedAnimIndex <= state.animsToPlay.Length - 1) {
                animName = state.animsToPlay[passedAnimIndex].Trim();
            }
        }

        if (animName.Length > 0) {
            InboraStudio.animManager.Play(animName, state.animT);
            return;
        }
    }

    void PlayAudio(SpareState state, int passedAudioIndex)
    {
        if (state.audiosToPlay == null) return;
        if (state.audiosToPlay.Length == 0) return;

        AudioClip chosenAudio = null;

        if (passedAudioIndex < 0) {
            int audioIndex = Random.Range(0, state.audiosToPlay.Length);
            chosenAudio = state.audiosToPlay[audioIndex];
        }
        else {
            if (passedAudioIndex <= state.audiosToPlay.Length - 1) {
                chosenAudio = state.audiosToPlay[passedAudioIndex];
            }
        }
        
        if (chosenAudio != null) {
            InboraStudio.agentAudio.clip = chosenAudio;
            InboraStudio.agentAudio.Play();
        }
    }

    #endregion
}