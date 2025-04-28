using System;
using System.Collections;
using System.Collections.Generic;
using SpaceBear.VRUI;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.Serialization;
using XRControls;

namespace UserInterface
{
    public class Menu : MonoBehaviour
    {
        public delegate void MenuDelegate();
        public delegate void TextDelegate(string header, string text);
        public delegate void ToggleDelegate(bool active);
        public static ToggleDelegate LockCursor { get; private set; }

       public GameObject parentObject;
       public GameObject player;
       [HideInInspector] public VRUIKeyboard vrKeyboard;
       protected static Menu PreviousMenu;
       
        public virtual void Start()
        {
            LockCursor = ToggleCursorMode;
            player = GameObject.FindGameObjectWithTag("Player");
            SetListeners();
        }
        public virtual void ToggleMenu(bool active) { }
        
        public virtual void SetListeners() { }

        /// <summary>
        /// Locks and unlocks cursor to screen in Desktop
        /// </summary>
        /// <param name="locked">True to lock cursor to screen, false to unlock</param>
        private void ToggleCursorMode(bool locked)
        {
            Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !locked;
            player.GetComponent<FirstPersonController>().cursorLocked = locked;
        }

    }
}