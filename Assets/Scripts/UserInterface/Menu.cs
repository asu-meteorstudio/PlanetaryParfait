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
        public static ToggleDelegate HoverMenu { get; private set; }

        public GameObject parentObject;
        public GameObject player;
        public Transform menu;
        [HideInInspector] public VRUIKeyboard vrKeyboard;
        protected static Menu PreviousMenu;

        void Awake()
        {
            if (GameState.IsVR)
            {
                menu = transform.GetChild(0);
                ToggleHoveringMenu(false);
            }
        }
        public virtual void Start()
        {
            LockCursor = ToggleCursorMode;
            HoverMenu = ToggleHoveringMenu;
            player = GameObject.FindGameObjectWithTag("Player");
            SetListeners();
        }
        public virtual void ToggleMenu(bool active) { }
        
        public virtual void SetListeners() { }

        /// <summary>
        /// Makes the primary menus hover in front of the user in VR when enabled, otherwise the menus are next to the left controller.
        /// </summary>
        /// <param name="enabled">True to make menu hover, false to have it next to left controller</param>
        public void ToggleHoveringMenu(bool enabled)
        {
            XRController xrRig = (XRController)GameObject.FindGameObjectWithTag("Player").GetComponent(typeof(XRController));
            if (enabled)
            {
                menu.parent = xrRig.player.transform.GetChild(0).transform;
                menu.position = menu.parent.position + menu.parent.forward;
                menu.LookAt(menu.parent.position);
                menu.Rotate(new Vector3(0.0f, 180.0f, 0.0f));
                menu.localScale = Vector3.one * 0.2f;
            }
            else
            {
                menu.parent = xrRig.leftController.transform;
                menu.position = menu.parent.position - menu.parent.right * 0.25f;
                menu.rotation = menu.parent.rotation;
                menu.Rotate(new Vector3(52.825f, 180.0f, 0.0f));
                menu.localScale = Vector3.one * 0.075f;
            }
        }

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