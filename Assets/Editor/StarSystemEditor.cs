using System;
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

    private void OnFocus()
    {
        SceneView.duringSceneGui -= this.OnSceneGUI;
        SceneView.duringSceneGui += this.OnSceneGUI;

    }

    private void OnDestroy()
    {
        SceneView.duringSceneGui -= this.OnSceneGUI;
    }

    private bool paintMode = false;

    private void OnGUI()
    {

        paintMode = GUILayout.Toggle(paintMode, "Start painting", "Button", GUILayout.Height(60f));

    }

    private void OnSceneGUI(SceneView sceneView)
    {
        if (paintMode)
        {
            DisplayVisualHelp();
        }
    }

    private Vector2 cellSize = new Vector2(200f, 200f);
    private void DisplayVisualHelp()
    {
        Ray guiRay = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
        Vector3 mousePosition = guiRay.origin - 
            guiRay.direction * (guiRay.origin.z / guiRay.direction.z);

        Vector2Int cell = new Vector2Int(Mathf.RoundToInt(mousePosition.x / cellSize.x)
            , Mathf.RoundToInt(mousePosition.y / cellSize.y));

        Vector2 cellCenter = cell * cellSize;

        Vector3 topLeft = cellCenter + 
            Vector2.left * cellSize * 0.5f + Vector2.up * cellSize * 0.5f;
        Vector3 topRight = cellCenter - 
            Vector2.left * cellSize * 0.5f + Vector2.up * cellSize * 0.5f;
        Vector3 bottomLeft = cellCenter + 
            Vector2.left * cellSize * 0.5f - Vector2.up * cellSize * 0.5f;
        Vector3 bottomRight = cellCenter - 
            Vector2.left * cellSize * 0.5f - Vector2.up * cellSize * 0.5f;

        Handles.color = Color.green;

        Vector3[] lines = { 
            topLeft, topRight, 
            topRight, bottomRight, 
            bottomRight, bottomLeft, 
            bottomLeft, topLeft 
        };


        Handles.DrawLines(lines);
    }
}
