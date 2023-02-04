using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PathdataManager))]
public class PathdataManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        if (GUILayout.Button("Gerar Pathdata"))
        {
            (serializedObject.targetObject as PathdataManager).OnEditorBuildPathdata();
        }

        serializedObject.ApplyModifiedProperties();
    }
}
