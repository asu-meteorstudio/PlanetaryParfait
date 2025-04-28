using System;
using System.Collections;
using System.Collections.Generic;
using TerrainEngine;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;


namespace UserInterface
{
    public class InfoPanel : MonoBehaviour
    {
        public static InfoPanel Panel;

        public GameObject panelObj;
        public TMP_Text terrainNameText; // terrain name
        public TMP_Text bodyText; // planet
        public TMP_Text dimensionText; // terrain dimensions
        public TMP_Text exagText; // exaggeration

        public Color dimmedColor, undimmedColor;
        
        private void Start()
        {
            Panel = this;
            if(!GameState.IsVR) TogglePanel(true); // always stays on in desktop
        }

        private void Update()
        {
            if(GameState.IsVR) InfoPanel.Panel.TogglePanel(GameState.InTerrain && !MainMenu.Instance.PrimaryMenus.activeSelf);
        }

        /// <summary>
        /// Updates data on info panel
        /// </summary>
        /// <param name="scene">Selected JMARS terrain.</param>
        public void UpdateInfo(JMARSScene scene)
        {
            terrainNameText.text = scene.name;
            bodyText.text = "Body: " + scene.body;
            
            var xdim = scene.dimension.Split("x")[0];
            var ydim = scene.dimension.Split("x")[1];
            var zdim = scene.dimension.Split("x")[2];

            float xxdim = Convert.ToSingle(xdim);
            float yydim = Convert.ToSingle(ydim);

            string dimStr = Math.Round(xxdim, 0) + scene.units + " X " + Math.Round(yydim, 0) + scene.units;
            dimensionText.text = "Dimensions: " + dimStr;

            exagText.text = "Exaggeration: " + Math.Round(SceneMaterializer.singleton.exaggerationSlider.value, 2) + "x";
        }

        /// <summary>
        /// Called on the exaggeration Slider's OnValueChanged() Event
        /// </summary>
        public void ChangeExaggeration(float value)
        {
            exagText.text = "Exaggeration: " + value + "x";
        }

        public void TogglePanel(bool active)
        {
            panelObj.SetActive(active);
        }

        /// <summary>
        /// Dims text on the info panel for VR users when they are using various tools on the toolbar.
        /// </summary>
        /// <param name="active">True if toolbar panels are open, false otherwise.</param>
        public void DimText(bool active)
        {
            terrainNameText.color = active ? dimmedColor : undimmedColor;
            bodyText.color = active ? dimmedColor : undimmedColor;
            dimensionText.color = active ? dimmedColor : undimmedColor;
            exagText.color = active ? dimmedColor : undimmedColor;
        }
    }

}