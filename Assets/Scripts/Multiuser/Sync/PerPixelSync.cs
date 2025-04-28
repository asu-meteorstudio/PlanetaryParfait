using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using Unity.Netcode;
using TerrainEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using UnityEngine.Networking;
using UserInterface;

namespace Multiuser.Sync
{

    /// <summary>
    /// Located on the pin prefab object. Synchronizes the pin number and values between host and client using NetworkVariables.S
    /// </summary>
    public class PerPixelSync : CustomUnnamedMessageHandler<string>
    {
        //using network variables to track theses values
        public NetworkVariable<FixedString32Bytes> pinNumber = new NetworkVariable<FixedString32Bytes>(default,
            NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

        public NetworkVariable<FixedString512Bytes> pinData = new NetworkVariable<FixedString512Bytes>(default,
            NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);


        /// <summary>
        /// Called when a network opens or when a new pin is spawned into server (EX: when host opens a server, client joins room)
        /// </summary>
        public override void OnNetworkSpawn()
        {
            if (!IsHost)
            {
                this.transform.GetChild(0).transform.GetChild(0).GetChild(0).GetComponent<TMP_Text>().text =
                    "Pin #" + pinNumber.Value;
                this.transform.GetChild(0).transform.GetChild(1).GetComponent<TMP_Text>().text =
                    pinData.Value.ToString();
            }
            
            base.OnNetworkSpawn();
        }
    }

}