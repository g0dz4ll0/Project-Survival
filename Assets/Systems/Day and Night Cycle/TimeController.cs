using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TimeController : MonoBehaviour
{
    [SerializeField] float timeMultiplier;
    [SerializeField] float startHour;
    [SerializeField] TextMeshProUGUI timeText;
    [SerializeField] Light sunLight;
    [SerializeField] float sunriseHour;
    [SerializeField] float sunsetHour;
    [SerializeField] Color dayAmbientLight;
    [SerializeField] Color nightAmbientLight;
    [SerializeField] AnimationCurve lightChangeCurve;
    [SerializeField] float maxSunLightIntensity;
    [SerializeField] Light moonLight;
    [SerializeField] float maxMoonLightIntensity;

    public bool isInDungeon;

    DateTime currentTime;
    TimeSpan sunriseTime;
    TimeSpan sunsetTime;

    public Material stars;

    // Start is called before the first frame update
    void Start()
    {
        currentTime = DateTime.Now.Date + TimeSpan.FromHours(startHour);

        sunriseTime = TimeSpan.FromHours(sunriseHour);
        sunsetTime = TimeSpan.FromHours(sunsetHour);
    }

    // Update is called once per frame
    void Update()
    {
        if (!isInDungeon)
        {
            UpdateTimeOfDay();
            RotateSun();
            UpdateLightSettings();
        }
        else
        {
            SetLightSettingsInDungeon();
        }
    }

    private void UpdateTimeOfDay()
    {
        currentTime = currentTime.AddSeconds(Time.deltaTime * timeMultiplier);

        if (timeText != null)  
            timeText.text = currentTime.ToString("HH:mm");
    }

    private void RotateSun()
    {
        float sunLightRotation;

        if (currentTime.TimeOfDay > sunriseTime && currentTime.TimeOfDay < sunsetTime)
        {
            TimeSpan sunriseToSunsetDuration = CalculateTimeDifference(sunriseTime, sunsetTime);
            TimeSpan timeSinceSunrise = CalculateTimeDifference(sunriseTime, currentTime.TimeOfDay);

            double percentage = timeSinceSunrise.TotalMinutes / sunriseToSunsetDuration.TotalMinutes;

            sunLightRotation = Mathf.Lerp(0, 180, (float)percentage);

            if (stars.GetFloat("_Cutoff") < 1)
            {
                float alpha = stars.GetFloat("_Cutoff") * 100f;
                alpha += 3 * timeMultiplier * Time.deltaTime;
                alpha *= .01f;
                stars.SetFloat("_Cutoff", alpha);
            }
        }
        else
        {
            TimeSpan sunsetToSunriseDuration = CalculateTimeDifference(sunsetTime, sunriseTime);
            TimeSpan timeSinceSunset = CalculateTimeDifference(sunsetTime, currentTime.TimeOfDay);

            double percentage = timeSinceSunset.TotalMinutes / sunsetToSunriseDuration.TotalMinutes;

            sunLightRotation = Mathf.Lerp(180, 360, (float)percentage);

            if (stars.GetFloat("_Cutoff") > .7f)
            {
                float alpha = stars.GetFloat("_Cutoff") * 100f;
                alpha -= 3 * timeMultiplier * Time.deltaTime;
                alpha *= .01f;
                stars.SetFloat("_Cutoff", alpha);
            }
        }

        sunLight.transform.rotation = Quaternion.AngleAxis(sunLightRotation, Vector3.right);
    }

    private void UpdateLightSettings()
    {
        float dotProduct = Vector3.Dot(sunLight.transform.forward, Vector3.down);
        sunLight.intensity = Mathf.Lerp(0, maxSunLightIntensity, lightChangeCurve.Evaluate(dotProduct));
        moonLight.intensity = Mathf.Lerp(maxMoonLightIntensity, 0, lightChangeCurve.Evaluate(dotProduct));
        RenderSettings.ambientLight = Color.Lerp(nightAmbientLight, dayAmbientLight, lightChangeCurve.Evaluate(dotProduct));
    }

    private void SetLightSettingsInDungeon()
    {
        sunLight.intensity = 0;
        moonLight.intensity = 0.5f;
        RenderSettings.ambientLight = Color.black;
        RenderSettings.reflectionIntensity = 0;
    }

    private TimeSpan CalculateTimeDifference(TimeSpan fromTime, TimeSpan toTime)
    {
        TimeSpan difference = toTime - fromTime;

        if (difference.TotalSeconds < 0)
            difference += TimeSpan.FromHours(24);

        return difference;
    }
}
