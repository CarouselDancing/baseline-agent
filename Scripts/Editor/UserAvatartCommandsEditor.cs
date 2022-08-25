using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Carousel
{
    namespace BaselineAgent{
[CustomEditor(typeof(UserAvatarCommands))]
public class UserAvatarCommandsEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        var controller = (UserAvatarCommands)target;

        if (GUILayout.Button("Generate"))
        {
            controller.SpawnAgent();
        }   
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
        }
        if (GUILayout.Button("Remove Agent"))
        {
            controller.RemoveAgent();
        }
        if (GUILayout.Button("Set Height"))
        {
            controller.SetHeight();
        }


        if (GUILayout.Button("GrabRight"))
        {
            controller.GrabRightHand();
        }
        if (GUILayout.Button("ReleaseRight"))
        {
            controller.ReleaseRightHand();
        }
        if (GUILayout.Button("GrabLeft"))
        {
            controller.GrabLeftHand();
        }
        
        if (GUILayout.Button("ReleaseLeft"))
        {
            controller.ReleaseLeftHand();
        }
        
        
    }

}
}
}