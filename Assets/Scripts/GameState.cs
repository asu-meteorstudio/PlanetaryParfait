using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UserInterface;

enum BuildType
{
    Desktop,
    VR
}

public class GameState : MonoBehaviour
{
    #region STATIC VARS
    
    /// <summary>
    /// True if user is in an active multiuser session, false otherwise.
    /// </summary>
    public static bool InMultiuser { get; set; }
    
    /// <summary>
    /// True if the user is in VR, false if user is in Desktop. 
    /// </summary>
    public static bool IsVR { get; set; } 
    
    /// <summary>
    /// True if user can access terrain tools and interact with terrain. 
    /// </summary>
    public static bool InTerrain
    {
        get;
        set;
    } 
    
    // DEBUG VARIABLES
    public static bool generateDataImages;
    public static bool printPerPixelCoordinates;
    
    /// <summary>
    /// Allows developer to view VR menus in world-space as a desktop user
    /// </summary>
    public static bool vrDebug = true;
    
    #endregion
    
    #region PRIVATE VARS

    /// <summary>
    /// This variable should reflect the current build platform. Desktop for Windows/Mac/Linux builds, and VR for Android Builds.
    /// </summary>
    [SerializeField] private BuildType SelectBuildType = BuildType.Desktop; 
    
    // Desktop / VR rig variables
    [SerializeField] private GameObject xrRig;
    [SerializeField] private GameObject armature;
    [SerializeField] private GameObject xrRigPrefab;
    [SerializeField] private GameObject desktopPrefab;
    [SerializeField] private Canvas mainCanvas;
    
    #endregion

    void Awake()
    {
        SetBuildType();
        SetUpMesh(IsVR);
        InMultiuser = false;
    }

    /// <summary>
    /// Configures scene based on whether the application is built for VR or Desktop. 
    /// </summary>
    private void SetBuildType()
    {
        switch (SelectBuildType)
        {
            case BuildType.VR:
                IsVR = true;
                mainCanvas.renderMode = RenderMode.WorldSpace;
                print("VR build");
                break;
            case BuildType.Desktop:
                IsVR = false;
                mainCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
                mainCanvas.GetComponent<CanvasScaler>().matchWidthOrHeight = 1;
                print("Desktop build");
                break;
            default:
                Debug.LogError("Unable to find build target. Please try again");
                break;
        }
    }

    /// <summary>
    /// Configures player rig for the appropriate user. 
    /// </summary>
    /// <param name="inVR">Enables VR player rig if true. Enables desktop player rig if false.</param>
    private void SetUpMesh(bool inVR)
    {
        armature.SetActive(!inVR);
        xrRig.SetActive(inVR);
    }
}
