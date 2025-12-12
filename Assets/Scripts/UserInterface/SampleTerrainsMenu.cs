using System.Collections;
using System.Collections.Generic;
using TerrainEngine;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UserInterface;

namespace UserInterface
{
    public class SampleTerrainsMenu : Menu
    {
        public static ToggleDelegate OpenMenu { get; private set; }
        public List<Button> terrainButtons;
        public Button exitToMenuButton;
        public Button exitToGameButton;
        public bool loadedSampleTerrain;

        [Header("Audio Cues")] public AudioSource buttonClick;
        
        void Awake()
        {
            OpenMenu = ToggleMenu;
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
            foreach (Button button in terrainButtons)
            {
                button.onClick.AddListener(delegate
                {
                    StartCoroutine(LoadTerrain(button.GetComponent<DataPackBehaviour>()));
                });
            }

            exitToMenuButton.onClick.AddListener(delegate
            {
                buttonClick.Play();
                ToggleMenu(false);
                MainMenu.OpenMenu(true);
            });
            
            exitToGameButton.onClick.AddListener(delegate
            {
                buttonClick.Play();
                PreviousMenu = this;
                ToggleMenu(false);
                MainMenu.OpenPrimaryMenus(false);
                GameState.InTerrain = true;
            });
        }

        private IEnumerator LoadTerrain(DataPackBehaviour datapack)
        {
            buttonClick.Play();
            PreviousMenu = this;
            yield return new WaitForSeconds(0.001f);
            
            
            ToggleMenu(false);
            MainMenu.OpenPrimaryMenus(false);
            datapack.LoadData();
            loadedSampleTerrain = true;
        }
    }
}