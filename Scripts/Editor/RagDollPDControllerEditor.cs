
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Carousel{

namespace BaselineAgent{

[CustomEditor(typeof(RagDollPDController))]
public class RagDollPDControllerEditor : Editor

{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        var c = (RagDollPDController)target;
        if (GUILayout.Button("Activate"))
        {

            c.Activate();
        }
        if (GUILayout.Button("Deactivate"))
        {

            c.Deactivate();
        }
    }
}


[CustomEditor(typeof(RBRagDollPDController))]
public class RBRagDollPDControllerEditor : Editor

{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        var c = (RBRagDollPDController)target;
        if (GUILayout.Button("Activate"))
        {

            c.Activate();
        }
        if (GUILayout.Button("Deactivate"))
        {

            c.Deactivate();
        }
    }
}
}
}