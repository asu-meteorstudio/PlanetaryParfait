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
        // network variables to assign on spawn 
        public NetworkVariable<NetworkObjectReference> pinNetworkReference = new NetworkVariable<NetworkObjectReference>();
        public NetworkVariable<Vector3> networkPosition = new NetworkVariable<Vector3>();

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

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            pin = null;
            
            // waits until pinNetworkReference is assigned
            StartCoroutine(WaitForNetworkReference());
        }

        public IEnumerator WaitForNetworkReference()
        {
            while (true){
                if (pinNetworkReference.Value.TryGet(out NetworkObject pinNetworkObject))
                {
                    // sets pin reference to corresponding spawned pin network object to allow pin movement with local users' terrain exaggeration
                    pin = pinNetworkObject.gameObject;
                    panel = this.gameObject;
                    position = networkPosition.Value;
                    yield break;
                }
                yield return null;
            }
        }
    }
}