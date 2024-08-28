using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeState : MonoBehaviour
{
    InboraStudioAI InboraStudio;

    // Start is called before the first frame update
    void Start()
    {
        InboraStudio = GetComponent<InboraStudioAI>();    
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E)) {
            InboraStudio.ChangeState("normal");
        }

        if (Input.GetKeyDown(KeyCode.R)) {
            InboraStudio.ChangeState("alert");
        }
    }
}
