using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Carousel{

namespace BaselineAgent{

[CustomEditor(typeof(AgentController))]
public class AgentControllerEditor : Editor

{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        AgentController c = (AgentController)target;
    
        if (GUILayout.Button("ToggleDancing"))
        {

           c.ToggleDancing();
        }
    }
}
}
}