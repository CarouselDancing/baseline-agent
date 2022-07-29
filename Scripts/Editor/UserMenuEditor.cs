using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Carousel
{
    [CustomEditor(typeof(UserMenu))]
    public class UserMenuEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            UserMenu t = (UserMenu)target;

            if (GUILayout.Button("Follow"))
            {

                t.ToggleFollower();
            }
            if (GUILayout.Button("SetHeight"))
            {

                t.SetHeight();
            }
         
        }
    }
}