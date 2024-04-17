using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using Network.Master;
using Round;
using UnityEngine;

namespace Network
{
    public struct MatchPlayerData
    {
        public string Username;
        public int RoundsWon;
        public int Kills;
        public int Deaths;
        public bool IsWinner;
    }
    
    public class MatchController : NetworkBehaviour
    {
        private const int MaxPlayers = 2;
        
        public static MatchController Instance { get; private set; }
        
        public bool Started { get; private set; }
        
        private int currentRoundIndex;
        private readonly List<RoundConfiguration> rounds = new();
        internal readonly Dictionary<NetworkConnectionToClient, string> Usernames = new();

        [field: SyncVar]
        public RoundConfiguration CurrentRound { get; private set; }

        private readonly SyncDictionary<string, MatchPlayerData> players = new();
        
        // Server side events
        public event Action OnMatchStarted;
        public event Action<MatchPlayerData, MatchPlayerData> OnMatchEnded;
        
        public int RoundCnt => rounds.Count;
        
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

        #region SERVER
        
        public override void OnStartServer()
        {
            base.OnStartServer();
            Debug.Log("MatchController started on server");
            
            rounds.AddRange(RiseNetworkManager.RoomOptions.CustomOptions.AsString(RoomsModule.RoundsKey).GetRounds());
            
            Debug.Log($"Round count: {RoundCnt}");
            
            RiseNetworkManager.OnServerPlayerAdded += OnAddPlayer;
        }
        
        [ServerCallback]
        private void OnAddPlayer(NetworkConnectionToClient conn, string username)
        {
            if (Usernames.ContainsKey(conn))
                return;
            
            if (Started)
                conn.Disconnect();
            
            if (!Usernames.ContainsKey(conn))
                InitPlayer(conn, username);
            
            if (players.Count == MaxPlayers)
                StartCoroutine(StartMatch());
        }

        [Server]
        private void InitPlayer(NetworkConnectionToClient conn, string username)
        {
            Usernames[conn] = username;
            
            players[username] = new MatchPlayerData { Username = username};
            
            Debug.LogError($"Adding player {username} to match. Current players: {Usernames.Count}/{MaxPlayers}");
        }

        [Server]
        private IEnumerator StartMatch()
        {
            if (Started)
                yield break;
            
            yield return new WaitForSeconds(2.5F);
        
            LoadRound(0);
            Started = true;
            OnMatchStarted?.Invoke();
            
            RoundController.OnRoundLoaded += () => RoundController.Instance.OnRoundWon += OnRoundWon;
        }
        
        [Server]
        private void LoadRound(int roundIndex)
        {
            currentRoundIndex = roundIndex;
            if (currentRoundIndex >= RoundCnt)
            {
                EndMatch();
                return;
            }
            
            Debug.Log($"Loading round {currentRoundIndex}/{RoundCnt}");
            CurrentRound = rounds[currentRoundIndex];

            RiseNetworkManager.singleton.ServerChangeScene(CurrentRound.scene);
        }
        
        [Server]
        private void EndMatch(string disconnectedPlayer = null)
        {
            Debug.Log("Match ended");
            Started = false;

            MatchPlayerData winner;
            MatchPlayerData loser;
            if (disconnectedPlayer == null)
            {
                var p = players.Values.ToList();
                winner = p[0];
                loser = p[1];

                if (p[1].RoundsWon > p[0].RoundsWon)
                {
                    winner = p[1];
                    loser = p[0];
                }

                winner.IsWinner = true;
                loser.IsWinner = false;
            }
            else
            {
                loser = players[disconnectedPlayer];
                winner = players.Values.First(player => player.Username != disconnectedPlayer);
            }
            
            Debug.Log($"Winner: {winner.Username}");
            
            OnMatchEnded?.Invoke(winner, loser);
        }

        [ServerCallback]
        public void OnPlayerDisconnected(string username)
        {
            if (!players.ContainsKey(username))
                return;
            
            if (Started)
                EndMatch(username);
            
            var conn = Usernames.First(pair => pair.Value == username).Key;
            Usernames.Remove(conn);
            players.Remove(username);
        }

        private void OnRoundWon(Player player)
        {
            var data = players[player.Username];
            data.RoundsWon++;
            players[player.Username] = data;
            LoadRound(currentRoundIndex + 1);
        }

        #endregion

        private void OnDestroy()
        {
            Instance = null;
        }
    }

    internal static class ConnectionExtensions
    {
        public static Player Player(this NetworkConnectionToClient conn)
        {
            if (conn.identity == null)
                return null;
            return !conn.identity.TryGetComponent(out Player player) ? null : player;
        }
        
        public static Player Opponent(this NetworkConnectionToClient conn)
        {
            return MatchController.Instance.Usernames.First(kp => kp.Value != conn.Player().Username).Key.Player();
        }
    }
}