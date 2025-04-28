using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UserInterface
{
    public class ConfirmQuit : Menu
    {
        public static ConfirmQuit Instance { get; set; }
        
        [Header("Buttons")]
        public Button returnButton;
        public Button quitButton;
        
        // Start is called before the first frame update
        new void Start()
        {
            Instance = this;
            
            base.Start();
        }

        public override void ToggleMenu(bool active)
        {
            parentObject.SetActive(active);
        }

        public override void SetListeners()
        {
            returnButton.onClick.AddListener(delegate
            {
                ToggleMenu(false);
                MainMenu.OpenMenu(true);
            });
            
            quitButton.onClick.AddListener(delegate
            {
                Application.Quit();
            });
        }
    }

}