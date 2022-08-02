using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


namespace Carousel
{
[CustomEditor(typeof(MirrorGameManager))]
public class MirrorGameManagerEditor : Editor

{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        MirrorGameManager t = (MirrorGameManager)target;
    
        if (GUILayout.Button("Host"))
        {

           t.HostServer();
        }
        if (GUILayout.Button("Join"))
        {

           t.JoinServer("");
        }
        if (GUILayout.Button("Server"))
        {

           t.StartServer();
        }
        if (GUILayout.Button("StartMirror"))
        {

           t.StartMirror();
        }
    }
}
}