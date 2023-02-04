using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Unity.EditorCoroutines.Editor;

[CustomEditor(typeof(ProcGenManager))]
public class ProcGenManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        if (GUILayout.Button("Regenerar Texturas"))
        {
            ProcGenManager targetManager = serializedObject.targetObject as ProcGenManager;
            targetManager.RegenerateTextures();
        }

        if (GUILayout.Button("Regenerar os Protótipos de Detalhes"))
        {
            ProcGenManager targetManager = serializedObject.targetObject as ProcGenManager;
            targetManager.RegenerateDetailPrototypes();
        }

        if (GUILayout.Button("Regenerar o Mundo"))
        {
            ProcGenManager targetManager = serializedObject.targetObject as ProcGenManager;
            EditorCoroutineUtility.StartCoroutine(PerformRegeneration(targetManager), this);
        }
    }

    int ProgressID;
    IEnumerator PerformRegeneration(ProcGenManager targetManager)
    {
        ProgressID = Progress.Start("A regenerar o terreno");

        yield return targetManager.AsyncRegenerateWorld(OnStatusReported);

        Progress.Remove(ProgressID);

        yield return null;
    }

    void OnStatusReported(EGenerationStage currentStage, string status)
    {
        Progress.Report(ProgressID, (int)currentStage, (int)EGenerationStage.NumStages, status);
    }
}
