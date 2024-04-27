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
        public const int MAX_PLAYERS = 2;
        public static MatchController Instance { get; private set; }
        
        
        private readonly List<RoundConfiguration> rounds = new();
        internal readonly Dictionary<NetworkConnectionToClient, string> ConnectionsToUsernames = new();
        
        [field: SyncVar] public bool Started { get; private set; }
        [field: SyncVar] public RoundConfiguration CurrentRound { get; private set; }
        [field: SyncVar] public int RoundCnt { get; private set; }
        public IReadOnlyDictionary<string, MatchPlayerData> Players => players;
        private readonly SyncDictionary<string, MatchPlayerData> players = new();
        [SyncVar] private int currentRoundIndex;
        
        // Server side events
        public event Action OnMatchStarted;
        public event Action<MatchPlayerData, MatchPlayerData> OnMatchEnded;
        
        // Client side events
        public event Action<string> OnPlayersDataChanged;
        
        
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
            RoundCnt = rounds.Count;
            
            Debug.Log($"Round count: {RoundCnt}");
            
            RiseNetworkManager.OnServerPlayerAdded += OnAddPlayer;
        }
        
        [ServerCallback]
        private void OnAddPlayer(NetworkConnectionToClient conn, string username)
        {
            if (ConnectionsToUsernames.ContainsKey(conn))
                return;
            
            if (Started)
                conn.Disconnect();
            
            if (!ConnectionsToUsernames.ContainsKey(conn))
                InitPlayer(conn, username);
            
            if (players.Count == MAX_PLAYERS)
                StartCoroutine(StartMatch());
        }

        [Server]
        private void InitPlayer(NetworkConnectionToClient conn, string username)
        {
            ConnectionsToUsernames[conn] = username;
            
            players[username] = new MatchPlayerData { Username = username};
            
            Debug.LogError($"Adding player {username} to match. Current players: {ConnectionsToUsernames.Count}/{MAX_PLAYERS}");
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

            RoundController.OnRoundLoaded += OnRoundLoaded;
        }

        private void OnRoundLoaded()
        {
            RoundController.Instance.OnRoundEnded += OnRoundWinner;
            RoundController.Instance.OnLoadNextRound += () => LoadRound(currentRoundIndex + 1);
        }

        [Server]
        private void LoadRound(int roundIndex)
        {
            currentRoundIndex = roundIndex;
            if (HasWinner())
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
            
            var conn = ConnectionsToUsernames.First(pair => pair.Value == username).Key;
            ConnectionsToUsernames.Remove(conn);
            players.Remove(username);
        }

        private void OnRoundWinner(Player winner)
        {
            var data = players[winner.Username];
            data.RoundsWon++;
            players[winner.Username] = data;
        }

        #endregion

        #region CLIENT

        public override void OnStartClient()
        {
            base.OnStartClient();
            
            players.Callback += (_, username, _) => OnPlayersDataChanged?.Invoke(username);
        }

        #endregion
        
        public bool HasWinner()
        {
            return currentRoundIndex >= RoundCnt || players.Values.Any(player => player.RoundsWon >= RoundCnt / 2 + 1);
        }

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
            return MatchController.Instance.ConnectionsToUsernames.First(kp => kp.Value != conn.Player().Username).Key.Player();
        }
    }
}