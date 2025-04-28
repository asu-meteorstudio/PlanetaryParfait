using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace TerrainEngine.Tools
{
    /// <summary>
    /// Components for nomenclature objects. 
    /// </summary>
    public class Nomenclature : MonoBehaviour
    {
        [Header("Pin Objects")]
        public GameObject pin;
        public GameObject panel;
        public Vector3 position;
        
        [Header("Pin Components")]
        public Image panelImage;
        public TMP_Text panelText;
        public Material pinMarker;
        public Material cube;

        public void SetMaterials()
        {
            pinMarker = pin.transform.GetChild(0).GetComponent<MeshRenderer>().material;
            cube = pin.transform.GetChild(1).GetComponent<MeshRenderer>().material;
        }
        
        public void SetText(string text)
        {
            panelText.text = text;
        }
    }   
}
