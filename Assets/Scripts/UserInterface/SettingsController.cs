using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.UIElements;
using UnityEngine.XR.Interaction.Toolkit;
using Slider = UnityEngine.UI.Slider;
using Toggle = UnityEngine.UI.Toggle;
using XRController = XRControls.XRController;
using Menu = UserInterface.Menu;

namespace UserInterface{

    public class SettingsController : Menu
    {
        public static ToggleDelegate OpenMenu;
        public delegate float SettingsDelegate();
        public static SettingsDelegate PlatformSpeed { get; private set; }
        public static SettingsDelegate MouseSensitivity { get; private set; }
        public static ToggleDelegate HoverMode { get; private set; }
        public static bool hoverMode;

        [SerializeField] private GameObject xrRig;
        
        [Header("Speed Controls")]
        public float platformSpeed = 5f;
        public float mouseSen = 1f;

        [Header("GameObjects")] 
        public GameObject safeModeSettings; // TODO: safe mode for desktop?
        public GameObject hoverMenuSettings;
        public Slider platformSlider;
        public GameObject rotateSlider;
        public GameObject mouseSlider;
        public UnityEngine.UI.Button exitButton;
        public XRController vrPlayer;

        [Header("Audio Cues")] public AudioSource buttonClick;

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
            var toggles = parentObject.GetComponentsInChildren<Toggle>();

            if (GameState.IsVR) // put into toggle listener
            {
                toggles[0].gameObject.SetActive(true);
                toggles[0].isOn = true;
                toggles[0].onValueChanged.AddListener(SafeMode);

                toggles[1].gameObject.SetActive(true);
                toggles[1].isOn = false;
                toggles[1].onValueChanged.AddListener(HoveringMenu);
                
                rotateSlider.gameObject.SetActive(true);
                rotateSlider.GetComponentInChildren<Slider>().onValueChanged.AddListener(SetVRRotateSpeed);

                mouseSlider.gameObject.SetActive(false);
            }
            else
            {
                toggles[0].gameObject.SetActive(false);
                toggles[1].gameObject.SetActive(false);
                rotateSlider.gameObject.SetActive(false);
                
                mouseSlider.gameObject.SetActive(true);
                mouseSlider.GetComponentInChildren<Slider>().onValueChanged.AddListener(SetMouseRotation);
            }
            
            platformSlider.onValueChanged.AddListener(SetPlatformSpeed);
            
            exitButton.onClick.AddListener(delegate
            {
                buttonClick.Play();
                ToggleMenu(false);
                MainMenu.OpenPrimaryMenus(false);
                TerrainTools.OpenMenu(true);
                GameState.InTerrain = true;
            });
        }
        
        private void SetDefaults()
        {
            vrPlayer.GetComponent<ContinuousTurnProviderBase>().turnSpeed = 50f;

            if (GameState.IsVR)
            {
                // continuous turn is off by default
                xrRig.GetComponent<SnapTurnProviderBase>().enabled = true;
                xrRig.GetComponent<ContinuousTurnProviderBase>().enabled = false;

                hoverMode = false;
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

        /// <summary>
        /// If enabled, primary menus will hover in front of VR camera. If disabled, all menus will be connected to the left controller.
        /// </summary>
        /// <param name="isOn"></param>
        private void HoveringMenu(bool isOn)
        {
            Menu.HoverMenu.Invoke(isOn);
            hoverMode = isOn;
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