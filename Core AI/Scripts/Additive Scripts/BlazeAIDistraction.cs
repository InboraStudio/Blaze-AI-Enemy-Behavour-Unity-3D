using UnityEngine;
using System.Collections.Generic;

[AddComponentMenu("InboraStudio AI/Additive Scripts/InboraStudio AI Distraction")]
public class InboraStudioAIDistraction : MonoBehaviour 
{
    #region PROPERTIES

    [Tooltip("Automatically trigger the distraction when the GameObject is created. Useful for explosions and similar distractions.")]
    public bool distractOnAwake;
    [Tooltip("The layers of the InboraStudio AI agents.")]
    public LayerMask agentLayers = Physics.AllLayers;
    [Min(0), Tooltip("The radius of the distraction.")]
    public float distractionRadius;
    [Tooltip("Do you want the distraction to pass through obstacles with colliders?")]
    public bool passThroughColliders;
    [Tooltip("If turned off and a distraction is triggered, all agents within the radius will get distracted and turn to look at the distraction. If turned on, only the chosen agent with the highest priority will get distracted.")]
    public bool distractOnlyPrioritizedAgent;

    #endregion
    
    #region GARBAGE REDUCTION
    
    List<InboraStudioAI> enemiesList = new List<InboraStudioAI>();
    
    #endregion

    #region UNITY METHODS

    public virtual void Start()
    {
        if (distractOnAwake) {
            TriggerDistraction();
        }
    }

    // show distraction radius
    void OnDrawGizmosSelected()
    {
        // Draw a yellow sphere at the transform's position
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, distractionRadius);
    }

    #endregion
    
    #region SYSTEM METHODS

    // public method for triggering the distractions
    public virtual void TriggerDistraction() 
    {
        // get the surrounding agents
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, distractionRadius, agentLayers);
        enemiesList.Clear();

        // agents may have more several colliders and each one returns the same script
        // add only unique agents
        foreach (var hit in hitColliders) 
        {
            var script = hit.GetComponent<InboraStudioAI>();

            if (script != null) {
                if (!enemiesList.Contains(script)) {
                    enemiesList.Add(script);
                }
            }
        }

        // now loop the enemy list and move to location the one with the highest priority level
        if (enemiesList.Count > 0) 
        {    
            // sort the enemies according to priority values
            enemiesList.Sort((a, b) => { return a.priorityLevel.CompareTo(b.priorityLevel); });

            if (distractOnlyPrioritizedAgent) {
                // distract the highest priority only
                int highestPriorityIndex = enemiesList.Count - 1;

                if (CheckIfReaches(enemiesList[highestPriorityIndex].transform)) {
                    enemiesList[highestPriorityIndex].Distract(transform.position);
                }

                return;
            }
            
            int max = enemiesList.Count;
            for (int i=0; i<max; i++) 
            {
                if (i == 0) {
                    // play audio on one agent
                    if (CheckIfReaches(enemiesList[i].transform)) {
                        enemiesList[i].Distract(transform.position);
                    }

                    continue;
                }
                
                if (CheckIfReaches(enemiesList[i].transform)) {
                    enemiesList[i].Distract(transform.position, false);
                }
            }
        }
    }

    // checks if distraction will reach agent through colliders
    public virtual bool CheckIfReaches(Transform enemy)
    {
        if (passThroughColliders) return true;

        RaycastHit hit;
        Collider coll = enemy.GetComponent<Collider>();
        Vector3 enemyCenter = coll.ClosestPoint(coll.bounds.center);

        Collider currentCol = gameObject.GetComponent<Collider>();
        Vector3 currentColCenter = currentCol.ClosestPoint(currentCol.bounds.center);
        Vector3 dir = (enemyCenter - currentColCenter);

        float distance = Vector3.Distance(enemyCenter, currentColCenter) + 5;

        if (Physics.Raycast(currentColCenter, dir, out hit, distance, Physics.AllLayers)) {
            if (hit.transform.IsChildOf(enemy) || enemy.IsChildOf(hit.transform)) {
                return true;
            }
            
            return false;
        }

        return false;
    }

    #endregion
}