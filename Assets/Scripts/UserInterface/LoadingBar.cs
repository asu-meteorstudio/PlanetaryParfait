using System;
using System.Collections;
using System.Collections.Generic;
using TerrainEngine;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

namespace UserInterface
{
    public class LoadingBar : Menu
    {
        public static LoadingBar Instance { get; private set; }
        public static ToggleDelegate OpenMenu { get; private set; }
        /// <summary>
        /// Loading Bar delegate handles all calls and updated to the loading bar menu
        /// </summary>
        public delegate void LoadingBarDelegate(float value = 0f, string text = "");
        public static LoadingBarDelegate Loading { get; private set; }
        public static LoadingBarDelegate DoneLoading { get; private set; }

        public static bool Abort;

        [Header("Loading Bar GOs")]
        public Slider loadingBar;
        public Button exitButton;
        public TMP_Text loadingText;
        public TMP_Text loadingPercent;
        
        [Header("Audio Cues")] 
        public AudioSource buttonClick;
        public AudioSource loadingSound;
        
                
        [Header("Error Handling Assets")] 
        public Button errorButton;

        void Awake()
        {
            Instance = this;
            
            OpenMenu = ToggleMenu;
            loadingBar.value = 0f;
            Loading = UpdateValue;
            DoneLoading = CloseMenu;
        }

        new void Start()
        {
            base.Start();
        }

        public override void ToggleMenu(bool active)
        {
            Abort = false; //set abort to false everytime the menu opens
            parentObject.SetActive(active);
        }

        public override void SetListeners()
        {
            exitButton.onClick.AddListener(delegate
            {
                buttonClick.Play();
                Abort = true;
                SceneDownloader.singleton.StopAllCoroutines();
                StartCoroutine(SceneDownloader.singleton.ChangeState(SceneDownloader.SceneSession.DONE));
                CloseMenu(0f, ""); //resets loading bar to zero
                MainMenu.Instance.CloseAllMenus();
                PreviousMenu.ToggleMenu(true);
                
            });
            
            errorButton.onClick.AddListener(delegate
            {
                buttonClick.Play();
                loadingText.color = Color.white;
                CloseMenu(0f, "");
                MainMenu.Instance.CloseAllMenus();
                PreviousMenu.ToggleMenu(true);
                errorButton.gameObject.SetActive(false);
            });
        }

        private void UpdateValue(float value, string text)
        {
            if (!loadingSound.isPlaying) loadingSound.Play();
            loadingBar.value += value;
            loadingPercent.text = Math.Round(loadingBar.value, 2) * 100 + "%";
            loadingText.text = text;
        }

        private void CloseMenu(float value, string text)
        {
            loadingSound.Stop();
            parentObject.SetActive(false);
            loadingBar.value = value;

            loadingPercent.text = value + "%";
            loadingText.text = text;
        }

        public void AbortLoading(string errorMessage)
        {
            loadingSound.Stop();
            loadingText.color = Color.red;
            loadingText.text = errorMessage;
            
            errorButton.gameObject.SetActive(true);
        }
    }
}