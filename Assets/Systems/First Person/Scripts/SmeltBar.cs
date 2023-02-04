using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SmeltBar : MonoBehaviour
{
    private Slider slider;

    private float currentTime;
    private float maxTime;

    void Awake()
    {
        slider = GetComponent<Slider>();
    }

    void Update()
    {
        currentTime = SmeltSystem.Instance.remainingTime;
        maxTime = SmeltSystem.Instance.timeNeededToSmeltIron;

        float fillValue = currentTime / maxTime;
        slider.value = fillValue;
    }
}
