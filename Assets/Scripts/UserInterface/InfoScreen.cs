using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UserInterface;

public class InfoScreen : MonoBehaviour
{
    public static bool canInteract = false;
    private TerrainTools _terrainTools; // refers to TerrainTools class so we can call its methods.

    void Start()
    {
        _terrainTools = FindObjectOfType<TerrainTools>();
    }
    
    /// <summary>
    /// Called when the user collides into another GameObject that receives collisions (isTrigger enabled)
    /// https://docs.unity3d.com/ScriptReference/Collider.OnTriggerEnter.html 
    /// </summary>
    /// <param name="other"></param>
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player" && (/*other.name == "DesktopRig" || */other.name == "XRRig"))
        {
            canInteract = true;
            //_terrainTools.ShowTerrainTools(); // Enable TerrainTools UI
        }
    }

    /// <summary>
    /// Called when the user leaves the area where GameObject that receives collisions is
    /// https://docs.unity3d.com/ScriptReference/Collider.OnTriggerEnter.html 
    /// </summary>
    /// <param name="other"></param>
    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player" && (other.name == "DesktopRig" || other.name == "XRRig"))
        {
            canInteract = false;
            //_terrainTools.HideTerrainTools(); // Disable TerrainTools UI
        }
    }
}
