using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Carousel
{
    namespace BaselineAgent{
[CustomEditor(typeof(RPMUserAvatar))]
public class RPMUserAvatarEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        var controller = (RPMUserAvatar)target;

        if (GUILayout.Button("Generate"))
        {
            controller.SpawnAgent();
        }   
        /*
        if (GUILayout.Button("ActivateAgent"))
        {
            controller.ActivateAgent();
        }
        if (GUILayout.Button("DeactivateAgent"))
        {
            controller.DeactivateAgent();
        } 
        if (GUILayout.Button("ActivateFollower"))
        {
            controller.ActivateFollower();
        }
         if (GUILayout.Button("DeactivateFollower"))
        {
            controller.DeactivateFollower();
        }
        if (GUILayout.Button("ActivatePairDance"))
        {
            controller.ActivatePairDance();
        }
         if (GUILayout.Button("DeactivatePairDance"))
        {
            controller.DeactivatePairDance();
        }
        if (GUILayout.Button("ToggleAgentDance"))
        {
            controller.ToggleAgentDancing();
        }*/

    }

}
}
}