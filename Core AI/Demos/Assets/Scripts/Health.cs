using UnityEngine;

namespace InboraStudioAIDemo
{
    public class Health : MonoBehaviour
    {
        public float maxHealth = 50;
        public float currentHealth { get; set; }


        void Start()
        {
            currentHealth = maxHealth;
        }
    }
}
