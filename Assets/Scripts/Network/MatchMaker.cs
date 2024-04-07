using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Mirror;
using Network.Messages;
using UnityEngine;
using MatchInfo = Network.Messages.MatchInfo;
using PlayerInfo = Network.Messages.PlayerInfo;

namespace Network
{
    // Server only script
    public class MatchMaker : NetworkBehaviour
    {

        [SerializeField] private MatchController matchControllerPrefab;
        
        private readonly Dictionary<string, MatchInfo> openMatches = new();
        private readonly Dictionary<string, HashSet<NetworkConnectionToClient>> matchConnections = new();
        private readonly Dictionary<NetworkConnection, PlayerInfo> playerInfos = new();
        
        public static MatchMaker Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Debug.LogWarning("MatchMaker: Instance already exists.");
                Destroy(gameObject);
                return;
            }
            
            Debug.Log("MatchMaker created");
        
            Instance = this;
            DontDestroyOnLoad(this);
            
            RiseNetworkManager.OnServerReadied += OnServerReadied;
            RiseNetworkManager.OnServerDisconnected += DisconnectPlayer;
        }

        private void OnServerReadied(NetworkConnectionToClient conn)
        {
            playerInfos.Add(conn, new PlayerInfo { username = conn.Username(), ready = false });
        }

        public bool SearchGame(NetworkConnectionToClient conn, out MatchInfo match, out PlayerInfo[] players)
        {
            if (!string.IsNullOrEmpty(playerInfos[conn].matchId))
            {
                match = new MatchInfo();
                players = new PlayerInfo[]{};
                return false;
            }

            match = openMatches.Count > 0 ? JoinMatch(conn) : CreateNewMatch(conn);
            players = matchConnections[match.matchId].Select(pConn => playerInfos[pConn]).ToArray();

            return true;
        }
        
        private MatchInfo JoinMatch(NetworkConnectionToClient conn)
        {
            var match = openMatches.Values.First();
            var matchId = match.matchId;
            
            match.players++;
            openMatches[match.matchId] = match;
            matchConnections[matchId].Add(conn);

            var playerInfo = playerInfos[conn];
            playerInfo.ready = false;
            playerInfo.matchId = matchId;
            playerInfos[conn] = playerInfo;
            
            Debug.Log($"Joined match {match.matchId}");
            return match;
        }

        private MatchInfo CreateNewMatch(NetworkConnectionToClient conn)
        {
            Debug.Log("Creating new match");
            
            var matchId = GetRandomMatchId();
            
            matchConnections.Add(matchId, new HashSet<NetworkConnectionToClient>());
            matchConnections[matchId].Add(conn);
            openMatches.Add(matchId, new MatchInfo { matchId = matchId, players = 1 });

            var playerInfo = playerInfos[conn];
            playerInfo.ready = false;
            playerInfo.matchId = matchId;
            playerInfos[conn] = playerInfo;
            
            Debug.Log($"Match {matchId} generated");
            
            return openMatches[matchId];
        }
        
        private static string GetRandomMatchId()
        {
            var id = string.Empty;
            
            for (var i = 0; i < 5; i++)
            {
                var random = UnityEngine.Random.Range(0, 36);
                if (random < 26)
                    id += (char) (random + 65);
                else
                    id += (random - 26).ToString();
            }
            
            return id;
        }

        public void SetReady(NetworkConnectionToClient conn, string matchId)
        {
            if (!matchConnections.ContainsKey(matchId) || !matchConnections[matchId].Contains(conn)) return;
            
            var player = playerInfos[conn];
            player.ready = true;
            playerInfos[conn] = player;

            if (openMatches[matchId].players != 2) return;
            if (matchConnections[matchId].Select(pConn => playerInfos[pConn]).Count(info => info.ready) == 2)
                StartCoroutine(BeginMatchDelayed(1, matchId));
        }
        
        private IEnumerator BeginMatchDelayed(float waitTime, string matchId)
        {
            if (!matchConnections.ContainsKey(matchId)) yield return null;
            yield return new WaitForSeconds(waitTime);
            
            var matchController = Instantiate(matchControllerPrefab);
            matchController.InitMatch(matchId);
            NetworkServer.Spawn(matchController.gameObject);

            foreach (var pConn in matchConnections[matchId])
            {
                pConn.Send(new ToClientMatchMessage { Op = ClientMatchOperation.Started, MatchId = matchId });

                var playerObj = Instantiate(NetworkManager.singleton.playerPrefab);
                var player = playerObj.GetComponent<Player>();
                player.SetMatch(matchController);
                NetworkServer.AddPlayerForConnection(pConn, playerObj);
                
                matchController.AddPlayer(playerObj.GetComponent<Player>());
                
                // Reset ready state for after the match
                var playerInfo = playerInfos[pConn];
                playerInfo.ready = false;
                playerInfos[pConn] = playerInfo;
            }
            
            openMatches.Remove(matchId);
            matchConnections.Remove(matchId);

            yield return null;
        }

        private void DisconnectPlayer(NetworkConnectionToClient conn)
        {
            if (!conn.identity) return;
            var player = conn.identity.GetComponent<Player>();
            if (!player || !player.match) return;
            
            Debug.Log ($"Player disconnected from match {player.match.matchId}");
            
            // TODO
        }
        
        public NetworkConnectionToClient OtherPlayerConn(string matchId, NetworkConnectionToClient conn)
        {
            return matchConnections[matchId].First(pConn => pConn.Username() != conn.Username());
        }
    }

    public static class MatchExtensions
    {
        public static Guid ToGuid(this string id)
        {
            var provider = new MD5CryptoServiceProvider();
            var inputBytes = Encoding.Default.GetBytes(id);
            var hashBytes = provider.ComputeHash(inputBytes);

            return new Guid(hashBytes);
        }
    }
}