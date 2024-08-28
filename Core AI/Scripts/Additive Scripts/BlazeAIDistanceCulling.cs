using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InboraStudioAISpace;

[AddComponentMenu("InboraStudio AI/Additive Scripts/InboraStudio AI Distance Culling")]
public class InboraStudioAIDistanceCulling : MonoBehaviour
{
    [Tooltip("Automatically get the game camera.")]
    public bool autoCatchCamera = true;

    [Tooltip("The player or camera to calculate the distance between it and the AIs.")]
    public Transform cameraOrPlayer;

    [Min(0), Tooltip("If an AI distance is more than this set value then the it will get culled.")]
    public float distanceToCull = 30;
    
    [Range(0, 30), Tooltip("Run the cycle every set frames. The bigger the number, the better it is for performance but less accurate.")]
    public int cycleFrames = 7; 
    [Range(0, 10), Tooltip("This sets how many frames to rest before moving to the next calculation in the loop. This helps spread the distance culling calculations across several frames which improves performance.")]
    public int restFrames = 2;

    [Tooltip("If set to true, the culling will be disabling InboraStudio only and playing the idle animation set in the [Anim To Play On Cull] property in InboraStudio inspector. When within range again, InboraStudio will re-enable.")]
    public bool disableInboraStudioOnly;


    #region SYSTEM VARIABLES

    public static InboraStudioAIDistanceCulling instance;

    List<InboraStudioAI> agentsList = new List<InboraStudioAI>();
    int framesPassed = 0;
    
    int restFramesPassed = 0;
    bool isLooping = false;

    #endregion

    #region UNITY & SYSTEM METHODS

    void Awake()
    {
        if (instance == null) {
            instance = this;
        }
        else {
            Destroy(this);
        }

        if (autoCatchCamera) {
            cameraOrPlayer = Camera.main.transform;
            return;
        }

        if (cameraOrPlayer == null) {
            Debug.LogWarning("No camera has been set in the camera property in the InboraStudio AI Distance Culling component.");
        }

        if (restFrames < 0) {
            restFrames = 0;
        }

        if (cycleFrames < 0) {
            cycleFrames = 0;
        }
    }

    public virtual void Update()
    {
        // prevent continuation if no camera set
        if (cameraOrPlayer == null) {
            return;
        }

        if (isLooping) return;

        // increment the frames for the cycle
        if (framesPassed < cycleFrames) {
            framesPassed++;
            return;
        }

        framesPassed = 0;

        if (!isLooping) {
            StartCoroutine("RunCulling");
        }
    }

    public virtual IEnumerator RunCulling()
    {
        isLooping = true;

        int max = agentsList.Count;
        for (int i=0; i<max; i++) 
        {
            if (i > 0) {
                while (restFramesPassed < restFrames) {
                    restFramesPassed++;
                    yield return null;
                }
            }

            restFramesPassed = 0;
            
            if (i > agentsList.Count-1) {
                break;
            }

            InboraStudioAI InboraStudio = agentsList[i];
            if (InboraStudio == null) {
                agentsList.RemoveAt(i);
                continue;
            }

            // calculate the distance using sqr magnitude since it's faster than Vector3.Distance()
            float agentDistance = (InboraStudio.transform.position - cameraOrPlayer.position).sqrMagnitude;

            // if distance is larger than set -> cull the AI
            if (agentDistance > distanceToCull * distanceToCull) 
            {
                if (disableInboraStudioOnly) {
                    if (!InboraStudio.enabled) {
                        continue;
                    }

                    InboraStudio.enabled = false;
                    
                    if (InboraStudio.state != InboraStudioAI.State.death) {
                        PlayCullAnim(InboraStudio);
                    }

                    continue;
                }

                if (!InboraStudio.gameObject.activeSelf) {
                    continue;
                }

                InboraStudio.gameObject.SetActive(false);
                continue;
            }
            

            // reaching this point means distance is less than set -> re-enable AI
            
            
            if (disableInboraStudioOnly) 
            {
                if (!InboraStudio.enabled) {
                    InboraStudio.enabled = true;
                    continue;
                }
            }

            if (!InboraStudio.gameObject.activeSelf) {
                InboraStudio.gameObject.SetActive(true);
                PlayCullAnim(InboraStudio);
            }
        }

        isLooping = false;
    }

    #endregion

    #region APIs

    // add agent to the list of culling
    public virtual void AddAgent(InboraStudioAI agent)
    {
        if (agentsList.Contains(agent)) {
            return;
        }

        agentsList.Add(agent);
    }

    // remove agent from the list of culling
    public virtual void RemoveAgent(InboraStudioAI agent)
    {
        if (!agentsList.Contains(agent)) {
            return;
        }

        agentsList.Remove(agent);
    }

    // check if passed agent is in the list of culling
    public bool CheckAgent(InboraStudioAI agent)
    {
        if (agentsList.Contains(agent)) {
            return true;
        }

        return false;
    }

    // play the cull animation
    void PlayCullAnim(InboraStudioAI InboraStudio)
    {
        if (InboraStudio.animToPlayOnCull.Length > 0 && InboraStudio.animToPlayOnCull != null) {
            InboraStudio.animManager.Play(InboraStudio.animToPlayOnCull, 0.25f);
        }
    }

    #endregion
}