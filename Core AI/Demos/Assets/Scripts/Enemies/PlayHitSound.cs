using UnityEngine;

namespace InboraStudioAIDemo
{
    public class PlayHitSound : MonoBehaviour
    {
        public AudioSource hitSound;
        public FlagAttack flagAttack;

        void OnTriggerEnter(Collider col)
        {
            if (col.CompareTag("Player") && flagAttack.attacking) {
                if (!hitSound.isPlaying) hitSound.Play();
            }
        }
    }
}

