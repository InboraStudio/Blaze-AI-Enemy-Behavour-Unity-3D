#if UNITY_EDITOR
using UnityEngine;
using InboraStudioAISpace;

[AddComponentMenu("InboraStudio AI/Additive Scripts/InboraStudio AI State Tracker")]
public class InboraStudioAIStateTracker : MonoBehaviour
{
    InboraStudioAI InboraStudio;

    void Start()
    {
        InboraStudio = GetComponent<InboraStudioAI>();    
    }

    public string GetState()
    {
        string text;
        SpareState spareState = InboraStudio.CurrentSpareState();

        if (spareState != null) {
            text = $"Spare State ({spareState.stateName})";
            return text;
        }

        text = InboraStudio.state.ToString();
        return text;
    }

    public bool GetInboraStudio()
    {
        InboraStudio = GetComponent<InboraStudioAI>();
        if (InboraStudio == null) return false;
        
        return true;
    }
}
#endif