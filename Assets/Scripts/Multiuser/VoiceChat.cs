using UnityEngine;
using Unity.Services.Vivox;
using System;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine.Android;

// TODO: update script with Vivox package update & integrate with multiuser logic
public class VoiceChat : MonoBehaviour
{
#if UNITY_ANDROID
    void Start()
    {
        if (Permission.HasUserAuthorizedPermission(Permission.Microphone) != true)
        {
            // request permission
            Permission.RequestUserPermission(Permission.Microphone);
            Debug.Log("permission requested");
        }
    }
#endif

    public async void StartVoiceChatAsync()
    {
        await UnityServices.InitializeAsync();

        if (!AuthenticationService.Instance.IsSignedIn)
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }
        
        await VivoxService.Instance.InitializeAsync();
        LoginAsync();
    }

    async void LoginAsync()
    {
        LoginOptions options = new LoginOptions
        {
            DisplayName = "sample display name",
            EnableTTS = true
        };
        await VivoxService.Instance.LoginAsync(options);
    }

    public async void JoinChannelAsync(string channel)
    {
        if (GameState.IsVR)
        {
            Channel3DProperties properties = new Channel3DProperties();
            await VivoxService.Instance.JoinPositionalChannelAsync(channel, ChatCapability.AudioOnly, properties);
        }
        else
        {
            await VivoxService.Instance.JoinGroupChannelAsync(channel, ChatCapability.AudioOnly);
        }
    }

    public async void LeaveChannelAsync(string channel)
    {
        await VivoxService.Instance.LeaveChannelAsync(channel);
    }

    public async void JoinTestChannelAsync()
    {
        string channelToJoin = "TestChannel";
        await VivoxService.Instance.JoinEchoChannelAsync(channelToJoin, ChatCapability.AudioOnly);
    }

    public async void LeaveTestChannelAsync()
    {
        string channelToLeave = "TestChannel";
        await VivoxService.Instance.LeaveChannelAsync(channelToLeave);
    }
}