using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Carousel
{
namespace BaselineAgent{
[CustomEditor(typeof(RBGrabber))]
public class RBGrabberEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        var controller = (RBGrabber)target;

        if (GUILayout.Button("Grab"))
        {
            controller.GrabObject();
        }   
        if (GUILayout.Button("Release"))
        {
            controller.ReleaseObject();
        }

    }

}
}
}