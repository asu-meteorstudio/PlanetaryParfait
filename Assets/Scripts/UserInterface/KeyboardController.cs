
using SpaceBear.VRUI;
using Spacebear;
using TMPro;
using UnityEngine;

namespace UserInterface{

    public class KeyboardController : MonoBehaviour
    {
        public static KeyboardController Instance;
        private GameObject vrKeyboard;

        private void Awake()
        {
            if (Instance)
            {
                Destroy(gameObject);
                return;
            }
            DontDestroyOnLoad(gameObject);
            Instance = this;
            vrKeyboard = gameObject;
        }
        

        public void OpenVRKeyboard(TMP_InputField vrKeyboardInputField)
        {
            if (GameState.IsVR || GameState.vrDebug)
            {
                vrKeyboard.SetActive(true);
                vrKeyboard.GetComponentInParent<VRUIKeyboard>().inputField = vrKeyboardInputField;
            }
        }
        
        public void CloseVRKeyboard()
        {
            if (vrKeyboard.activeSelf)
            {
                vrKeyboard.SetActive(false);
            }
        }
    }

}