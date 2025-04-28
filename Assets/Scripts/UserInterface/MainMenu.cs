using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UserInterface;

namespace UserInterface
{
    public class MainMenu : Menu
    {
        public static MainMenu Instance { get; private set; }
        public static ToggleDelegate OpenMenu { get; private set; }
        public static ToggleDelegate OpenPrimaryMenus { get; private set; }
        
        [Header("Main Menu GameObjects")]
        public GameObject PrimaryMenus;
        public Button sampleTerrainButton;
        public Button customTerrainButton;
        public Button multiuserButton;
        public Button exitToGameButton;
        public Button exitToDesktopButton;

        void Awake()
        {
            Instance = this;
            
            //setting delegates
            OpenMenu = ToggleMenu;
            OpenPrimaryMenus = TogglePrimaryMenu;
            
            //close all menus, start on main menu
            CloseAllMenus();
            PreviousMenu = this;
        }

        new void Start()
        {
            base.Start();
            
            // session starts on main menu
            TogglePrimaryMenu(true);
            ToggleMenu(true);
        }
        
        public override void ToggleMenu(bool active)
        {
            multiuserButton.gameObject.SetActive(!GameState.InMultiuser);
            parentObject.SetActive(active);
            GameState.InTerrain = false;
        }

        public override void SetListeners()
        {
            sampleTerrainButton.onClick.AddListener(delegate
            {
                ToggleMenu(false);
                SampleTerrainsMenu.OpenMenu(true);
            });
            
            customTerrainButton.onClick.AddListener(delegate
            {
                ToggleMenu(false);
                PreviousMenu = this;
                Login.TryLogin();
            });
            
            multiuserButton.onClick.AddListener(delegate
            {
                ToggleMenu(false);
                MultiuserMenu.OpenMenu(true);
            });

            
            exitToGameButton.onClick.AddListener(delegate
            {
                PreviousMenu = this;
                ToggleMenu(false);
                OpenPrimaryMenus(false);
                GameState.InTerrain = true;
            });
            
            exitToDesktopButton.onClick.AddListener(delegate
            {
                ToggleMenu(false);
                ConfirmQuit.Instance.ToggleMenu(true);
            });
        }

        public void CloseAllMenus()
        {
            for (int i = 0; i < PrimaryMenus.transform.childCount; i++)
            {
                PrimaryMenus.transform.GetChild(i).gameObject.SetActive(false);
            }
        }

        private void TogglePrimaryMenu(bool active)
        {
            PrimaryMenus.SetActive(active);
            TerrainTools.OpenMenu(!active);
            if(!GameState.IsVR) LockCursor.Invoke(!active);
        }
    }
}
