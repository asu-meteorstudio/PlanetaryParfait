using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using XRControls;

namespace UserInterface
{
    public class ColorPicker : MonoBehaviour
    {
        public Image colorWheel;
        public RectTransform rt;

        public Image selectedColorGO;
        public Image currentColorGO;
        public Color selectedColor;
        public Material scaleBarMat;

        private Texture2D colorTexture;
        [SerializeField] private XRController controlCheck; // determines if we are pressing the button in VR
        [SerializeField] private GameObject vrController;

        private Ray ray;

        // Start is called before the first frame update
        void Start()
        {
            colorTexture = (Texture2D)colorWheel.mainTexture; // get the texture from the colorpicker image
            
            if(GameState.IsVR)
            {
                controlCheck = GameObject.FindGameObjectWithTag("Player").GetComponent<XRController>();
            }
        }

        private void Awake()
        {
            ResetColor(); //default starting color is white
        }

        private void OnDestroy()
        {
            ResetColor();
        }

        // Update is called once per frame
        void Update()
        {
            if (Input.GetMouseButtonDown(0) || controlCheck.triggerActive)
            {
                Vector2 localPoint;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(rt, GetPosition(), null, out localPoint);

                Vector2 texturePos = GetTextureCoords(localPoint);
                if (isValidPos(texturePos))
                {
                    selectedColor = colorTexture.GetPixel((int)texturePos.x, (int)texturePos.y);
                    if (selectedColor.a != 0)
                    {
                        selectedColorGO.color = selectedColor;
                        currentColorGO.color = selectedColor;
                        scaleBarMat.color = selectedColor;
                        scaleBarMat.SetColor("_EmissionColor", selectedColor);
                    }
                }
            }
        }

        /// <summary>
        /// Gets point position on RectTransform based on user input (VR or Desktop)
        /// </summary>
        private Vector3 GetPosition()
        {
            if (GameState.IsVR) //VR
            {
                ray.origin = vrController.transform.position;
                ray.direction = vrController.transform.forward;

                RaycastHit hit;
                if (Physics.Raycast(ray, out hit, Mathf.Infinity, 1 << 11))
                {
                    if (hit.collider.gameObject == colorWheel.gameObject)
                    {
                        return hit.point;
                    }
                }
            }
            else // desktop
            {
                return Input.mousePosition;
            }

            return Vector3.zero; // can't determine rig
        }

        /// <summary>
        /// Gets the texture coordinates from a point on a RectTransform
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        private Vector2 GetTextureCoords(Vector2 point)
        {
            float pivotOffsetX = (point.x + rt.rect.width * 0.5f) /
                                 rt.rect.width;
            float pivotOffsetY = (point.y + rt.rect.height * 0.5f) /
                                 rt.rect.height;

            float textureX = colorTexture.width * pivotOffsetX;
            float textureY = colorTexture.height * pivotOffsetY;

            return new Vector2(textureX, textureY);
        }

        private bool isValidPos(Vector2 pos)
        {
            return pos.x >= 0 && pos.x < colorTexture.width && pos.y >= 0 && pos.y < colorTexture.height;
        }

        public void ResetColor()
        {
            selectedColorGO.color = Color.white;
            currentColorGO.color = Color.white;
            scaleBarMat.color = Color.white;
            scaleBarMat.SetColor("_EmissionColor", Color.white);
        }
    }
}
