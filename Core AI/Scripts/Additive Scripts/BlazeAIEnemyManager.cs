using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("InboraStudio AI/Additive Scripts/InboraStudio AI Enemy Manager")]
public class InboraStudioAIEnemyManager : MonoBehaviour
{
    [Tooltip("The amount of time in seconds to send an enemy to attack.")]
    public float attackTimer = 5f;
    [Tooltip("Setting this to false won't let enemies attack instead they'll just be in attack idle state.")]
    public bool callEnemies = true;

    #region SYSTEM VARIABLES

    float _timer = 0f;
    Transform lastSelectedAI;
    int attackDir = -1;

    public List<InboraStudioAI> targetedBy = new List<InboraStudioAI>();
    public List<InboraStudioAI> potentialEnemies = new List<InboraStudioAI>();
    List<InboraStudioAI> enemiesScheduled = new List<InboraStudioAI>();

    #endregion
    
    #region UNITY METHODS

    void Awake()
    {
        enabled = false;
    }

    public virtual void Update ()
    {
        if (enemiesScheduled.Count == 0) {
            lastSelectedAI = null;
            return;
        }

        // run the timer
        _timer += Time.deltaTime;
        if (_timer < attackTimer) return;
        _timer = 0f;


        // remove unwanted AIs
        for (int i=0; i<enemiesScheduled.Count; i+=1) {
            // AI doesn't have a target
            if (enemiesScheduled[i].enemyToAttack == null) {
                RemoveEnemy(enemiesScheduled[i]);
                continue;
            }

            // AI has a target but it's not this transform or it's children
            if (!enemiesScheduled[i].enemyToAttack.transform.IsChildOf(transform)) {
                RemoveEnemy(enemiesScheduled[i]);
                continue;
            }

            // if AI is dead
            if (enemiesScheduled[i].state == InboraStudioAI.State.death) {
                RemoveEnemy(enemiesScheduled[i]);
                continue;
            }
        }
        

        // randomize enemies list and choose one to attack
        if (callEnemies && enemiesScheduled.Count > 0) 
        {
            // if only 1 AI then there's no need for further logic -> just make that AI attack
            if (enemiesScheduled.Count == 1) {
                if (enemiesScheduled[0].enemyToAttack != null) {
                    enemiesScheduled[0].Attack();
                    lastSelectedAI = enemiesScheduled[0].transform;
                }

                return;
            }

            if (attackDir + 1 > 2) attackDir = 0;
            else attackDir++;

            if (attackDir == 0) {
                FrontAttack();
                return;
            }

            if (attackDir == 1) {
                BackAttack();
                return;
            }
            
            // if attackDir == 2 -> then shuffle and choose a random AI that's not the last selected one
            Shuffle();
            
            // loop the AIs and make any AI with target to attack
            for (int i=0; i<enemiesScheduled.Count; i++) {
                InboraStudioAI selectedAI = enemiesScheduled[i];

                if (enemiesScheduled.Count > 1) {
                    if (selectedAI.enemyToAttack != null && selectedAI.transform != lastSelectedAI) {
                        selectedAI.Attack();
                        lastSelectedAI = selectedAI.transform;
                        break;
                    }
                }
            }
        }
    }

    #endregion

    #region SYSTEM METHODS

    // add an enemy to the list the manager is targeted by
    public virtual void AddTarget(InboraStudioAI enemy)
    {
        if (targetedBy.IndexOf(enemy) >= 0) return;
        targetedBy.Add(enemy);
    }

    // add an enemy to the list of AIs using scheduled attacks -> the AI isn't set to Interval attacks
    public virtual void AddScheduledEnemy(InboraStudioAI enemy)
    {
        enabled = true;
        if (enemiesScheduled.IndexOf(enemy) >= 0) return;
        AddTarget(enemy);
        enemiesScheduled.Add(enemy);
    }

    // add potential enemy that's detecting this manager through vision meter
    public virtual void AddPotentialEnemy(InboraStudioAI enemy)
    {
        if (potentialEnemies.IndexOf(enemy) >= 0) return;
        potentialEnemies.Add(enemy);
    }

    // remove a specific enemy from the list
    public virtual void RemoveEnemy(InboraStudioAI enemy)
    {
        if (targetedBy.IndexOf(enemy) >= 0) {
            targetedBy.Remove(enemy);
        }

        if (enemiesScheduled.IndexOf(enemy) >= 0) { 
            enemiesScheduled.Remove(enemy);
        }

        if (potentialEnemies.IndexOf(enemy) >= 0) {
            potentialEnemies.Remove(enemy);
        }

        if (enemiesScheduled.Count == 0) enabled = false;
    }

    public virtual void FrontAttack()
    {
        float frontMostDir = -1;
        InboraStudioAI bestSelectedAI = null;

        for (int i=0; i<enemiesScheduled.Count; i++) {
            InboraStudioAI enemyItem = enemiesScheduled[i];
            Vector3 directionToAI = Vector3.Normalize(enemyItem.transform.position - transform.position);
            float dot = Vector3.Dot(transform.forward, directionToAI);

            if (dot >= frontMostDir && lastSelectedAI != enemyItem.transform) {
                frontMostDir = dot;
                bestSelectedAI = enemyItem;
                continue;
            }
        }

        bestSelectedAI.Attack();
        lastSelectedAI = bestSelectedAI.transform;
    }

    public virtual void BackAttack()
    {
        float backMostDir = 1;
        InboraStudioAI bestSelectedAI = null;

        for (int i=0; i<enemiesScheduled.Count; i++) {
            InboraStudioAI enemyItem = enemiesScheduled[i];
            Vector3 directionToAI = Vector3.Normalize(enemyItem.transform.position - transform.position);
            float dot = Vector3.Dot(transform.forward, directionToAI);

            if (dot <= backMostDir && lastSelectedAI != enemyItem.transform) {
                backMostDir = dot;
                bestSelectedAI = enemyItem;
                continue;
            }
        }

        bestSelectedAI.Attack();
        lastSelectedAI = bestSelectedAI.transform;
    }

    // shuffle the enemiesScheduled list to choose a random one
    public virtual void Shuffle()
    {
        var count = enemiesScheduled.Count;
        var last = count - 1;

        for (var i = 0; i < last; ++i) {
            var r = Random.Range(i, count);
            var tmp = enemiesScheduled[i];

            enemiesScheduled[i] = enemiesScheduled[r];
            enemiesScheduled[r] = tmp;
        }
    }

    #endregion
}