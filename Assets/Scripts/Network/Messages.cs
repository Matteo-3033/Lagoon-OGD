using System;
using Mirror;
using UnityEngine.Serialization;

namespace Network.Messages
{
    
    public struct ToServerMatchMessage : NetworkMessage
    {
        public ServerMatchOperation Op;
        public string MatchId;
    }
    
    public struct ToClientMatchMessage : NetworkMessage
    {
        public ClientMatchOperation Op;
        public string MatchId;
        public PlayerInfo[] PlayerInfos;
    }

    [Serializable]
    public struct MatchInfo
    {
        public string matchId;
        public byte players;
    }
   
    [Serializable]
    public struct PlayerInfo
    {
        public string username;
        public bool ready;
        public string matchId;
    }

    public enum ServerMatchOperation : byte
    {
        None,
        Search,
        Ready
    }
    
    public enum ClientMatchOperation : byte
    {
        None,
        MatchFound,
        OpponentJoined,
        Ended,
        Started
    }
}