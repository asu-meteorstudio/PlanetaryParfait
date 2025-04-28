using System.Collections;
using System.Collections.Generic;
using Multiuser;
using TMPro;
using Unity.Netcode;
//using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.UI;

namespace UserInterface
{
    public class MultiuserMenu : Menu
    {
        public static MultiuserMenu Instance { get; private set; }
        public static TextDelegate TextMessage { get; private set; }
        public static ToggleDelegate OpenMenu { get; private set; }
        public static MenuDelegate SetMultiplayerMenu { get; private set; }

        [Header("Button Menu Assets")]
        public GameObject buttonMenu;
        public TMP_Text buttonHeader;
        public Button topButton;
        public Button bottomButton;
        public Button backButtonMenu;

        [Header("Input Menu Assets")]
        public GameObject inputMenu;
        public TMP_InputField upperField;
        public TMP_InputField lowerField;
        public TMP_Text upperDescription;
        public TMP_Text lowerDescription;
        public TMP_Text instructions;
        public TMP_Text inputHeader;
        public TMP_Text notificationText;
        public Button enterButton;
        public Button backInputMenu;

        [Header("Error Menu Assets")] 
        public GameObject errorMenu;
        public TMP_Text errorHeader;
        public TMP_Text errorText;
        public Button errorButton;
        
        [HideInInspector] public bool canTab = false, canEnter = false, isHosting = false;

        void Awake()
        {
            Instance = this;
            
            OpenMenu = ToggleMenu;
            TextMessage = DeSpawnPlayer;
            SetMultiplayerMenu = InMultiuserMenu;
            CreateJoinRoom();
        }

        new void Start()
        {
            base.Start();
        }

        void Update()
        {
            TabNextLine();
        }
        
        public override void ToggleMenu(bool active)
        {
            parentObject.SetActive(active);
        }

        public override void SetListeners()
        {
            topButton.onClick.AddListener(delegate
            {
                // If player is in a multiuser room, top button is the "Leave Multiuser" button. Kicks player from multiuser room
                if (GameState.InMultiuser)
                {
                    NetworkManager.Singleton.Shutdown();
                    GameState.InMultiuser = false;
                    ToggleMenu(false);
                    
                    CreateJoinRoom(); //resets menu to create/join room buttons
                    TerrainTools.SetRoomCode(); //turn off room code
                    TerrainTools.SetClientUI(false); //resets client UI
                    MainMenu.OpenMenu(true);
                    
                    //resets username / room code
                    Login.username = ""; 
                    Login.password = "";
                }
                else
                {
                    CreateRoom();
                    buttonMenu.SetActive(false);
                    inputMenu.SetActive(true);
                }
            });
            
            bottomButton.onClick.AddListener(delegate
            {
                JoinRoom();
                buttonMenu.SetActive(false);
                inputMenu.SetActive(true);
            });

            //Error Menu Message
            errorButton.onClick.AddListener(delegate
            {
                errorMenu.SetActive(false);
                parentObject = buttonMenu;
                CreateJoinRoom(); //resets menu to create/join room
                MainMenu.OpenMenu(true);
            });
            
            upperField.onSelect.AddListener(delegate(string arg0)
            {
                if (isHosting) canEnter = true;
                else canTab = true;
                
                if (GameState.IsVR)
                {
                    vrKeyboard.gameObject.SetActive(true);
                    vrKeyboard.inputField = upperField;
                }
            });

            upperField.onDeselect.AddListener(delegate(string arg0)
            {
                if(isHosting) canEnter = false;
                canTab = false;
            });
            
            lowerField.onSelect.AddListener(delegate(string arg0)
            {
                canEnter = true;
                
                if (GameState.IsVR)
                {
                    vrKeyboard.gameObject.SetActive(true);
                    vrKeyboard.inputField = lowerField;
                }
            });
            
            lowerField.onDeselect.AddListener(delegate(string arg0) { canEnter = false; });
            
            //Back button listener
            backButtonMenu.onClick.AddListener(delegate
            {
                ToggleMenu(false); 
                if (GameState.InTerrain) MainMenu.OpenPrimaryMenus(false);
                else MainMenu.OpenMenu(true);
                if(GameState.IsVR) vrKeyboard.gameObject.SetActive(false);
            });
            
            //Input Menu Listeners
            backInputMenu.onClick.AddListener(delegate
            {
                inputMenu.SetActive(false);
                buttonMenu.SetActive(true);
                parentObject = buttonMenu;
                if(GameState.IsVR) vrKeyboard.gameObject.SetActive(false);
            });
        }

        private void CreateJoinRoom()
        {
            parentObject = buttonMenu;
            buttonHeader.text = "Multiuser";
            topButton.GetComponentInChildren<TMP_Text>().text = "Create Room";
            bottomButton.gameObject.SetActive(true);
            bottomButton.GetComponentInChildren<TMP_Text>().text = "Join Room";
            backButtonMenu.GetComponentInChildren<TMP_Text>().text = "Back to Main Menu";
        }

        private void CreateRoom()
        {
            isHosting = true;
            
            parentObject = inputMenu;
            inputHeader.text = "Create Room";
            upperDescription.SetText("Username:");
            upperField.text = "";
            lowerField.gameObject.SetActive(false);
            lowerDescription.gameObject.SetActive(false);
            enterButton.GetComponentInChildren<TMP_Text>().text = "Create Room";
            backInputMenu.GetComponentInChildren<TMP_Text>().text = "Back";
            
            PreviousMenu = this;
            enterButton.onClick.RemoveAllListeners();
            enterButton.onClick.AddListener(delegate
            {
                player.GetComponentInChildren<TMP_Text>().text = upperField.text; //sets username
                ToggleMenu(false);
                MultiplayerManager.Instance.CreateRelay();
                
                if(GameState.IsVR) vrKeyboard.gameObject.SetActive(false);
            });
        }

        private void JoinRoom()
        {
            isHosting = false;
            
            parentObject = inputMenu;
            inputHeader.text = "Join Room";
            upperDescription.SetText("Username:");
            upperField.text = "";
            lowerField.gameObject.SetActive(true);
            lowerDescription.gameObject.SetActive(true);
            lowerDescription.SetText("Room Code:");
            lowerField.text = "";
            enterButton.GetComponentInChildren<TMP_Text>().text = "Join Room";
            backInputMenu.GetComponentInChildren<TMP_Text>().text = "Back";
            
            PreviousMenu = this;
            enterButton.onClick.RemoveAllListeners();
            enterButton.onClick.AddListener(delegate
            {
                player.GetComponentInChildren<TMP_Text>().text = upperField.text; //sets username
                ToggleMenu(false);
                MultiplayerManager.Instance.JoinRelay(lowerField.text);
                
                if(GameState.IsVR) vrKeyboard.gameObject.SetActive(false);
            });
        }

        public void ErrorMessage(string errorType, string errorMessage)
        {
            parentObject = errorMenu; //sets parent menu to error menu
            errorHeader.text = errorType;
            errorText.text = errorMessage;
            
            MainMenu.OpenPrimaryMenus(true);
            ToggleMenu(true);
        }

        private void DeSpawnPlayer(string errorType, string errorMessage)
        {
            NetworkManager.Singleton.Shutdown();
            GameState.InMultiuser = false;
            
            TerrainTools.SetRoomCode(); //turn off room code
            parentObject = errorMenu; //sets parent menu to error menu
            errorHeader.text = errorType;
            errorText.text = errorMessage;
            
            MainMenu.OpenPrimaryMenus(true);
            ToggleMenu(true);
            TerrainTools.SetClientUI(false);
            
            //resets user/join code input fields
            Login.username = "";
            Login.password = "";
        }

        private void InMultiuserMenu()
        {
            parentObject = buttonMenu;
            buttonHeader.text = "Room Code: " + MultiplayerManager.Instance.joinCode;
            topButton.GetComponentInChildren<TMP_Text>().text = "Leave Multiuser";
            bottomButton.gameObject.SetActive(false);
            backButtonMenu.GetComponentInChildren<TMP_Text>().text = "Close Menu";
        }

        private void TabNextLine()
        {
            if (PreviousMenu.GetType() == typeof(MultiuserMenu))
            {
                if (canTab && Input.GetKeyDown(KeyCode.Tab))
                {
                    lowerField.Select();
                }

                if (canEnter && Input.GetKeyDown(KeyCode.Return))
                {
                    if (isHosting)
                    {
                        player.GetComponentInChildren<TMP_Text>().text = upperField.text; //sets username
                        ToggleMenu(false);
                        MultiplayerManager.Instance.CreateRelay();

                        if (GameState.IsVR) vrKeyboard.gameObject.SetActive(false);
                    }
                    else
                    {
                        player.GetComponentInChildren<TMP_Text>().text = upperField.text; //sets username
                        ToggleMenu(false);
                        MultiplayerManager.Instance.JoinRelay(lowerField.text);

                        if (GameState.IsVR) vrKeyboard.gameObject.SetActive(false);
                    }
                }
            }
        }
    }
}