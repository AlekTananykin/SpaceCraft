using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class StarSystemEditor : EditorWindow
{
    [MenuItem("Custom/StarSystem")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(StarSystemEditor));
    }

    private void OnGUI()
    {



    }
}
