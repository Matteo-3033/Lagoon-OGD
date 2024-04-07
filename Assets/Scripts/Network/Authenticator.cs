using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

namespace Network
{
    public class Authenticator : NetworkAuthenticator
    {
        private readonly HashSet<NetworkConnection> connectionsPendingDisconnect = new();
        private readonly HashSet<string> playerNames = new();
        private string username;

        private struct AuthRequestMessage : NetworkMessage
        {
            public string Username;
        }

        public struct AuthResponseMessage : NetworkMessage
        {
            public bool Ok;
        }

        public override void OnStartServer()
        {
            NetworkServer.RegisterHandler<AuthRequestMessage>(OnAuthentication, false);
        }
        
        public override void OnStopServer()
        {
            // unregister the handler for the authentication request
            NetworkServer.UnregisterHandler<AuthRequestMessage>();
        }
        
        private void OnAuthentication(NetworkConnectionToClient conn, AuthRequestMessage msg)
        {
            Debug.Log($"Authentication Request: {msg.Username}");

            if (connectionsPendingDisconnect.Contains(conn)) return;

            if (!string.IsNullOrEmpty(msg.Username) && !playerNames.Contains(msg.Username))
            {
                Debug.Log("Authentication successful");
                playerNames.Add(msg.Username);

                conn.authenticationData = msg.Username;

                var authResponseMessage = new AuthResponseMessage
                {
                    Ok = true
                };
                conn.Send(authResponseMessage);

                ServerAccept(conn);
            }
            else
            {
                Debug.Log("Authentication failed");
                connectionsPendingDisconnect.Add(conn);
                
                var authResponseMessage = new AuthResponseMessage
                {
                    Ok = false
                };
                conn.Send(authResponseMessage);

                conn.isAuthenticated = false;

                // disconnect the client after 1 second so that response message gets delivered
                StartCoroutine(DelayedDisconnect(conn, 1f));
            }
        }

        public void OnPlayerDisconnected(Player player)
        {
            playerNames.Remove(player.username);
        }
        
        private IEnumerator DelayedDisconnect(NetworkConnectionToClient conn, float waitTime)
        {
            yield return new WaitForSeconds(waitTime);

            ServerReject(conn);

            yield return null;

            connectionsPendingDisconnect.Remove(conn);
        }
        
        
        
        
        public override void OnStartClient()
        {
            NetworkClient.RegisterHandler<AuthResponseMessage>(OnAuthResponseMessage, false);
        }
        
        public override void OnStopClient()
        {
            NetworkClient.UnregisterHandler<AuthResponseMessage>();
        }


        public override void OnClientAuthenticate()
        {
            NetworkClient.Send(new AuthRequestMessage { Username = username });
        }


        private void OnAuthResponseMessage(AuthResponseMessage msg)
        {
            if (msg.Ok)
            {
                Debug.Log($"Authentication success");
                
                ClientAccept();
            }
            else
            {
                Debug.LogError($"Authentication failed");

                NetworkManager.singleton.StopHost();
            }
        }

        public void SetUsername(string playerName)
        {
            username = playerName;
        }
    }
}