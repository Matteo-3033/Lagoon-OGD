using Mirror;
using UnityEngine;

namespace Network
{
    public class MatchController : NetworkBehaviour
    {
        [SyncVar] public  string matchId;
        [SyncVar] public bool inGame;
        public readonly SyncList<Player> Players = new();
        
        public bool IsFull => Players.Count == 2;

        public void Resolve()
        {
            // TODO: Implement match resolution
        }

        public void InitMatch(string id, Player firstPlayer)
        {
            if (!string.IsNullOrEmpty(matchId)) return;
            Debug.Log($"Initializing match {matchId}");
            
            inGame = false;
            matchId = id;
            Players.Add(firstPlayer);
        }

        public bool SetOpponent(Player player)
        {
            if (IsFull) return false;
            Players.Add(player);
            return true;
        }
    }
}