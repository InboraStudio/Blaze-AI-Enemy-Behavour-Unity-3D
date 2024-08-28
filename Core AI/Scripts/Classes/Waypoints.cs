using UnityEngine;
using System.Collections.Generic;

namespace InboraStudioAISpace
{
    [System.Serializable]
    public class Waypoints
    {
        [Header("Set Waypoints"), Tooltip("Locations of the waypoints in world space. Will appear as orange spheres at agent's location to tweak their locations visually but the [Randomize] property must be set to off. If Randomize is set to off the waypoints CAN NOT be 0 and will add current agent position as the first waypoint.")]
        public List<Vector3> waypoints = new List<Vector3>();
        [Tooltip("Setting this to true will loop the waypoints when patrolling, setting it to false will stop at the last waypoint.")]
        public bool loop = false;

        [Tooltip("Set the idle rotation for each waypoint. Set the turning animations below. The rotation direction is shown in the scene view as red squares along the waypoints. If both the x and y are 0 then no rotation will occur and no red squares will appear. THIS GETS SET AUTOMATICALLY BASED ON NUMBER OF WAYPOINTS.")]
        public List<Vector2> waypointsRotation = new List<Vector2>();
        [Min(0), Tooltip("The amount of time in seconds to pass before turning to waypoint rotation.")]
        public float timeBeforeTurning = 0.2f;
        [Tooltip("Turning speed of waypoints rotations and movement turning.")]
        public float turnSpeed = 2f;
        
        [Header("Show & Set Waypoints In Scene"), Tooltip("This will show the waypoints in the scene view. Randomize must be set to off.")]
        public bool showWaypoints = true;
        
        [Header("Randomize Waypoints"), Tooltip("Enabling randomize will instead generate randomized waypoints within a radius from the start position in a continuous fashion and won't use the pre-set waypoints.")]
        public bool randomize = true;
        [Min(0), Tooltip("The radius from the start position to get a randomized position.")]
        public float randomizeRadius = 20f;
        [Tooltip("This ensures that when the AI generates a random point, it's on the same level or with a slight difference as set here. Set the min and max level difference.")]
        public Vector2 minAndMaxLevelDiff = new Vector2(-0.5f, 0.5f);
        [Tooltip("Shows the radius as a yellow disc in the scene view. Randomize needs to be enabled for this to show.")]
        public bool showRandomizeRadius;

        [Header("Turning")]
        [Tooltip("Movement turning will make the AI when in normal-alert states turn to the correct direction before moving and always turn to face the correct path. The turn speed is the property found above.")]
        public bool useMovementTurning = false;
        [Range(-1f, 1f), Tooltip("Movement turning will be used if the dot product between path corner and AI forward is equal to or less than this value. Best to keep it between 0.5 - 0.7.")]
        public float movementTurningSensitivity = 0.5f;
        [Tooltip("Play turn animations when turning. This doesn't apply on attack state (it has it's own property to apply turning)")]
        public bool useTurnAnims;

        [Header("Turn Animations"), Tooltip("The animation state name that will be called for turning right in normal state.")]
        public string rightTurnAnimNormal;
        [Tooltip("The animation state name that will be called for turning left in normal state.")]
        public string leftTurnAnimNormal;
        [Tooltip("The animation state name that will be called for turning right in alert and attack states.")]
        public string rightTurnAnimAlert;
        [Tooltip("The animation state name that will be called for turning left in alert and attack states.")]
        public string leftTurnAnimAlert;
        [Tooltip("Transition time from any state to the turning animation.")]
        public float turningAnimT = 0.25f;

        // save inspector states
        bool inspectorLoop;
        bool inspectorRandomize;


        // GUI validation for the waypoint system -> pass the transform.position of the AI
        public void WaypointsValidation(Vector3 position) 
        {
            if (randomize && loop) {
                randomize = !inspectorRandomize;
                loop = !inspectorLoop;
            }

            
            inspectorLoop = loop;
            inspectorRandomize = randomize;

            
            if (!randomize) {
                showRandomizeRadius = false;

                // if randomize is off and no waypoints -> set current position as waypoint
                if (waypoints.Count <= 0) {
                    waypoints.Add(position);
                    waypointsRotation.Clear();
                    waypointsRotation.Add(Vector2.zero);

                    return;
                }
            }


            // if one waypoint -> set point to current AI position
            for (int i=0; i<waypoints.Count; i+=1) {
                if (waypoints[i] == Vector3.zero) {
                    waypoints[i] = position;
                }
            }


            // set rotations list size
            WaypointsRotationValidate();
        }

        // set waypoint rotations list
        public void WaypointsRotationValidate()
        {
            
            if (waypointsRotation != null) {
                List<Vector2> listCopy = new List<Vector2>(waypointsRotation);


                int waypointsCount = waypoints.Count;
                waypointsRotation.Clear();
                
                
                for (int i=0; i<waypointsCount; i++) {
                    if (i <= listCopy.Count - 1) {
                        Vector2 temp = listCopy[i];

                        if (temp.x > 0.5f) temp.x = 0.5f;
                        if (temp.y > 0.5f) temp.y = 0.5f;
                        if (temp.x < -0.5f) temp.x = -0.5f;
                        if (temp.y < -0.5f) temp.y = -0.5f;

                        waypointsRotation.Add(temp);
                        continue;
                    }

                    waypointsRotation.Add(Vector2.zero);
                }
            }
        }

        #region EDITOR CODE

        #if UNITY_EDITOR
        // mark the waypoints in editor-view
        public void Draw(Vector3 position, InboraStudioAI InboraStudio)
        {
            if (randomize) 
            {
                if (showRandomizeRadius) 
                {
                    Gizmos.color = Color.yellow;
                    RaycastHit hit;
                    if (Physics.Raycast(InboraStudio.transform.position, -Vector3.up, out hit, Mathf.Infinity, Physics.AllLayers)) {
                        Debug.DrawRay(InboraStudio.transform.position, hit.point - InboraStudio.transform.position, Color.yellow, 0.1f);
                        UnityEditor.Handles.color = Color.yellow;
                        UnityEditor.Handles.DrawWireDisc(hit.point, InboraStudio.transform.up, randomizeRadius);
                    }
                }

                return;
            }

            if (!showWaypoints) {
                return;
            }

            for (int i = 0; i < waypoints.Count; i++) 
            {
                if (i == 0) {
                    Gizmos.color = new Color(1f, 0.3f, 0f);
                }
                else {
                    Gizmos.color = new Color(1f, 0.6f, 0.0047f);
                }

                
                RaycastHit hit;
                if (Physics.Raycast(waypoints[i], -Vector3.up, out hit, Mathf.Infinity, Physics.AllLayers)) {
                    Debug.DrawRay(waypoints[i], hit.point - waypoints[i], new Color(1f, 0.3f, 0f), 0.1f);
                    
                    UnityEditor.Handles.color = new Color(1f, 0.3f, 0f);
                    UnityEditor.Handles.DrawWireDisc(hit.point, InboraStudio.transform.up, 0.5f);
                    UnityEditor.Handles.Label(hit.point + new Vector3(0, 1, 0), "Waypoint " + (i+1));
                }
                
                if (InboraStudio.groundLayers.value == 0) {
                    Debug.LogWarning("Ground layers property not set. Make sure to set the ground layers in the main InboraStudio inspector (general tab) in order to see the waypoints visually.");
                }

                // Draws the waypoint rotation cubes
                if (waypointsRotation[i].x != 0 || waypointsRotation[i].y != 0) {
                    Gizmos.color = Color.red;
                    Gizmos.DrawCube(new Vector3(hit.point.x + waypointsRotation[i].x, hit.point.y, hit.point.z + waypointsRotation[i].y), new Vector3(0.3f, 0.3f, 0.3f));
                }

                if (waypoints.Count > 1) {
                    Gizmos.color = Color.blue;

                    if (i == 0) {
                        Gizmos.DrawLine(waypoints[0], waypoints[1]);
                    }
                    else if (i == waypoints.Count - 1) {
                        Gizmos.DrawLine(waypoints[i - 1], waypoints[i]);
                        Gizmos.color = Color.grey;
                        Gizmos.DrawLine(waypoints[waypoints.Count - 1], waypoints[0]);
                    }
                    else {
                        Gizmos.color = Color.blue;
                        Gizmos.DrawLine(waypoints[i - 1], waypoints[i]);
                    } 
                }
            }
        }
        #endif

        #endregion
    }
}