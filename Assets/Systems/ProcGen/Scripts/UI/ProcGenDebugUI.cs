using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ProcGenDebugUI : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI StatusDisplay;
    [SerializeField] ProcGenManager TargetManager;
    [SerializeField] GameObject loadingScreen;

    void Awake()
    {
        OnRegenerate();
    }

    public void OnRegenerate()
    {
        StartCoroutine(PerformRegeneration());
    }

    IEnumerator PerformRegeneration()
    {
        yield return TargetManager.AsyncRegenerateWorld(OnStatusReported);

        loadingScreen.SetActive(false);

        yield return null;
    }

    void OnStatusReported(EGenerationStage currentStage, string status)
    {
        StatusDisplay.text = $"Fase {(int)currentStage} de {(int)EGenerationStage.NumStages}: {status}";
    }
}
