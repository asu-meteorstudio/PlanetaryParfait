using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.Collections;
using TMPro;

// NetworkVariable synchronizes a property ("variable") between a server and client (s) without having to use custom messages or RPCs

namespace Multiuser
{

    public class Username : NetworkBehaviour
    {
        // We use this to send a player's username across the network. Synchronized with all players in network (already connected clients & late joining clients)
        public NetworkVariable<FixedString128Bytes> username = new NetworkVariable<FixedString128Bytes>(default,
            NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner); // read perm, write perm

        [SerializeField] private GameObject _localMesh;
        [SerializeField] private TMP_Text usernameText;

        /// <summary>
        /// Invoked on each NetworkBehaviour associated with a spawned NetworkObject 
        /// Since parent is dynamically spawned via Netcode, this method will run first BEFORE Start(). 
        /// </summary>
        public override void OnNetworkSpawn()
        {
            // find local player and change the text to the name
            _localMesh = GameObject.FindGameObjectWithTag("Player");
            if (IsLocalPlayer) username.Value = _localMesh.GetComponentInChildren<TMP_Text>().text; // updates username NetworkVariable to player's name
            usernameText.text = username.Value.ToString();

            // subscribes all connected clients to OnValueChanged(), which makes sure that when this variable updates, it is updated correctly across the network
            username.OnValueChanged += UpdateUsername;
        }

        void Start()
        {
            // makes sure client can write to NetworkVariable
            username.CanClientWrite(gameObject.GetComponentInParent<NetworkObject>().OwnerClientId);
        }

        /// <summary>
        /// Ran when a player leaves from experience
        /// </summary>
        public override void OnNetworkDespawn()
        {
            // Unsubscribes all connected clients to this instance of OnValueChanged() for this client 
            username.OnValueChanged -= UpdateUsername;
        }

        /// <summary>
        /// Updates a client's username
        /// </summary>
        /// <param name="previous"></param> Previous value
        /// <param name="current"></param> Current/New Value
        public void UpdateUsername(FixedString128Bytes previous, FixedString128Bytes current)
        {
            Debug.Log($"Client-{NetworkManager.LocalClientId}'s TextString = {username.Value}");
            this.gameObject.GetComponent<TMP_Text>().text = username.Value.ToString(); // updates NetworkVariable so it shows across network
        }
    }

}