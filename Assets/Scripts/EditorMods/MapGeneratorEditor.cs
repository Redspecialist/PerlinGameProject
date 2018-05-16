using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;

[CustomEditor (typeof (MapGenScript))]
public class MapGeneratorEditor : Editor {


    public override void OnInspectorGUI()
    {
        MapGenScript mapGen = (MapGenScript)target;

        if (DrawDefaultInspector())
        {
            if (mapGen.autoUpdate)
            {
                mapGen.DrawMapInEditor();
            }
        }

        if (GUILayout.Button("Generate"))
        {
            mapGen.DrawMapInEditor();
        }
        if(GUILayout.Button("Save Mesh"))
        {
            mapGen.SaveMesh();
        }
    }

}
#endif
