///-----------------------------------------------------------------
///   Class:          CaveMapEditor
///   Description:    Cave map editor, provides a display view of
///                   the map contained inside the cave editor object
///   Author:         Thiago de Araujo Silva  Date: 8/11/2016
///-----------------------------------------------------------------
using UnityEditor;
using CaveMapGenerator;

/// <summary>
/// This editor shows the picture of the cave map in it
/// </summary>
[CustomEditor(typeof(CaveMap))]
public class CaveMapEditor : Editor
{
    private Display display;
    CaveMap caveMap;
    public override void OnInspectorGUI()
    { 
        if(caveMap == null)
            caveMap = (CaveMap)target;

        if (display == null){
            display = new Display(512);
            display.UpdateDisplay(caveMap);
        }

        EditorGUILayout.LabelField("This cave size is... " + caveMap.Size.ToString() + " tiles side");


        display.GUIDisplay(73, EditorGUIUtility.currentViewWidth);
    }
}
