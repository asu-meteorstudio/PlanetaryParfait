using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UserInterface
{

    /// <summary>
    /// This script is used to handle UI related to image layers.
    /// </summary>
    public class ChangeButtonImage : MonoBehaviour
    {
        public Toggle button;
        public Sprite image1;
        public Sprite image2;
        public bool usingImage1 = true;


        public void ChangeImage()
        {
            button.image.preserveAspect = true;
            if (usingImage1)
            {
                button.image.sprite = image2;
                usingImage1 = false;
            }
            else
            {
                button.image.sprite = image1;
                usingImage1 = true;
            }
        }
    }
}