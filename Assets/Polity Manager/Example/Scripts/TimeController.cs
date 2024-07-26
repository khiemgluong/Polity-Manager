using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TimeController : MonoBehaviour
{
    public static TimeController Singleton { get; private set; }
    public const int TimeInDay = 2400;

    [Range(0, TimeInDay)] public float time;
    [Range(1, 360)] public int day;
    [Tooltip("How fast time will go"), Range(0, 20)] public float timeMultiplier = 1;
    [SerializeField] private bool ControlLights = true;
    public delegate void DayChangedHandler();
    public static event DayChangedHandler OnDayChanged;
    [SerializeField] int timeInterval = 5;
    int lastInterval = 0;
    public delegate void TimeIntervalHandler();
    public static event TimeIntervalHandler OnTimeInterval;
    void Awake()
    {
        if (Singleton == null)
        {
            Singleton = this;
            Debug.LogWarning("TimeController Awake");
        }
        else
            Destroy(gameObject);
    }

    void Start()
    {
        if (timeInterval == 0) timeInterval = 1;
        if (day < 1) day = 1;
        if (ControlLights)
        {
            Light[] lights = FindObjectsOfType<Light>();
            foreach (Light li in lights)
                switch (li.type)
                {
                    case LightType.Disc:
                    case LightType.Point:
                    case LightType.Rectangle:
                    case LightType.Spot:
                        break;
                    case LightType.Directional:
                    default:
                        break;
                }
        }
        // RenderSettings.fog = true;
        // RenderSettings.fogMode = FogMode.Linear;
        // RenderSettings.fogStartDistance = fogStartDistance;
        // RenderSettings.fogEndDistance = fogEndDistance;
    }



    void Update()
    {
        time += Time.deltaTime * timeMultiplier;

        int currentInterval = Mathf.FloorToInt(time / timeInterval);

        // Check if we've reached a new interval
        if (currentInterval > lastInterval)
        {
            lastInterval = currentInterval;
            OnTimeInterval?.Invoke();
        }
        if (time >= TimeInDay)
        {
            day++;
            time = 0;
            OnDayChanged?.Invoke();
            Debug.LogWarning("On day " + day);
        }

    }

    public static string ConvertToStandardTime(float militaryTime)
    {
        // Validate input
        if (militaryTime < 0 || militaryTime >= TimeInDay)
        {
            return "Invalid time";
        }

        int hours = (int)(militaryTime / 100);
        float fractionalMinutes = militaryTime % 100;
        int minutes = (int)(fractionalMinutes / 100 * 60);

        string amPm = hours >= 12 ? "PM" : "AM";

        // Convert to 12-hour format
        hours %= 12;
        if (hours == 0)
            hours = 12;

        // Format time string
        return string.Format("{0}:{1:D2}{2}", hours, minutes, amPm);
    }

}