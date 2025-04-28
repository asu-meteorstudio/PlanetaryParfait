using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UserInterface;

namespace TerrainEngine.Tools
{
    /// <summary>
    /// [FEATURE ON HOLD] Designed for writing notes on per-pixel pins. 
    /// </summary>
    public class PinInfo : MonoBehaviour
    {
        public void OpenVRKeyboard(GameObject inputField)
        {
            KeyboardController.Instance.OpenVRKeyboard(inputField.GetComponent<TMP_InputField>());
        }

        public void CloseVRKeyboard(GameObject inputField)
        {
            KeyboardController.Instance.CloseVRKeyboard();
        }
    }
}