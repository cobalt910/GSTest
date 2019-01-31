using System;
using GameSparks.Api.Requests;
using GameSparks.Api.Responses;
using UnityEngine;

namespace NetworkFramework.RequestSystem
{
    public static class RegistrationSystem
    {
        public static void AuthenticateUser(string                         userName, string password,
                                            Action<RegistrationResponse>   regCallback,
                                            Action<AuthenticationResponse> authCallback)
        {
            // fill pregistration request data
            var regRequest = new RegistrationRequest();
            regRequest.SetDisplayName(userName);
            regRequest.SetUserName(userName);
            regRequest.SetPassword(password);

            regRequest.Send(regResponse =>
            {
                if (regResponse == null)
                {
                    Debug.Log("GSM| GameSparks non initialized.");
                }
                else if (!regResponse.HasErrors)
                {
                    Debug.Log("GSM| Registration Successful...");
                    regCallback?.Invoke(regResponse);
                }
                else
                {
                    if (regResponse.NewPlayer.GetValueOrDefault() == false)
                    {
                        var authRequest = new AuthenticationRequest();
                        authRequest.SetUserName(userName);
                        authRequest.SetPassword(password);

                        authRequest.Send(authResponse =>
                        {
                            if (authResponse.HasErrors == false)
                            {
                                Debug.Log("Authentication Successful...");
                                authCallback(authResponse);
                            }
                            else
                                Debug.LogWarning("GSM| Error Authenticating User \n" + authResponse.Errors.JSON);
                        });
                    }
                    else
                        Debug.LogWarning("GSM| Error Authenticating User \n" + regResponse.Errors.JSON);
                }
            });
        }
    }
}