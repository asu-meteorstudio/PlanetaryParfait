using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.Serialization;
using UserInterface;

namespace TerrainEngine.Tools
{
    /// <summary>
    /// Pin components for displaying per-pixel data. 
    /// </summary>
    public class Pin : NetworkBehaviour
    {
        [Header("Pin Objects")] public GameObject pin; //pin object
        public GameObject panel; //panel Prefab
        public TMP_Text pinNumber; //pin count
        public TMP_Text pinData; //pin data

        public string number = ""; // pin number in string form
        public string data = ""; //list of data

        [Header("Pin Location")] public Vector3 position;

        [Header("Multiuser")] 
        public ulong clientID = 0; // client who placed pin
        public string guid; //Holds players unique guid for individual deletion
    }
}