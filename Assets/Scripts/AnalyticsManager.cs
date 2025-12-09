using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Services.Analytics;
using UnityEngine;

public class AnalyticsManager : MonoBehaviour
{
    public static CustomTerrainTracker currentCustomTerrainEvent;
    public static TerrainsVisitedTracker visitTracker = new ();

    private void OnApplicationQuit()
    {
        if (currentCustomTerrainEvent != null) currentCustomTerrainEvent.RecordEvent();
        visitTracker.RecordEvent();
        Debug.Log("All events recorded");
    }
}

public class TerrainsVisitedTracker : Unity.Services.Analytics.Event
{
    public TerrainsVisitedTracker() : base("terrainsVisited") { }

    public void RecordEvent()
    {
        terrainCount = count;
        AnalyticsService.Instance.RecordEvent(this);
    }

    public int count = 1; // including starting terrain
    int terrainCount { set { SetParameter("terrainCount", value);}}
}

public class CustomTerrainTracker : Unity.Services.Analytics.Event
{
    public CustomTerrainTracker(string name, string url) : base("customTerrainTracker")
    {
        startTime = Time.time;
        terrainName = name;
        terrainURL = url;
    }

    public void RecordEvent()
    {
        float endTime = Time.time;
        usageTime = endTime - startTime;
        AnalyticsService.Instance.RecordEvent(this);
    }

    float startTime;
    float usageTime { set { SetParameter("usageTime", value);}}
    string terrainName { set { SetParameter("terrainName", value);}}
    string terrainURL { set { SetParameter("terrainURL", value);}}
}