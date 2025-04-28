using System;
using System.Collections;
using System.Collections.Generic;
using TerrainEngine;
using Unity.Collections;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;

namespace Multiuser.Sync
{
    public class LayerSync : CustomUnnamedMessageHandler<string>
    {
        public static LayerSync singleton;

        private float layerTransparency;

        private void Awake()
        {
            singleton = this;
        }

        protected override byte MessageType()
        {
            return 3; // layer 
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            if (IsHost)
            {
                NetworkManager.OnClientConnectedCallback += OnClientConnectedCallback;
            }
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
            if (IsHost)
            {
                NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientConnectedCallback;
            }
        }

        private void OnClientConnectedCallback(ulong clientID)
        {
            //make sure all current layer transparency is reflected on the terrain upon client spawn
        }

        protected override void OnReceivedUnnamedMessage(ulong clientID, FastBufferReader reader)
        {
            if (MessageType() == 3) //layer message
            {
                var stringMessage = string.Empty;
                reader.ReadValueSafe(out stringMessage);

                if (IsHost)
                {
                    Debug.Log($"Host received unnamed message of type {MessageType()} from client {clientID} that contained the string: {stringMessage}");
                }
                else //client
                {
                    Debug.Log($"Client received unnamed message of type {MessageType()} from client {clientID} that contained the string: {stringMessage}");
                    var num = -1.0f; //slider value, default = -1

                    if (float.TryParse(stringMessage, out num)) //message is a floating point number
                    {
                        layerTransparency = num;
                    }
                    else
                    {
                        if (stringMessage.Equals("Exaggeration"))
                        {
                            SceneMaterializer.singleton.exaggerationSlider.value = layerTransparency;
                        }
                        else
                        {
                            foreach (var layer in SceneMaterializer.singleton.selectedScene.layers)
                            {
                                if (stringMessage.Equals(layer.layer_name))
                                {
                                    layer.transparency = layerTransparency;
                                    layer.slider.value = layerTransparency;
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dataToSend"></param>
        public override void SendUnnamedMessage(string dataToSend)
        {
            //need to send layer transparency across the network. In order to do that, we need the scene, the layer, and the slider value
            var writer = new FastBufferWriter(1100, Allocator.Temp);
            var customMessagingManager = NetworkManager.CustomMessagingManager;

            using (writer)
            {
                writer.WriteValueSafe(MessageType());
                writer.WriteValueSafe(dataToSend);

                if (IsServer)
                {
                    customMessagingManager.SendUnnamedMessageToAll(writer);
                }
                else //client
                {
                    customMessagingManager.SendUnnamedMessage(NetworkManager.ServerClientId, writer);
                }
            }
        }
    }
}
