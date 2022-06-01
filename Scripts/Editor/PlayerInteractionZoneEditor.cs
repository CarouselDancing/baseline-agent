using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Carousel{

[CustomEditor(typeof(PlayerInteractionZone))]
public class PlayerInteractionZoneEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        var controller = (PlayerInteractionZone)target;

        if (GUILayout.Button("Activate"))
        {
            controller.ActivateAgent();
        }

        if (GUILayout.Button("Deactivate"))
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


    }

}

}