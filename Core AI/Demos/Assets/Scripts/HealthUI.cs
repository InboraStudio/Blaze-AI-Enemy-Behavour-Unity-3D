using UnityEngine;
using TMPro;
using InboraStudioAIDemo;

public class HealthUI : MonoBehaviour
{
    public TextMeshProUGUI healthText;
    public InboraStudioAIDemo.Health InboraStudioHealth;
    

    // Update is called once per frame
    void Update()
    {
        healthText.text = "Health: " + InboraStudioHealth.currentHealth;
    }
}

