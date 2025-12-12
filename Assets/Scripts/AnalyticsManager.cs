using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Services.Analytics;
using UnityEngine;
using Event = Unity.Services.Analytics.Event;

public class AnalyticsManager : MonoBehaviour
{
    public static TerrainUsageTracker terrainTracker;
    public static PerPixelUsageTracker perPixelTracker;
    public static LayersUsageTracker layersTracker;
    public static ScalebarUsageTracker scalebarTracker;

    private void OnApplicationQuit()
    {
        if (terrainTracker != null) terrainTracker.RecordEvent();
        if (perPixelTracker != null) perPixelTracker.RecordEvent();
        if (layersTracker != null) layersTracker.RecordEvent();
        if (scalebarTracker != null) scalebarTracker.RecordEvent();
        Debug.Log("All events recorded");
    }
}

public class PerPixelUsageTracker : BaseToolTracker
{
    public PerPixelUsageTracker() : base("perPixelUsageTracker") { }
    public new void RecordEvent()
    {
        pinsPlaced = pinCount;
        base.RecordEvent();
    }

    public int pinCount = 0;
    int pinsPlaced { set => SetParameter("pinsPlaced", value); }
}

public class LayersUsageTracker : BaseToolTracker
{
    public LayersUsageTracker() : base("layersUsageTracker") { }
}

public class ScalebarUsageTracker : BaseToolTracker
{
    public ScalebarUsageTracker() : base("scalebarUsageTracker") { }
}

public class TerrainUsageTracker : BaseToolTracker
{
    /// <summary>
    /// Terrain tracker event constructor
    /// </summary>
    /// <param name="name"></param>
    /// <param name="url">Terrain JSON url</param>
    /// <param name="terrainType">True if custom terrain, false if sample terrain.</param>
    public TerrainUsageTracker(string name, string url, bool customTerrain) : base("terrainUsageTracker")
    {
        terrainName = name;
        terrainURL = url;
        isCustom = customTerrain;
    }
    
    string terrainName { set { SetParameter("terrainName", value);}}
    string terrainURL { set { SetParameter("terrainURL", value);}}
    bool isCustom { set {SetParameter("isCustomTerrain", value);}}
}

public abstract class BaseToolTracker : Event
{
    protected float startTime;

    protected BaseToolTracker(string name) : base(name)
    {
        startTime = Time.time;
    }

    public void RecordEvent()
    {
        usageTime = Time.time - startTime;
        AnalyticsService.Instance.RecordEvent(this);
    }
    
    float usageTime { set => SetParameter("usageTime", value); }
}