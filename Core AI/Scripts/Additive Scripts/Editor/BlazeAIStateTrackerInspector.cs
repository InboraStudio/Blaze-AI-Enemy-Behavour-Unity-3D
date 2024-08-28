using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(InboraStudioAIStateTracker))]
public class InboraStudioAIStateTrackerInspector : Editor
{
    InboraStudioAIStateTracker script;

    public override void OnInspectorGUI()
    {
        script = (InboraStudioAIStateTracker) target;
        string text;
        bool InboraStudioExist = script.GetInboraStudio();

        if (!Application.isPlaying) {
            text = "Place on a gameobject where InboraStudio AI exists. Then on game start this will automatically track the state as it changes.";
        }
        else {
            if (InboraStudioExist) {
                text = script.GetState();
            }
            else {
                text = "Place on a gameobject where InboraStudio AI exists. Then on game start this will automatically track the state as it changes.";
            }
        }

        GUILayout.Button(text, BoxStyling(InboraStudioExist && Application.isPlaying), GUILayout.MinWidth(70), GUILayout.Height(200));
        EditorGUILayout.Space();

        if (InboraStudioExist) {
            EditorGUILayout.LabelField("InboraStudio AI detected", LabelStyling(true), GUILayout.MinWidth(70), GUILayout.Height(30));
        }
        else {
            EditorGUILayout.LabelField("No InboraStudio AI detected", LabelStyling(false), GUILayout.MinWidth(70), GUILayout.Height(30));
        }
    }

    GUIStyle BoxStyling(bool isPlaying)
    {
        var boxStyle = new GUIStyle();
        
        if (isPlaying) boxStyle.fontSize = 25;
        else boxStyle.fontSize = 18;

        boxStyle.margin = new RectOffset(4,4,2,2);
        boxStyle.alignment = TextAnchor.MiddleCenter;
        boxStyle.normal.background = InboraStudioAIEditor.MakeTex(1, 1, new Color(0.15f, 0.15f, 0.15f));
        boxStyle.normal.textColor = new Color(1, 0.5f, 0);
        boxStyle.active.textColor = new Color(1, 0.5f, 0);
        boxStyle.wordWrap = true;

        return boxStyle;
    }

    GUIStyle LabelStyling(bool isDetected)
    {
        var labelStyle = new GUIStyle();
        labelStyle.fontSize = 15;
        labelStyle.alignment = TextAnchor.MiddleCenter;
        labelStyle.normal.background = InboraStudioAIEditor.MakeTex(1, 1, new Color(0.1f, 0.1f, 0.1f));
        
        if (isDetected) labelStyle.normal.textColor = new Color(0, 1, 0);
        else labelStyle.normal.textColor = new Color(1, 0, 0);
        
        labelStyle.wordWrap = true;
        return labelStyle;
    }
}