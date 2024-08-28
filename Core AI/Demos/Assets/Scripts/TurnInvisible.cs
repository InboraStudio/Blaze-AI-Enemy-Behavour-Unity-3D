using UnityEngine;

namespace InboraStudioAIDemo
{
    public class TurnInvisible : MonoBehaviour
    {
        public GameObject body;
        public Material invisibleMat;
        public AudioSource invisibleAudio;
        public AudioSource returnAudio;
        public bool invisibleOnStart = false;

        bool state = false;
        string defaultTag;
        Material defaultMat;

        void Start()
        {
            if (invisibleOnStart) Invisible();
        }

        // Update is called once per frame
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space)) {
                if (state) {
                    Appear();
                }else{
                    Invisible();
                }
            }
        }

        void Invisible()
        {
            //turn invisible
            state = true;
            defaultTag = transform.tag;
            defaultMat = body.GetComponent<Renderer>().material;
            body.GetComponent<Renderer>().material = invisibleMat;
            transform.tag = "Untagged";
            if (!invisibleAudio.isPlaying) invisibleAudio.Play();
        }

        void Appear()
        {
            //return normal
            state = false;
            body.GetComponent<Renderer>().material = defaultMat;
            transform.tag = defaultTag;
            if (!returnAudio.isPlaying) returnAudio.Play();
        }
    }
}
