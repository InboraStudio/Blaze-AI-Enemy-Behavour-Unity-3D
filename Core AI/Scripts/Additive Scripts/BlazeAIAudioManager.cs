using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[AddComponentMenu("InboraStudio AI/Additive Scripts/InboraStudio AI Audio Manager")]
public class InboraStudioAIAudioManager : MonoBehaviour
{
    #region PROPERTIES

    [Tooltip("Auto catch the game camera so that based on it's distance from AIs, it plays the patrol audios from closest AIs.")]
    public bool autoCatchCamera = true;

    [Tooltip("Set the camera or player transform so that based on it's distance from AIs, it plays the patrol audios from closest AIs.")]
    public Transform cameraOrPlayer;
    
    [Min(0), Tooltip("A distance check will be made between the player/camera and all AIs in the manager. If the distance is less or equal to this value, the patrol audio will play. If more, the AI audio won't play until the player is closer.")]
    public float distanceToPlay = 30;
    
    [Min(0), Tooltip("A random time value will be generated between the two values to play an audio. For a constant value set the two fields to the same value.")]
    public Vector2 playAudioEvery = new Vector2(10, 30);

    #endregion

    #region SYSTEM VARIABLES

    public static InboraStudioAIAudioManager instance;
    List<InboraStudioAI> InboraStudioList = new List<InboraStudioAI>();
    List<InboraStudioAI> eligibleAIList = new List<InboraStudioAI>();
    InboraStudioAI lastInboraStudioPlayed;

    float chosenTime = 0;
    float timer = 0;
    bool isLooping = false;
    int restFrames = 2;
    int restFramesElapsed = 0;

    #endregion

    #region UNITY METHODS

    void Start()
    {
        SetInstance();
        SetCamera();
        chosenTime = Random.Range(playAudioEvery.x, playAudioEvery.y);
        // no need to be enabled on start -> will enable when AI adds itself to manager
        enabled = false;
    }

    void Update()
    {
        if (cameraOrPlayer == null) {
            Debug.LogWarning("No transform set to the CameraOrPlayer property in the Audio Manager component. Please set a transform.");
            return;
        }

        // if last played AI hasn't finished yet
        if(!LastAISilent()) return;
        
        timer += Time.deltaTime;
        if (timer < chosenTime) return;
        timer = 0;

        // prepare for next cycle
        chosenTime = Random.Range(playAudioEvery.x, playAudioEvery.y);

        if (!isLooping) {
            StartCoroutine("AudioManagerLoop");
        }
    }

    #endregion

    #region SYSTEM METHODS

    void SetInstance()
    {
        if (InboraStudioAIAudioManager.instance == null) {
            instance = this;
            return;
        }
    
        Destroy(this);
    }

    void SetCamera()
    {
        if (autoCatchCamera) {
            if (Camera.main != null) cameraOrPlayer = Camera.main.transform;
            else Debug.LogWarning("Audio Manager component couldn't auto find the main camera. Please make sure your camera has the MainCamera tag OR set the CameraOrPlayer property in this component to the player instead (by disabling Auto Catch Camera).");

            return;
        }

        if (cameraOrPlayer == null) {
            Debug.LogWarning("No transform set to the CameraOrPlayer property in the Audio Manager component. Please set a transform.");
        }
    }

    IEnumerator AudioManagerLoop()
    {
        isLooping = true;

        for (int i=0; i<InboraStudioList.Count; i++)
        {
            if (i > 0) {
                while (restFramesElapsed < restFrames) {
                    restFramesElapsed++;
                    yield return null;
                }
            }

            restFramesElapsed = 0;

            if (InboraStudioList[i] == null) {
                InboraStudioList.RemoveAt(i);
                eligibleAIList.Remove(InboraStudioList[i]);
                continue;
            }

            if (cameraOrPlayer == null) break;

            float distance = Vector3.Distance(InboraStudioList[i].transform.position, cameraOrPlayer.position);
            if (distance > distanceToPlay) continue;
            if (InboraStudioList[i].state != InboraStudioAI.State.normal && InboraStudioList[i].state != InboraStudioAI.State.alert) continue;

            eligibleAIList.Add(InboraStudioList[i]);
        }

        if (eligibleAIList.Count > 0) {
            int randIndex = Random.Range(0, eligibleAIList.Count);
            lastInboraStudioPlayed = eligibleAIList[randIndex];
            lastInboraStudioPlayed.PlayPatrolAudio();
            eligibleAIList.Clear();
        }

        isLooping = false;
    }

    bool LastAISilent()
    {
        if (lastInboraStudioPlayed == null) return true;

        if (lastInboraStudioPlayed.state == InboraStudioAI.State.normal || lastInboraStudioPlayed.state == InboraStudioAI.State.alert) {
            if (!lastInboraStudioPlayed.agentAudio.isPlaying) {
                return true;
            }

            return false;
        }

        lastInboraStudioPlayed = null;
        return true;
    }

    public void AddToManager(InboraStudioAI InboraStudio)
    {
        if (InboraStudioList.Contains(InboraStudio)) return;
        InboraStudioList.Add(InboraStudio);
        enabled = true;
    }

    public void RemoveFromManager(InboraStudioAI InboraStudio)
    {
        if (!InboraStudioList.Contains(InboraStudio)) return;

        InboraStudioList.Remove(InboraStudio);
        
        if (InboraStudioList.Count == 0) {
            Disable();
        }
    }

    public List<InboraStudioAI> ReturnList()
    {
        return InboraStudioList;
    }

    void Disable()
    {
        enabled = false;
        StopCoroutine("AudioManagerLoop");
        isLooping = false;
        timer = 0;
    }

    #endregion
}