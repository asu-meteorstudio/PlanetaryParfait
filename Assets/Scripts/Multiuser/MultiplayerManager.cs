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

        //public VoiceChat _voiceChat;

        private void Awake() 
        {
            Instance = this;
            joinCode = "";

            // Unity Authentication. Sign user in anonymously if they aren't already signed in.
            SignInUserAnonymously();

            //_voiceChat = GameObject.FindObjectOfType<VoiceChat>();

            // Initialize Vivox Services -- Makes the Voice/Text chat work in Multiuser
            //_voiceChat.InitializeVivox();
            //_voiceChat.Login(); // login user into Vivox Services
        }

        /// <summary>
        /// Signs the user in anonymously from their local machine. This is a standard for Relay and Lobby that all users must be signed in before they can join a server. 
        /// /// </summary>
        private async void SignInUserAnonymously() // runs code asynchronously -- sends request to internet when request is made
        {
            try
            {
                // Initialize Unity services, pulls services from Unity Dashboard
                await UnityServices.InitializeAsync();
                AuthenticationService.Instance.SignedIn += () =>
                {
                    Debug.Log("Signed in " + AuthenticationService.Instance.PlayerId);
                };

                await AuthenticationService.Instance.SignInAnonymouslyAsync();

                // Update Player name
                playerName = "Player" + UnityEngine.Random.Range(10, 99);
            }
            //if there are exceptions, it keeps trying until user is authenticated
            catch(Exception e) when (e is AuthenticationException || e is RequestFailedException)
            {
                Debug.LogError(e);
                SignInUserAnonymously();
            }

        }

        /// <summary>
        /// Creates a Relay when user opens a Lobby. 
        /// </summary>
        public async void CreateRelay()
        {
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
                
                if (LoadingBar.Abort)
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

                // Join Vivox Channel -- Note: name of channel will be the Relay code for that particular server
                //_voiceChat.JoinChannelMultiuser(joinCode);

            }
            catch (Exception e) //when (e is RelayServiceException || e is TimeoutException)
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
            Debug.Log("joining..");
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

                // Join Vivox channel
                //_voiceChat.JoinChannelMultiuser(joinCode);
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

    }

}