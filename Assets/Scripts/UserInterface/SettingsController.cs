using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.UIElements;
using UnityEngine.XR.Interaction.Toolkit;
using Slider = UnityEngine.UI.Slider;
using Toggle = UnityEngine.UI.Toggle;
using XRController = XRControls.XRController;

namespace UserInterface{

    public class SettingsController : Menu
    {
        public static ToggleDelegate OpenMenu;
        public delegate float SettingsDelegate();
        public static SettingsDelegate PlatformSpeed { get; private set; }
        public static SettingsDelegate MouseSensitivity { get; private set;  }

        [SerializeField] private GameObject xrRig;
        
        [Header("Speed Controls")]
        public float platformSpeed = 5f;
        public float mouseSen = 1f;

        [Header("GameObjects")] 
        public GameObject safeModeSettings; // TODO: safe mode for desktop?
        public Slider platformSlider;
        public GameObject rotateSlider;
        public GameObject mouseSlider;
        public UnityEngine.UI.Button exitButton;
        public XRController vrPlayer;

        // Start is called before the first frame update
        void Awake()
        {
            OpenMenu = ToggleMenu;
            PlatformSpeed = GetPlatformSpeed;
            MouseSensitivity = GetMouseSensitivity;

            SetDefaults();
        }

        new void Start()
        {
            base.Start();
        }
        
        public override void ToggleMenu(bool active)
        {
            parentObject.SetActive(active);
        }

        public override void SetListeners()
        {
            var safeToggle = parentObject.GetComponentInChildren<Toggle>();

            if (GameState.IsVR) // put into toggle listener
            {
                safeToggle.gameObject.SetActive(true);
                safeToggle.isOn = false;
                safeToggle.onValueChanged.AddListener(SafeMode);
                
                rotateSlider.gameObject.SetActive(true);
                rotateSlider.GetComponentInChildren<Slider>().onValueChanged.AddListener(SetVRRotateSpeed);

                mouseSlider.gameObject.SetActive(false);
            }
            else
            {
                safeToggle.gameObject.SetActive(false);
                rotateSlider.gameObject.SetActive(false);
                
                mouseSlider.gameObject.SetActive(true);
                mouseSlider.GetComponentInChildren<Slider>().onValueChanged.AddListener(SetMouseRotation);
            }
            
            platformSlider.onValueChanged.AddListener(SetPlatformSpeed);
            
            exitButton.onClick.AddListener(delegate
            {
                ToggleMenu(false);
                MainMenu.OpenPrimaryMenus(false);
                TerrainTools.OpenMenu(true);
            });
        }
        
        private void SetDefaults()
        {
            vrPlayer.GetComponent<ContinuousTurnProviderBase>().turnSpeed = 50f;

            // continuous turn is on by default
            if (GameState.IsVR)
            {
                xrRig.GetComponent<SnapTurnProviderBase>().enabled = false;
                xrRig.GetComponent<ContinuousTurnProviderBase>().enabled = true;
            }
        }

        /// <summary>
        /// If safe mode is ON, enable snap turn and disable below-terrian travel. If safe mode is OFF, enable continuous turn and free travel
        /// </summary>
        /// <param name="isOn"></param>
        private void SafeMode(bool isOn)
        {
            xrRig.GetComponent<SnapTurnProviderBase>().enabled = isOn;
            xrRig.GetComponent<ContinuousTurnProviderBase>().enabled = !isOn;
            
            // TODO: write code to prevent user from going below the terrain when safe mode is on
        }

        #region GettersAndSetters
        
        private float GetPlatformSpeed()
        {
            return platformSpeed;
        }

        private float GetMouseSensitivity()
        {
            return mouseSen;
        }

        private void SetPlatformSpeed(float speed)
        {
            platformSpeed = speed;
        }

        private void SetMouseRotation(float speed)
        {
            mouseSen = speed;
        }

        /// <summary>
        /// Sets turn speed of VR player. Min = 20, Max = 80
        /// </summary>
        /// <param name="speed"></param>
        private void SetVRRotateSpeed(float speed)
        {
            vrPlayer.GetComponent<ContinuousTurnProviderBase>().turnSpeed = speed;
        }

        #endregion
    }

}