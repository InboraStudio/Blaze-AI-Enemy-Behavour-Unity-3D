using UnityEngine;
using InboraStudioAISpace;
using InboraStudioAIDemo;
using System.Collections.Generic;

public class DeathDemoScript : MonoBehaviour
{
    public InboraStudioAI[] InboraStudioAI;
    public List<Vector3> startPositions = new List<Vector3>();

    void Start()
    {
        foreach (var item in InboraStudioAI) {
            startPositions.Add(item.transform.position);
        }
    }

    void Update()
    {
        // hit the AI
        if (Input.GetKeyDown(KeyCode.E)) 
        {
            for (int i=0; i<InboraStudioAI.Length; i++) {
                InboraStudioAI[i].Hit();
                InboraStudioAIDemo.Health InboraStudioHealth = InboraStudioAI[i].GetComponent<InboraStudioAIDemo.Health>();

                if (InboraStudioHealth.currentHealth > 0) {
                    InboraStudioHealth.currentHealth -= 10;
                }

                if (InboraStudioHealth.currentHealth <= 0) {
                    if (i < 2) {
                        // plays either death animation or ragdolls instantly depending on inspector
                        InboraStudioAI[i].Death();
                    }
                    else {
                        // plays death animation and then ragdolls midway
                        InboraStudioAI[i].DeathDoll(0.5f);
                    }
                }
            }
        }


        // return alive
        if (Input.GetKeyDown(KeyCode.R)) 
        {
            for (int i=0; i<InboraStudioAI.Length; i++) {
                InboraStudioAIDemo.Health InboraStudioHealth = InboraStudioAI[i].GetComponent<InboraStudioAIDemo.Health>();
                InboraStudioHealth.currentHealth = InboraStudioHealth.maxHealth;
                InboraStudioAI[i].ChangeState("normal");
                InboraStudioAI[i].transform.position = startPositions[i];
            }
        }
    }
}
