using System;
using System.Collections;
using Mirror;
using UnityEngine;

namespace Network
{
    [Serializable]
    public struct MatchPlayerData
    {
        public string username;
        // TODO: add other match data
    }
    
    [RequireComponent(typeof(NetworkMatch))]
    public class MatchController : NetworkBehaviour
    {
        public readonly SyncDictionary<string, MatchPlayerData> Players = new();
        [SyncVar] public string matchId;
        
        public bool IsFull => Players.Count == 2;
        
        private NetworkMatch networkMatch;

        private void Awake()
        {
            networkMatch = gameObject.GetComponent<NetworkMatch>();
        }

        public void InitMatch(string id)
        {
            if (!string.IsNullOrEmpty(matchId)) return;
            
            matchId = id;
            networkMatch.matchId = matchId.ToGuid();
        }


        public void AddPlayer(Player player)
        {
            if (IsFull) return;
            StartCoroutine(AddPlayersToMatchController(player));
        }
        
        // For the SyncDictionary to properly fire the update callback, we must
        // wait a frame before adding the players to the already spawned MatchController
        private IEnumerator AddPlayersToMatchController(Player player)
        {
            yield return null;
            
            Players.Add(player.username, new MatchPlayerData { username = player.username });
        }
    }
}