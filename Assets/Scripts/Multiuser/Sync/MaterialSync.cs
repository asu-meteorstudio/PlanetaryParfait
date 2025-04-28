using UnityEngine;
using Unity.Netcode;
using Unity.Collections;
using TerrainEngine;
using TerrainEngine.Tools;
using UnityEngine.Serialization;
using UserInterface;

namespace Multiuser.Sync
{

    /// <summary>
    /// This class syncronizes the terrain scene object between hosts and clients by sending the terrain jsonURL as an unnamed message
    /// and downloading that terrain on the client's machine.
    /// </summary>
    public class MaterialSync : CustomUnnamedMessageHandler<string>
    {
        public string terrainURL;

        [FormerlySerializedAs("ceres")] [Header("Sample Terrain Objects")] 
        public GameObject sampleTerrain01;
        public GameObject sampleTerrain02;
        public GameObject sampleTerrain03;
        public GameObject sampleTerrain04;

        private void Update()
        {
            //dynamically updates terrain in a multiuser session
            if (IsHost)
            {
                if ((terrainURL != SceneDownloader.singleton.terrainURL) && GameState.InMultiuser)
                {
                    terrainURL = SceneDownloader.singleton.terrainURL;
                    SendUnnamedMessage(terrainURL);
                    //Debug.Log("Host is updating terrain");

                    // remove all pins
                    PerPixelDataReader.singleton.RemoveOldPins();
                }
            }
        }

        /// <summary>
        /// Override method that defines the unnamed message
        /// </summary>
        /// <returns></returns>
        protected override byte MessageType()
        {
            return 1; // 1 = scene JSON
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            if (IsHost)
            {
                terrainURL = SceneDownloader.singleton.terrainURL;
                NetworkManager.OnClientConnectedCallback += OnClientConnectedCallback;
            }
            /*else
            {
                SendUnnamedMessage(terrainURL);
            }*/
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
            if (IsHost)
            {
                NetworkManager.OnClientDisconnectCallback -= OnClientConnectedCallback;
            }

        }

        /// <summary>
        /// When a client connects, the host will send an unnamed message to that client containing the host's current terrainURL
        /// </summary>
        /// <param name="clientID"></param>
        private void OnClientConnectedCallback(ulong clientID)
        {
            SendUnnamedMessage(terrainURL);
        }

        /// <summary>
        /// Determines what happens with the message once it's recieved by the host/client
        /// </summary>
        /// <param name="clientID"></param>
        /// <param name="reader"></param>
        protected override void OnReceivedUnnamedMessage(ulong clientID, FastBufferReader reader)
        {
            Debug.Log("message type = " + this.MessageType());

            if (this.MessageType() == 1)
            {
                var stringMessage = string.Empty;
                reader.ReadValueSafe(out stringMessage);

                if (IsHost)
                {
                    Debug.Log($"Host received unnamed message of type ({MessageType()}) from client " +
                              $"({clientID}) that contained the string: {stringMessage}");
                }
                else
                {
                    Debug.Log("Client message = " + stringMessage);
                    SceneDownloader.singleton.terrainURL = stringMessage;

                    //checks for sample terrain URLs for faster terrain changes
                    if (stringMessage == sampleTerrain01.GetComponent<DataPackBehaviour>().dataPack.jsonURL)
                    {
                        sampleTerrain01.GetComponent<DataPackBehaviour>().LoadData();
                    }
                    else if (stringMessage == sampleTerrain03.GetComponent<DataPackBehaviour>().dataPack.jsonURL)
                    {
                        sampleTerrain03.GetComponent<DataPackBehaviour>().LoadData();
                    }
                    else if (stringMessage == sampleTerrain02.GetComponent<DataPackBehaviour>().dataPack.jsonURL)
                    {
                        sampleTerrain02.GetComponent<DataPackBehaviour>().LoadData();
                    }
                    else if (stringMessage == sampleTerrain04.GetComponent<DataPackBehaviour>().dataPack.jsonURL)
                    {
                        sampleTerrain04.GetComponent<DataPackBehaviour>().LoadData();
                    }
                    else //custom terrain
                    {
                        StartCoroutine(SceneDownloader.singleton.ChangeState(SceneDownloader.SceneSession.DOWNLOADING));
                    }
                    
                    //only enters terrain and opens client menu once terrain is done syncing
                    MainMenu.OpenPrimaryMenus(false);
                    LoadingBar.DoneLoading();
                    TerrainTools.SetClientUI(true);
                }
            }
        }


        public override void SendUnnamedMessage(string dataToSend)
        {
            var writer = new FastBufferWriter(1100, Allocator.Temp);
            var customMessagingManager = NetworkManager.CustomMessagingManager;

            using (writer)
            {
                writer.WriteValueSafe(MessageType()); //message type

                writer.WriteValueSafe(dataToSend); //writing string message

                if (IsServer)
                {
                    customMessagingManager.SendUnnamedMessageToAll(writer);
                }
                else
                {
                    customMessagingManager.SendUnnamedMessage(NetworkManager.ServerClientId, writer);
                }
            }
        }

    }

    public class CustomUnnamedMessageHandler<T> : NetworkBehaviour
    {
        //creates a unique identifier for unnamed messages
        protected virtual byte MessageType()
        {
            return 0;
            //0 = default
            //1 = string
        }

        public override void OnNetworkSpawn()
        {
            NetworkManager.CustomMessagingManager.OnUnnamedMessage += ReceiveMessage;
        }

        public override void OnNetworkDespawn()
        {
            NetworkManager.CustomMessagingManager.OnUnnamedMessage -= ReceiveMessage;
        }

        /// <summary>
        /// Receives unnamed message across the network through a Buffer Reader
        /// </summary>
        /// <param name="clientID"></param>
        /// <param name="reader"></param>
        protected virtual void OnReceivedUnnamedMessage(ulong clientID, FastBufferReader reader)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="clientID"></param>
        /// <param name="reader"></param>
        private void ReceiveMessage(ulong clientID, FastBufferReader reader)
        {
            var messageType = (byte)0;
            reader.ReadValueSafe(out messageType);

            if (messageType == MessageType())
            {
                OnReceivedUnnamedMessage(clientID, reader);
            }
        }

        /// <summary>
        /// Unnamed message to be sent across the network by the Host, Server, or Client
        /// </summary>
        /// <param name="dataToSend"></param>
        public virtual void SendUnnamedMessage(T dataToSend)
        {
        }
    }

}