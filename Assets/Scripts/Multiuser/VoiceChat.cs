using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Services.Vivox;
using System;
using VivoxUnity;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine.Android;

public class VoiceChat : MonoBehaviour
{
    public ILoginSession LoginSession; // the login session that owns this channel session
    public IChannelSession ChannelSession;
    public GameState _gameState;

    async void Start()
    {
        // await UnityServices.InitializeAsync();
        //await AuthenticationService.Instance.SignInAnonymouslyAsync();
        // InitializeVivox();

        _gameState = GameObject.FindObjectOfType<GameState>();
        Debug.Log("mic permission: " + Permission.Microphone);

        Debug.Log("Permission check : " + Permission.HasUserAuthorizedPermission(Permission.Microphone));

        if (Permission.HasUserAuthorizedPermission(Permission.Microphone) != true)
        {
            // request permission
            Permission.RequestUserPermission(Permission.Microphone);
            Debug.Log("permission requested");
        }

    }

    private void Awake()
    {
        //  Client _client = new Client();
        // _client.Initialize();
    }

    /// <summary>
    /// Initialize Vivox. This runs in MultiplayerManager script after user signs in anonymously 
    /// </summary>
    public void InitializeVivox()
    {
        VivoxService.Instance.Initialize();
        Debug.Log("Vivox Initialized");

    }

    /// <summary>
    /// Join a channel. Used whenever a user joins a server.
    /// </summary>
    /// <param name="channelName"></param>
    /// <param name="channelType"></param>
    /// <param name="connectAudio"></param>
    /// <param name="connectText"></param>
    /// <param name="transmissionSwitch"></param>
    /// <param name="properties"></param>
    public void JoinChannel(string channelName, ChannelType channelType, bool connectAudio, bool connectText, bool transmissionSwitch = true, Channel3DProperties properties = null)
    {
        if (LoginSession.State == LoginState.LoggedIn) // if user is logged in
        {
            Channel channel = new Channel(channelName, channelType, properties); // create new channel
            IChannelSession channelSession = LoginSession.GetChannelSession(channel); // get current channel session 

            // connect to channel
            channelSession.BeginConnect(connectAudio, connectText, transmissionSwitch, channelSession.GetConnectToken(), ar =>
            {
                try
                {
                    channelSession.EndConnect(ar);
                    Debug.Log("Joined Channel " + channel);
                }
                catch (Exception e)
                {
                    Debug.LogError($"Could not connect to channel: {e.Message}");
                    return;
                }
            });
        }
        else
        {
            Debug.LogError("Can't join a channel when not logged in.");
        }
    }


    /// <summary>
    /// User signs into Vivox when the game starts. Runs after the user is signed in asynchronously for Netcode services.
    /// </summary>
    public void Login(string displayName = null)
    {
        var account = new Account(displayName);
        bool connectAudio = true;
        bool connectText = true;

        LoginSession = VivoxService.Instance.Client.GetLoginSession(account); // get account session of connected user
        //LoginSession.PropertyChanged += LoginSession_PropertyChanged; // when LoginSession changes, call delegate to join channel

        // Logs user into Vivox using user's account / Access Token
        LoginSession.BeginLogin(LoginSession.GetLoginToken(), SubscriptionMode.Accept, null, null, null, ar =>
        {
            try
            {
                LoginSession.EndLogin(ar);
            }
            catch (Exception e)
            {
                // Unbind any login session-related events you might be subscribed to.
                // Handle error
                return;
            }
            // At this point, we have successfully requested to login. 
            // When you are able to join channels, LoginSession.State will be set to LoginState.LoggedIn.
            // Reference LoginSession_PropertyChanged()
            Debug.Log("Vivox Login Requested by user");
        });
    }

    public void JoinChannelMultiuser(string channelName)
    {
        if (LoginSession.State == LoginState.LoggedIn)
        {
            // User is logged in
            bool connectAudio = true;
            bool connectText = true;


            /*
            ChannelSession.BeginConnect(true, true, true, ChannelSession.GetConnectToken(), ar =>
            {
                Debug.Log("Trying to join");
            });
            */


            if (GameState.IsVR)
            {
                JoinChannel(channelName, ChannelType.Positional, connectAudio, connectText);

            }
            else
            {
                JoinChannel(channelName, ChannelType.NonPositional, connectAudio, connectText);
            }

            Debug.Log("User is joining channel " + channelName);
        }

    }


    public void LeaveChannel(ChannelId channelIdToLeave)
    {

        var channelSession = LoginSession.GetChannelSession(channelIdToLeave);
        if (channelSession != null)
        {
            // Disconnect from channel
            channelSession.Disconnect();
            Debug.Log("User has left channel " + channelIdToLeave);
            // (Optionally) deleting the channel entry from the channel list
            //LoginSession.DeleteChannelSession(channelIdToLeave);
        }

    }

}