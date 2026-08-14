using System;
using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using TerrainEngine.Tools;
using UserInterface;

namespace Multiuser
{
    /// <summary>
    /// Main script for creating/joining a multiuser server. 
    /// </summary>
    public class MultiplayerManager : MonoBehaviour
    {
        public static MultiplayerManager Instance { get; private set; } //Singleton instance
        public string joinCode;
        public string playerName = "";
        public bool voluntaryDisconnect = false;

        public VoiceChat voiceChat;
        [SerializeField] private bool developerMode;

        private void Awake() 
        {
            Instance = this;
            joinCode = "";
            playerName = "Player" + UnityEngine.Random.Range(10, 99);
            voiceChat = FindObjectOfType<VoiceChat>(); // TODO: instantiate on session start, destroy on session end
        }
        
        /// <summary>
        /// Creates a Relay when user opens a Lobby. 
        /// </summary>
        public async void CreateRelay()
        {
            voluntaryDisconnect = false;
            PerPixelDataReader.singleton.DisablePins();
            NomenclatureDataReader.singleton.DisablePins();
            LoadingBar.OpenMenu(true);

            try
            {
                LoadingBar.Loading(0.25f, "Creating Server Data");
                // Create an allocation on the relay
                Allocation allocation =
                    await RelayService.Instance.CreateAllocationAsync(9); // max connections, preferred region
                RelayServerData relayServerData = new RelayServerData(allocation, "udp");

                // Generate room code for relay server
                joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

                LoadingBar.Loading(0.25f, "Pinging Server");

                // Access NetworkManager and send data to the NetworkManager's UnityTransport
                NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);
                NetworkManager.Singleton.StartHost();

                LoadingBar.Loading(0.25f, "Configuring Host Data");
                
                if (LoadingBar.Abort)
                {
                    NetworkManager.Singleton.Shutdown();
                    LoadingBar.Abort = false;
                    return;
                }

                GameState.InMultiuser = true;
                GameState.InTerrain = true;
                PerPixelDataReader.singleton.EnablePins(); //turns pins back on once room has loaded
                NomenclatureDataReader.singleton.EnablePins();
                LoadingBar.Loading(0.25f, "Loading Host Data");
                
                if (LoadingBar.Abort) // TODO: attach listener to loading bar
                {
                    NetworkManager.Singleton.Shutdown();
                    GameState.InMultiuser = false;
                    LoadingBar.Abort = false;
                    return;
                }
                
                TerrainTools.SetRoomCode();
                MainMenu.OpenPrimaryMenus(false);
                MultiuserMenu.SetMultiplayerMenu();

                print("RELAY CODE " + joinCode);

                if (developerMode)
                    voiceChat.JoinTestChannelAsync();
                else 
                    voiceChat.JoinChannelAsync(joinCode);
            }
            catch (Exception e) //TODO: when (e is RelayServiceException || e is TimeoutException)
            {
                Debug.LogError(e);
                GameState.InMultiuser = false;

                PerPixelDataReader.singleton.EnablePins();
                NomenclatureDataReader.singleton.EnablePins();

                MultiuserMenu.TextMessage("Relay Error", "Unable to connect to Relay Servers. Please try again.");
            }
            
            LoadingBar.DoneLoading();
        }

        /// <summary>
        /// Let users join the server using the generated joinCode. Ran when a client wants to join the server
        /// </summary>
        /// <param name="roomJoinCode"></param>
        public async void JoinRelay(string roomJoinCode)
        {
            voluntaryDisconnect = false;
            Debug.Log("joining..");
            PerPixelDataReader.singleton.DisablePins();
            NomenclatureDataReader.singleton.DisablePins();
            LoadingBar.OpenMenu(true);
            try
            {
                LoadingBar.Loading(0.25f, "Pinging Server");

                joinCode = roomJoinCode.ToUpper();
                // Find relay server with that room code
                JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
                RelayServerData relayServerData = new RelayServerData(joinAllocation, "udp");

                LoadingBar.Loading(0.25f, "Loading Server Data");

                // Access NetworkManager, let them join the server
                NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);
                NetworkManager.Singleton.StartClient();
                Debug.Log("Client ID: " + NetworkManager.Singleton.LocalClientId);

                LoadingBar.Loading(0.25f, "Configuring Client Data");

                if (LoadingBar.Abort)
                {
                    NetworkManager.Singleton.Shutdown();
                    LoadingBar.Abort = false;
                    return;
                }

                GameState.InMultiuser = true;
                GameState.InTerrain = true;

                if (LoadingBar.Abort)
                {
                    NetworkManager.Singleton.Shutdown();
                    GameState.InMultiuser = false;
                    LoadingBar.Abort = false;
                    return;
                }

                TerrainTools.SetRoomCode();
                MultiuserMenu.SetMultiplayerMenu();

                if (developerMode)
                    voiceChat.JoinTestChannelAsync();
                else 
                    voiceChat.JoinChannelAsync(joinCode);
            }
            catch (RelayServiceException e)
            {
                Debug.LogError(e);

                GameState.InMultiuser = false;
                LoadingBar.DoneLoading();
                if (e.Reason.ToString().Equals("JoinCodeNotFound") ||
                    e.Reason.ToString()
                        .Equals("InvalidRequest")) //reason codes: https://docs.unity.com/ugs/en-us/packages/com.unity.services.relay/1.0/api/Unity.Services.Relay.RelayExceptionReason
                {
                    MultiuserMenu.TextMessage("Room Code Error", "Incorrect room code. Please try again.");
                }
                else
                {
                    MultiuserMenu.TextMessage("Relay Error",
                        "Unable to connect to Relay servers. Please try again.");
                }
            }
            catch (ArgumentNullException e)
            {
                Debug.LogError(e);
                
                GameState.InMultiuser = false;
                LoadingBar.DoneLoading();
                
                MultiuserMenu.TextMessage("Relay Code Error",
                    "Please enter a room code.");
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                
                GameState.InMultiuser = false;
                LoadingBar.DoneLoading();

                MultiuserMenu.TextMessage("Relay Error",
                    "Unable to connect to Relay servers. Please try again.");
            }
        }

        public void LeaveRelay()
        {
            if (developerMode)
                voiceChat.LeaveTestChannelAsync();
            else 
                voiceChat.LeaveChannelAsync(joinCode);
            
            voluntaryDisconnect = true;
            NetworkManager.Singleton.Shutdown();
        }
    }
}