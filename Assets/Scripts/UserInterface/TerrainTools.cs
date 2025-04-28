using Multiuser;
using TerrainEngine;
using TerrainEngine.Tools;
using TMPro;
using Unity.Netcode;
using UnityEngine;
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
        public Button perPixelButton;
        public Button multiplayerButton;
        public Button scaleBarButton;
        public Button helpButton;
        public Button layersButton;
        public Button terrainMenuButton;
        public Button resetPosition;
        public Button clearAllPins;
        public Button clearMyPins;
        public Button settingButton;
        
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
        }

        new void Start()
        {
            base.Start();
            
            RoomCodeUI(); //turn off room code UI on start
            
            if (GameState.IsVR)
            {
                tabImg.gameObject.SetActive(false);
                shiftImg.gameObject.SetActive(false);
                generalTip.text = "Pull Trigger to Interact with Toolbar";
            }
            else
            {
                generalTip.text = "Toggle Tab to Interact with Toolbar";
            }
        }

        void Update()
        {
            resetPosition.gameObject.SetActive(SceneMaterializer.singleton.terrain.transform.position !=
                                               SceneMaterializer.singleton.terrainStartingPosition);

            if(GameState.IsVR) InfoPanel.Panel.DimText(perPixelPanel.activeSelf || terrainLayers.activeSelf || scaleBarPanel.activeSelf || m_TutorialOpen);
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
                ToggleTerrainsPanel(!PreviousMenu.parentObject.activeSelf);
            });
            perPixelButton.onClick.AddListener(delegate
            {
                TogglePerPixelData(!perPixelPanel.activeSelf);
                ToggleScaleBar(false);
                colorPickerPanel.SetActive(false);
            });
            layersButton.onClick.AddListener(delegate
            {
                ToggleLayersPanel(!terrainLayers.activeSelf);
                ToggleScaleBar(false);
                colorPickerPanel.SetActive(false);
            });
            multiplayerButton.onClick.AddListener(delegate
            {
                ToggleMenu(false);
                MainMenu.OpenPrimaryMenus(true);
                MultiuserMenu.OpenMenu.Invoke(true);
                ToggleTab(false);
                generalTip.text = "";
            });
            scaleBarButton.onClick.AddListener(delegate
            {
                ToggleScaleBar(!scaleBarPanel.activeSelf);
                ToggleLayersPanel(false);
                TogglePerPixelData(false);
            });
            resetPosition.onClick.AddListener(delegate
            {
                SceneMaterializer.singleton.terrain.transform.position = SceneMaterializer.singleton.terrainStartingPosition;
            });
            
            helpButton.onClick.AddListener(delegate
            {
                if (!m_TutorialOpen)
                {
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
                ToggleMenu(false);
                ToggleTab(false);
                
                MainMenu.OpenPrimaryMenus(true);
                SettingsController.OpenMenu(true);
            });
        }
        
        #endregion
        
        #region METHODS
        
        private void TogglePerPixelData(bool active)
        {
            clearAllPins.gameObject.SetActive(GameState.InMultiuser && NetworkManager.Singleton.IsHost);
            PerPixelDataReader.singleton.readingData = active;
            perPixelPanel.SetActive(active);
        }

        private void ToggleLayersPanel(bool active)
        {
            terrainLayers.SetActive(active);
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
        }

        /// <summary>
        /// Called when desktop user pressed the "Shift" key.
        /// </summary>
        /// <param name="active">True when user is in the "shifted" state. False otherwise.</param>
        public void ToggleShift(bool active)
        {
            shiftImg.color = active ? Color.white: highlightColor;
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
            if (GameState.InMultiuser)
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
        
        #endregion
    }

}
