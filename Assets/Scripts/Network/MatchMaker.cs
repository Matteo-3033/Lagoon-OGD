using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using Mirror;
using UnityEngine;

namespace Network
{
    // Server only script
    public class MatchMaker : NetworkBehaviour
    {

        [SerializeField] private MatchController matchControllerPrefab;
        
        public static MatchMaker Instance { get; private set; }

        private readonly Dictionary<string, MatchController> matches = new();

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
        }

        public bool SearchGame(Player player, out MatchController match)
        {
            foreach (var m in matches.Values)
            {
                Debug.Log ($"Checking match {m.matchId} | inMatch {m.inGame} | matchFull {m.IsFull}");
                if (m.inGame || m.IsFull) continue;
                if (JoinMatch(player, m))
                {
                    match = m;
                    return true;
                }
            }

            return CreateNewMatch(player, out match);
        }
        
        private bool JoinMatch(Player player, MatchController match)
        { 
            if (match.inGame || match.IsFull) return false;

            if (!match.SetOpponent(player)) return false;

            Debug.Log($"Joined match {match.matchId}");
            return true;
        }

        private bool CreateNewMatch(Player firstPlayer, out MatchController match)
        {
            Debug.Log("Creating new match");
            
            var matchID = GetRandomMatchID();
            
            match = Instantiate(matchControllerPrefab);
            NetworkServer.Spawn(match.gameObject);
            match.InitMatch(matchID, firstPlayer);
            
            matches[matchID] = match;
            
            Debug.Log($"Match {matchID} generated");
            return true;
        }
        
        private static string GetRandomMatchID()
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

        public void BeginMatch(MatchController match)
        {
            match.inGame = true;
            foreach (var player in match.Players)
            {
             //   player.StartGame ();
            }
        }

        public void DisconnectPlayer(Player player)
        {
            if (player.Match == null) return;
            var match = player.Match;
            
            Debug.Log ($"Player disconnected from match {match.matchId}");
            
            // TODO: remove points from disconnected player
            match.Players.Remove(player);
            match.Resolve();
            
            matches.Remove(match.matchId);
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