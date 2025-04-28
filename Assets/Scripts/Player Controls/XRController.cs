
using UnityEngine.InputSystem;
using UserInterface;
using TerrainEngine.Tools;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

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
    public Transform terrainTiles;
    private GameObject playersListGameObject;
    private GameObject temp;

    [Header("Interaction Types")]
    [HideInInspector] public bool hovering;
    [HideInInspector] public bool joystickActive;
    [HideInInspector] public bool triggerActive;
    [HideInInspector] public bool leftHandActive; 
    [HideInInspector] public bool gripActive;
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
        gameObject.GetComponent<ContinuousMoveProviderBase>().enabled = !gripActive;

        if (gripActive)
        {
            Vector3 inputDirection = transform.right * movePlatform.x + transform.forward * movePlatform.y;
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
    }

    #endregion

    #region INPUT
    /// <summary>
    /// These methods detect input using InputAction.CallbackContext & Default Input Actions. 
    /// </summary>
    /// <param name="context"></param>

    public void OnSelect(InputAction.CallbackContext context)
    {
        gripActive = context.performed;
    }

    public void OnSelectValue(InputAction.CallbackContext context)
    {
        //throw new NotImplementedException();
    }

    public void OnActivate(InputAction.CallbackContext context)
    {
        triggerActive = context.performed; // the action is completed
        leftHandActive = context.ToString().Contains("LeftHand");

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
       //throw new NotImplementedException();
    }


    #endregion

}

}