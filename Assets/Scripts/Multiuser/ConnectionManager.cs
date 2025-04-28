using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using Unity.Netcode;
using UserInterface;

namespace Multiuser
{
    public class ConnectionManager : MonoBehaviour
    {
        public static ConnectionManager singleton;

        public enum ConnectionStatus
        {
            Connected,
            Disconnected
        }

        /// <summary>
        /// Invoked when a client connects or disconnects from the server
        /// </summary>
        public event Action<ulong, ConnectionStatus> OnClientConnectionNotification;

        void Awake()
        {
            if (singleton != null)
            {
                throw new Exception($"More than one instance of {nameof(ConnectionManager)}!");
            }

            singleton = this;
        }

        void Start()
        {
            if (singleton != this) return;

            if (NetworkManager.Singleton == null)
            {
                throw new Exception(
                    $"There is no {nameof(NetworkManager)} for the {nameof(ConnectionManager)} to interact with!");
            }

            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnectCallback;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnectCallback;
        }

        void OnDestroy()
        {
            if (NetworkManager.Singleton != null)
            {
                NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnectCallback;
                NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnectCallback;
            }
        }

        private void OnClientConnectCallback(ulong clientID)
        {
            OnClientConnectionNotification?.Invoke(clientID, ConnectionStatus.Connected);
            Debug.Log($"Joining client ID: {clientID} and local client ID: {NetworkManager.Singleton.LocalClientId}");
        }

        /// <summary>
        /// When a client disconnects, it sends this event to host and disconnected client
        /// </summary>
        /// <param name="clientID"></param>
        private void OnClientDisconnectCallback(ulong clientID)
        {
            OnClientConnectionNotification?.Invoke(clientID, ConnectionStatus.Disconnected);
            Debug.Log($"client {clientID} has disconnected");
            
            //if host disconnects, or if client network connection is lost, then disconnect client & return to main menu
            if (!NetworkManager.Singleton.IsHost) //despawns client that has connection issues
            {
                MultiuserMenu.TextMessage("Connection Error",
                    "Unable to maintain connection with the Relay Server. Return to singleuser and try again.");
            }
        }
    }

}