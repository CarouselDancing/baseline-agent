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

           t.EnterScene(true);
        }
        if (GUILayout.Button("Join"))
        {

           t.EnterScene(false);
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