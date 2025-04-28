using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace TerrainEngine.Tools
{
    /// <summary>
    /// Sends RPCs from pins between all players in a multiuser room.
    /// </summary>
    public class PinRPCS : NetworkBehaviour
    {
        /// <summary>
        /// Sends an RPC to place a pin for all clients in the multiuser server.
        /// </summary>
        /// <param name="position">Pin's world-space position</param>
        /// <param name="data">Pin Data</param>
        /// <param name="guid">ID of the client that placed the pin</param>
        /// <param name="serverRpcParams">RPC settings for the multiuser server</param>
        [ServerRpc(RequireOwnership = false)] // anyone can send this RPC
        public void PinServerRpc(Vector3 position, string data, string guid, ServerRpcParams serverRpcParams = default)
        {
            if (IsHost) // Only host can spawn pins, so client sends pin position and data to host so the pin spawns for everyone
            {
                PerPixelDataReader.singleton.SpawnPin(position, data, guid);
            }
        }

        /// <summary>
        /// Sends an RPC to destroy a pin for all clients in the multiuser server.
        /// </summary>
        /// <param name="guid">ID of the client that placed the pin</param>
        /// <param name="serverRpcParams">RPC settings for the multiuser server</param>
        [ServerRpc(RequireOwnership = false)]
        public void RemovePinServerRpc(string guid, ServerRpcParams serverRpcParams = default)
        {
            if (IsHost) // Remove pin from host's end
            {
                PerPixelDataReader.singleton.RemovePinsWithGuid(guid);
            }
        }
    }
}