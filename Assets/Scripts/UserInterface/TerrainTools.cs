using Multiuser;
using TerrainEngine;
using TerrainEngine.Tools;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Button = UnityEngine.UI.Button;

namespace UserInterface
{
    public class TerrainTools : Menu
    {
        
        #region EVENTS
        public static TerrainTools Instance { get; private set; }
        public static ToggleDelegate OpenMenu { get; private set; }
        public static ToggleDelegate SetClientUI { get; private set; }
        public static MenuDelegate SetRoomCode { get; private set; }
        public static TextDelegate DynamicReadout { get; private set; }
        
        #endregion

        #region VARIABLES
        
        [Header("Tab/Shift Icons")]
        public Image tabImg;
        public Image shiftImg;
        public Color highlightColor;
        
        [Header("Panels")] 
        public GameObject terrainLayers;
        public GameObject perPixelPanel;
        public GameObject leftSideTools;
        public GameObject colorPickerPanel;
        public GameObject scaleBarPanel;
        public GameObject roomCodePanel;
        public GameObject firstTutorial;
        
        [Header("Text Assets")]
        public TMP_Text perPixelHeader;
        public TMP_Text perPixelData;
        public TMP_Text roomCodeText;
        public TMP_Text generalTip;

        [Header("Buttons")] 
        public Button vrPP;
        public Button desktopPP;
        public Button vrMultiuser;
        public Button desktopMultiuser;
        public Button vrScalebar;
        public Button desktopScalebar;
        public Button vrHelp;
        public Button desktopHelp;
        public Button vrLayers;
        public Button desktopLayers;
        public Button vrTerrains;
        public Button desktopTerrains;
        public Button vrReset;
        public Button desktopReset;
        public Button vrSettings;
        public Button desktopSettings;

        private Button perPixelButton;
        private Button multiplayerButton;
        private Button scaleBarButton;
        private Button helpButton;
        private Button layersButton;
        private Button terrainMenuButton;
        private Button resetPosition;
        public Button clearAllPins;
        public Button clearMyPins;
        private Button settingButton;
        
        [Header("Audio Cues")] public AudioSource buttonClick;

        public GameObject desktopTools;
        public GameObject vrTools;

        // private vars
        private bool m_TutorialOpen;
        
        #endregion
        
        #region MONO
        
        void Awake()
        {
            Instance = this;
            
            // Assign delegates
            OpenMenu = ToggleMenu;
            SetClientUI = SetClientToolbar;
            SetRoomCode = RoomCodeUI;
            DynamicReadout = ReadData;

            vrTools.SetActive(GameState.IsVR);
            desktopTools.SetActive(!GameState.IsVR);
            
            perPixelButton = GameState.IsVR ? vrPP : desktopPP;
            multiplayerButton = GameState.IsVR ? vrMultiuser : desktopMultiuser;
            scaleBarButton = GameState.IsVR ? vrScalebar : desktopScalebar;
            helpButton = GameState.IsVR ? vrHelp : desktopHelp;
            layersButton = GameState.IsVR ? vrLayers : desktopLayers;
            terrainMenuButton = GameState.IsVR ? vrTerrains : desktopTerrains;
            resetPosition = GameState.IsVR ? vrReset : desktopReset;
            settingButton = GameState.IsVR ? vrSettings : desktopSettings;
        }

        new void Start()
        {
            base.Start();
            
            RoomCodeUI(); //turn off room code UI on start
            
            if (GameState.IsVR)
            {
                tabImg.gameObject.SetActive(false);
                shiftImg.gameObject.SetActive(false);
                //generalTip.text = "Pull Trigger to Interact with Toolbar";
            }
            else
            {
                generalTip.text = "Toggle Tab to Interact with Toolbar";
            }
        }

        void Update()
        {
            if (GameState.InMultiuser)
            {
                // shows reset platform button only for host
                resetPosition.gameObject.SetActive(SceneMaterializer.singleton.terrain.transform.position !=
                                                SceneMaterializer.singleton.terrainStartingPosition &&
                                                NetworkManager.Singleton.IsHost);
            }
            else
            {
                resetPosition.gameObject.SetActive(SceneMaterializer.singleton.terrain.transform.position !=
                                                SceneMaterializer.singleton.terrainStartingPosition);
            }
            
        }
        
        #endregion

        #region OVERRIDES

        public override void ToggleMenu(bool active)
        {
            parentObject.SetActive(active);
        }

        public override void SetListeners()
        {
            terrainMenuButton.onClick.AddListener(delegate
            {
                buttonClick.Play();
                ToggleTerrainsPanel(!PreviousMenu.parentObject.activeSelf);
                Deselect();  
            });
            perPixelButton.onClick.AddListener(delegate
            {
                buttonClick.Play();
                TogglePerPixelData(!perPixelPanel.activeSelf);
                ToggleScaleBar(false);
                if(GameState.IsVR) ToggleLayersPanel(false);
                Deselect();  
            });
            layersButton.onClick.AddListener(delegate
            {
                buttonClick.Play();
                ToggleLayersPanel(!terrainLayers.activeSelf);
                ToggleScaleBar(false);
                if(GameState.IsVR) TogglePerPixelData(false);
                Deselect();  
            });
            multiplayerButton.onClick.AddListener(delegate
            {
                buttonClick.Play();
                ToggleMenu(false);
                MainMenu.OpenPrimaryMenus(true);
                MultiuserMenu.OpenMenu.Invoke(true);
                ToggleTab(false);
                generalTip.text = "";
            });
            scaleBarButton.onClick.AddListener(delegate
            {
                buttonClick.Play();
                ToggleScaleBar(!scaleBarPanel.activeSelf);
                ToggleLayersPanel(false);
                TogglePerPixelData(false);
                Deselect();  
            });
            resetPosition.onClick.AddListener(delegate
            {
                buttonClick.Play();
                SceneMaterializer.singleton.terrain.transform.position = SceneMaterializer.singleton.terrainStartingPosition;
                Deselect();  
            });
            
            helpButton.onClick.AddListener(delegate
            {
                if (!m_TutorialOpen)
                {
                    buttonClick.Play();
                    firstTutorial.SetActive(true);
                    m_TutorialOpen = true;
                    
                    //turn off other panels 
                    ToggleScaleBar(false);
                    ToggleLayersPanel(false);
                    TogglePerPixelData(false);
                }
            });
            
            settingButton.onClick.AddListener(delegate
            {
                buttonClick.Play();
                ToggleMenu(false);
                ToggleTab(false);
                
                MainMenu.OpenPrimaryMenus(true);
                SettingsController.OpenMenu(true);
                Deselect();  
            });
        }
        
        #endregion
        
        #region METHODS
        
        private void TogglePerPixelData(bool active)
        {
            clearAllPins.gameObject.SetActive(GameState.InMultiuser && NetworkManager.Singleton.IsHost);
            PerPixelDataReader.singleton.readingData = active;
            perPixelPanel.SetActive(active);

            if (active) AnalyticsManager.perPixelTracker = new PerPixelUsageTracker();
            else
            {
                if (AnalyticsManager.perPixelTracker != null)
                {
                    AnalyticsManager.perPixelTracker.RecordEvent();
                    AnalyticsManager.perPixelTracker = null; // reset
                }
            }
        }

        private void ToggleLayersPanel(bool active)
        {
            terrainLayers.SetActive(active);

            if (active) AnalyticsManager.layersTracker = new LayersUsageTracker();
            else
            {
                if (AnalyticsManager.layersTracker != null)
                {
                    AnalyticsManager.layersTracker.RecordEvent();
                    AnalyticsManager.layersTracker = null; // reset
                }
            }
        }
        
        private void ToggleTerrainsPanel(bool active) 
        {
            MainMenu.OpenPrimaryMenus(active);
            
            //close other panels if terrains menu is open
            TogglePerPixelData(false);
            ToggleLayersPanel(false);
            ToggleScaleBar(false);
            
            generalTip.text = "";
            ToggleTab(false);
            
            if (PreviousMenu.GetType() != typeof(MultiuserMenu))
            {
                PreviousMenu.ToggleMenu(active);
            }
            else
            {
                MainMenu.OpenMenu(true);
            }
        }

        private void ToggleScaleBar(bool active)
        {
            ScaleBar.singleton.scalebarMode = active;
            scaleBarPanel.SetActive(active);
            colorPickerPanel.SetActive(false);

            if (active) AnalyticsManager.scalebarTracker = new ScalebarUsageTracker();
            else
            {
                if (AnalyticsManager.scalebarTracker != null)
                {
                    AnalyticsManager.scalebarTracker.RecordEvent();
                    AnalyticsManager.scalebarTracker = null;
                }
            }
        }
        
        
        /// <summary>
        /// Called from tutorial exit button to reset tutorial. 
        /// </summary>
        /// <param name="active"></param>
        public void TutorialActive(bool active)
        {
            m_TutorialOpen = active;
        }
        
        /// <summary>
        /// Called when desktop user presses the "Tab" key.
        /// </summary>
        /// <param name="active">True when user is in the "tabbed" state. False otherwise.</param>
        public void ToggleTab(bool active)
        {
            tabImg.color = active ? Color.white: highlightColor;
            generalTip.text = "Toggle Tab to Interact with Toolbar";
        }

        /// <summary>
        /// Called when desktop user pressed the "Shift" key.
        /// </summary>
        /// <param name="active">True when user is in the "shifted" state. False otherwise.</param>
        public void ToggleShift(bool active)
        {
            shiftImg.color = active ? Color.white: highlightColor;
            generalTip.text = "Toggle Shift to Move Platform";
        }

        /// <summary>
        /// Called from PerPixelDataReader.cs when PerPixel Panel is open for dynamic data readout.
        /// </summary>
        /// <param name="header">Per Pixel Layer Name</param>
        /// <param name="data">Per Pixel Value</param>
        private void ReadData(string header, string data)
        {
            perPixelHeader.text = header;
            perPixelData.text = data;
        }

        /// <summary>
        /// Restricts clients from using certain tools on the toolbar in a multiuser session.
        /// </summary>
        /// <param name="active">True if user is a client in a multiuser session, false otherwise.</param>
        private void SetClientToolbar(bool active)
        {
            clearAllPins.gameObject.SetActive(!active);
            terrainMenuButton.gameObject.SetActive(!active);
            resetPosition.gameObject.SetActive(!active);
        }

        /// <summary>
        /// Updates room code UI for users in a multiuser room.
        /// </summary>
        private void RoomCodeUI()
        {
            if (GameState.InMultiuser && !GameState.IsVR) //currently disable roomcode panel for VR users
            {
                roomCodePanel.SetActive(true);
                roomCodeText.text = "Room Code: " + MultiplayerManager.Instance.joinCode;
            }
            else
            {
                roomCodePanel.SetActive(false);
                roomCodeText.text = string.Empty;
            }
        }

        /// <summary>
        /// Removes current "selected" object so UI image changes (like changing the color of a button when hovering over it) can fire on the last selected object
        /// </summary>
        private void Deselect()
        {
            EventSystem.current.SetSelectedGameObject(null);
        }
        
        #endregion
    }

}
