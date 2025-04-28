using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

namespace UserInterface
{


    public class SwitchTutorialMenu : MonoBehaviour
    {
        [Header("GameObjects To Change")] public TMP_Text bottomText;
        public VideoPlayer videoPlayer;

        [Header("Desktop")] 
        public string desktopText;
        public VideoClip desktopVideo;

        [Header("VR")] 
        public string vrText;
        public VideoClip vrVideo;

        // Start is called before the first frame update
        void Start()
        {
            if (GameState.IsVR)
            {
                bottomText.text = vrText;
                videoPlayer.clip = vrVideo;
            }
            else if (!GameState.IsVR)
            {
                bottomText.text = desktopText;
                videoPlayer.clip = desktopVideo;
            }
        }
    }
}