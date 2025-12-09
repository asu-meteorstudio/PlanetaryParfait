using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Services.Analytics;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;
using Event = Unity.Services.Analytics.Event;

public class UnityServicesManager : MonoBehaviour
{
    async void Start()
    {
        await UnityServices.InitializeAsync();  // Initialize Unity services, pulls services from Unity Dashboard
        SignInUserAnonymously();
        AnalyticsService.Instance.StartDataCollection();
    }
    
    /// <summary>
    /// Signs the user in anonymously from their local machine. This is a standard for Relay and Lobby that all users must be signed in before they can join a server. 
    /// /// </summary>
    private async void SignInUserAnonymously() // runs code asynchronously -- sends request to internet when request is made
    {
        try
        {
            AuthenticationService.Instance.SignedIn += () =>
            {
                Debug.Log("Signed in " + AuthenticationService.Instance.PlayerId);
            };

            await AuthenticationService.Instance.SignInAnonymouslyAsync();

        }
        //if there are exceptions, it keeps trying until user is authenticated
        catch(Exception e) when (e is AuthenticationException || e is RequestFailedException)
        {
            Debug.LogError(e);
            SignInUserAnonymously();
        }
    }
}
