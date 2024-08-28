using UnityEngine;

namespace InboraStudioAIDemo
{
    public class ClickToDistract : MonoBehaviour
    {
        public AudioSource distractionAudio;
        InboraStudioAIDistraction distractionScript;
        
        void Start() {
            distractionScript = GetComponent<InboraStudioAIDistraction>();
        }

        // Update is called once per frame
        void Update()
        {
            if (Input.GetMouseButtonDown(0)){
                if (distractionAudio) distractionAudio.Play();
                distractionScript.TriggerDistraction();
            }
        }
    }
}
