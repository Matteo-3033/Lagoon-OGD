using System;
using System.Collections.Generic;
using Mirror;
using Network.Master;
using UnityEngine;
using Utils;

namespace Network
{
    public class MatchController : NetworkBehaviour
    {
        public static MatchController Instance { get; private set; }
        private const int MaxPlayers = 2;
        
        [SyncVar] private RoundConfiguration currentRound;
        [SyncVar] private int roundCnt;
        private readonly SyncList<RoundConfiguration> rounds = new();
        private readonly Dictionary<NetworkIdentity, string> players = new();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Debug.LogWarning("MatchController already exists in the scene. Deleting duplicate.");
                Destroy(gameObject);
                return;
            }
            
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public override void OnStartServer()
        {
            base.OnStartServer();
            Debug.Log("MatchController started on server");
            
            roundCnt = RiseNetworkManager.RoomOptions.CustomOptions.AsInt(RoomsModule.RoundsCntKey);
            rounds.AddRange(RiseNetworkManager.RoomOptions.CustomOptions.AsString(RoomsModule.RoundsKey).GetRounds());
            
            Debug.Log($"Round count: {roundCnt}");
            foreach (var r in rounds)
            {
                Debug.Log($"Round: {r.name}");
            }
            
            RiseNetworkManager.OnServerPlayerAdded += OnAddPlayer;
            RiseNetworkManager.OnServerReadied += OnPlayerReady;
            RiseNetworkManager.OnServerDisconnected += OnPlayerDisconnected;
        }
        
        [ServerCallback]
        private void OnPlayerReady(NetworkConnectionToClient conn)
        {
            // Client just connected, do nothing
            if (!conn.identity)
                return;
        }
        
        [ServerCallback]
        private void OnPlayerDisconnected(NetworkConnectionToClient obj)
        {
            if (!players.ContainsKey(obj.identity))
                return;
            
            var username = players[obj.identity];
            players.Remove(obj.identity);
        }
        
        [ServerCallback]
        private void OnAddPlayer(NetworkConnectionToClient conn, string username)
        {
            if (players.Count >= MaxPlayers)
            {
                Debug.LogError("Match is full");
                return;
            }
            
            Debug.Log($"Player {username} added to match");
            
            players[conn.identity] = username;

            if (players.Count == MaxPlayers)
                StartMatch();
        }

        private void StartMatch()
        {
            foreach (var identity in players.Keys)
            {
                var conn = identity.connectionToClient;
                if (conn == null)
                {
                    Debug.LogError("Connection is null");
                    continue;
                }
                
                var player = identity.gameObject;
                NetworkServer.AddPlayerForConnection(conn, player);
            }

            StartRound(0);
        }

        private void StartRound(int roundIndex)
        {
            if (roundIndex >= roundCnt)
            {
                Debug.Log("Match finished");
                return;
            }
            
            Debug.Log($"Starting round {roundIndex}");
            currentRound = rounds[roundIndex];
            
            RiseNetworkManager.singleton.ServerChangeScene(currentRound.scene);
        }
    }
}