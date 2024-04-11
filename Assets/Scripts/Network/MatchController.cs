using System;
using System.Collections.Generic;
using Mirror;
using Network.Master;
using UnityEngine;
using Utils;

namespace Network
{
    [Serializable]
    public struct MatchPlayerData
    {
        public string username;
        // TODO: add other match data
    }
    
    public class MatchController : NetworkBehaviour
    {
        public static MatchController Instance { get; private set; }
        private const int MaxPlayers = 2;
        
        [SyncVar] private RoundConfiguration currentRound;
        [SyncVar] private int roundCnt;
        private readonly SyncList<RoundConfiguration> rounds = new();
        private readonly SyncDictionary<string, MatchPlayerData> players = new();
        private readonly Dictionary<NetworkIdentity, string> connections = new();

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
            
            RiseNetworkManager.OnServerPlayerAdded += OnPlayerReady;
            RiseNetworkManager.OnServerDisconnected += OnPlayerDisconnected;
        }
        
        [ServerCallback]
        private void OnPlayerReady(NetworkConnectionToClient conn, string username)
        {
            if (!players.ContainsKey(username))
                AddPlayer(conn.identity, username);
        }
        
        [ServerCallback]
        private void OnPlayerDisconnected(NetworkConnectionToClient obj)
        {
            if (!connections.ContainsKey(obj.identity))
                return;
            
            var username = connections[obj.identity];
            connections.Remove(obj.identity);
            players.Remove(username);
        }
        
        [ServerCallback]
        private void AddPlayer(NetworkIdentity conn, string username)
        {
            if (connections.Count >= MaxPlayers)
            {
                Debug.LogError("Match is full");
                return;
            }
            
            Debug.Log($"Player {username} added to match");
            
            connections[conn] = username;
            players[username] = new MatchPlayerData {username = username};

            if (connections.Count == MaxPlayers)
                StartMatch();
        }

        private void StartMatch()
        {
            foreach (var identity in connections.Keys)
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