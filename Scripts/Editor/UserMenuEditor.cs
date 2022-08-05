using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Carousel
{
namespace BaselineAgent{
[CustomEditor(typeof(UserMenu))]
public class UserMenuEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        var controller = (UserMenu)target;
        if (GUILayout.Button("ToggleFollower"))
        {
            controller.ToggleFollower();
        }   
        if (GUILayout.Button("ToggleMirror"))
        {
            controller.ToggleMirror();
        }   
        if (GUILayout.Button("SetHeight"))
        {
            controller.SetHeight();
        }   
        if (GUILayout.Button("ExitToLobby"))
        {
            controller.ExitToLobby();
        }

    }

}
}
}