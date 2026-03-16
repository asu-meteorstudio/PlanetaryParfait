
using System.Numerics;
using UnityEngine.InputSystem;
using UserInterface;
using TerrainEngine.Tools;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.XR.Interaction.Toolkit;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

namespace XRControls
{
/// <summary>
/// This script handles VR interactions. 
/// </summary>
public class XRController : MonoBehaviour, 
    XRIDefaultInputActions.IXRIRightHandInteractionActions, 
    XRIDefaultInputActions.IXRIRightHandLocomotionActions, 
    XRIDefaultInputActions.IXRILeftHandInteractionActions, 
    XRIDefaultInputActions.IXRILeftHandLocomotionActions
{
    #region FIELDS

    public XRIDefaultInputActions controls;
    private Vector2 movePlatform;
    
    [Header("GameObjects")]
    public GameObject player;
    public Camera mainCamera;
    public Transform terrainTiles;
    public GameObject leftController;
    public GameObject rightController;
    
    public GameObject buttonHints;
    public GameObject platformButtonHints;
    public GameObject walkButtonHint;
    
    private GameObject playersListGameObject;
    private GameObject temp;
    private GameObject menu;

    [Header("Interaction Types")]
    [HideInInspector] public bool hovering;
    [HideInInspector] public bool joystickActive;
    [HideInInspector] public bool triggerActive;
    [FormerlySerializedAs("leftHandActive")] [HideInInspector] public bool rightHandActive; 
    [HideInInspector] public bool leftGripActive;
    [HideInInspector] public bool rightGripActive;
    [HideInInspector] public bool xActive;
    [HideInInspector] public bool yActive;
    [HideInInspector] public bool aActive;
    [HideInInspector] public bool bActive;
    #endregion

    #region MONO

    private void Start()
    {
        DontDestroyOnLoad(gameObject); // puts Event System on DontDestroyOnLoad so the values are the same when you join a multiplayer game
        
        // Create XRI Default Input Actions and enable it so it responds when the user provides input.
        controls = new XRIDefaultInputActions();
        controls.XRIRightHandInteraction.SetCallbacks(this);
        controls.XRILeftHandInteraction.SetCallbacks(this);
        controls.XRIRightHandLocomotion.SetCallbacks(this);
        controls.XRILeftHandLocomotion.SetCallbacks(this);
        controls.Enable();
        
        menu = GameObject.FindGameObjectWithTag("Menu");
        menu.SetActive(false);
    }
    
    private void Update()
    {
        MovePlatform();
    }

    #endregion

    #region METHODS

    public void ToggleGameObject(GameObject gameObj)
    {
        gameObj.SetActive(!gameObj.activeInHierarchy);
    }

    //Tells the menu that the interactor is hovering over one of it's objects and passes that object.
    public void Hovering(GameObject gameObj)
    {
        hovering = !hovering;
        if (hovering)
        {
            temp = gameObj;
        }
    }

    /// <summary>
    /// Platform movement controls for VR user. Left joystick translates the platform, X movements platform down, and Y moves platform up.
    /// </summary>
    private void MovePlatform()
    {
        if (!rightGripActive) return;
        Vector3 normalizedRight = new Vector3(mainCamera.transform.right.x, 0, mainCamera.transform.right.z);
        Vector3 normalizedForward = new Vector3(mainCamera.transform.forward.x, 0, mainCamera.transform.forward.z);
        Vector3 inputDirection = normalizedRight * movePlatform.x + normalizedForward * movePlatform.y;
        terrainTiles.position = Vector3.MoveTowards(terrainTiles.gameObject.transform.position, terrainTiles.position - inputDirection, SettingsController.PlatformSpeed() * Time.deltaTime);

        //moving pins
        foreach (Pin _pin in PerPixelDataReader.pinList)
        {
            _pin.pin.transform.position = Vector3.MoveTowards(_pin.pin.transform.position, _pin.pin.transform.position - new Vector3(player.GetComponentInChildren<Camera>().transform.forward.x, 0f, player.GetComponentInChildren<Camera>().transform.forward.z), SettingsController.PlatformSpeed() * Time.deltaTime);
            _pin.panel.transform.position = Vector3.MoveTowards(_pin.panel.transform.position, _pin.panel.transform.position - new Vector3(player.GetComponentInChildren<Camera>().transform.forward.x, 0f, player.GetComponentInChildren<Camera>().transform.forward.z), SettingsController.PlatformSpeed() * Time.deltaTime);
        }
            
        //moving platform UP
        if (yActive)
        {
            terrainTiles.position = Vector3.MoveTowards(terrainTiles.gameObject.transform.position, terrainTiles.position + Vector3.down, 5 * Time.deltaTime);

            //moving pins
            foreach (Pin _pin in PerPixelDataReader.pinList)
            {
                _pin.pin.transform.position = Vector3.MoveTowards(_pin.pin.transform.position, _pin.pin.transform.position + Vector3.down, SettingsController.PlatformSpeed() * Time.deltaTime);
                _pin.panel.transform.position = Vector3.MoveTowards(_pin.panel.transform.position, _pin.panel.transform.position + Vector3.down, SettingsController.PlatformSpeed() * Time.deltaTime);
            }
        }
            
        //moving platform DOWN
        if (xActive)
        {
            terrainTiles.position = Vector3.MoveTowards(terrainTiles.gameObject.transform.position, terrainTiles.position + Vector3.up, 5 * Time.deltaTime);

            //moving pins
            foreach (Pin _pin in PerPixelDataReader.pinList)
            {
                _pin.pin.transform.position = Vector3.MoveTowards(_pin.pin.transform.position, _pin.pin.transform.position + Vector3.up, SettingsController.PlatformSpeed() * Time.deltaTime);
                _pin.panel.transform.position = Vector3.MoveTowards(_pin.panel.transform.position, _pin.panel.transform.position + Vector3.up, SettingsController.PlatformSpeed() * Time.deltaTime);
            }
        }
    }

    #endregion

    #region INPUT
    /// <summary>
    /// These methods detect input using InputAction.CallbackContext & Default Input Actions. 
    /// </summary>
    /// <param name="context"></param>

    public void OnSelect(InputAction.CallbackContext context)
    {
        if (!context.ToString().Contains("RightHand"))
        {
            menu.SetActive(context.performed);
            buttonHints.SetActive(!context.performed); // turn off button hints while menu is open to prevent UI overlapping
        }
        else if (!context.ToString().Contains("LeftHand"))
        {
            rightGripActive = context.performed;
            gameObject.GetComponent<ContinuousMoveProviderBase>().enabled = !rightGripActive;
            platformButtonHints.SetActive(context.performed); // turn on platform button hints while platform mode is active
            walkButtonHint.SetActive(!context.performed);
        }
    }

    public void OnSelectValue(InputAction.CallbackContext context)
    {
        //throw new NotImplementedException();
    }

    public void OnActivate(InputAction.CallbackContext context)
    {
        triggerActive = context.performed; // the action is completed
        rightHandActive = context.ToString().Contains("RightHand");

        if (context.performed)
        {
            if (hovering && temp)
            {
                ToggleGameObject(temp);
            }
        }
    }

    public void OnActivateValue(InputAction.CallbackContext context)
    {
        //throw new NotImplementedException();
    }

    public void OnUIPress(InputAction.CallbackContext context)
    {
        //throw new NotImplementedException();
    }

    public void OnUIPressValue(InputAction.CallbackContext context)
    {
        //throw new NotImplementedException();
    }

    public void OnRotateAnchor(InputAction.CallbackContext context)
    {
        //throw new NotImplementedException();
    }

    public void OnTranslateAnchor(InputAction.CallbackContext context)
    {
        //throw new NotImplementedException();
    }

    public void OnTeleportSelect(InputAction.CallbackContext context)
    {
        //throw new NotImplementedException();
    }

    public void OnTeleportModeActivate(InputAction.CallbackContext context)
    {
        //throw new NotImplementedException();
    }

    public void OnTeleportModeCancel(InputAction.CallbackContext context)
    {
        //throw new NotImplementedException();
    }

    public void OnTurn(InputAction.CallbackContext context)
    {
        
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        joystickActive = context.performed;
        movePlatform = context.ReadValue<Vector2>();
    }

    public void OnY(InputAction.CallbackContext context)
    {
        yActive = context.performed;
    }

    public void OnX(InputAction.CallbackContext context)
    {
        xActive = context.performed;
    }

    public void OnA(InputAction.CallbackContext context)
    {
        aActive = context.performed;
        /*if (context.started && !context.canceled)
        {
            aActive = "started";
        }
        else if (!context.started && context.canceled)
        {
            aActive = "canceled";
        }*/

    }

    public void OnB(InputAction.CallbackContext context)
    {
        bActive = context.performed;
    }

    public void OnGripActive(InputAction.CallbackContext context)
    {

    }

    #endregion

}

}