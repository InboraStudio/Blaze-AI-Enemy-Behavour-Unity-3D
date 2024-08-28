using UnityEngine;

namespace InboraStudioAISpace
{
    public class StayInPosition : MonoBehaviour
    {
        InboraStudioAI InboraStudio;

        void Start()
        {
            InboraStudio = GetComponent<InboraStudioAI>();
        }
        
        void Update()
        {
            if (InboraStudio.state == InboraStudioAI.State.normal || InboraStudio.state == InboraStudioAI.State.alert) {
                InboraStudio.StayIdle();
            }
        }
    }
}