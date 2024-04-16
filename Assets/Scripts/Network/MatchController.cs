using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using Network.Master;
using UnityEngine;
using Utils;

namespace Network
{
    public struct MatchPlayerData
    {
        public string Username;
        public int KeyFragments;
        public bool Ready;
        public int Score;
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
        private readonly Dictionary<NetworkConnectionToClient, string> usernames = new();
        
        [SyncVar] private RoundConfiguration currentRound;
        private readonly SyncDictionary<string, MatchPlayerData> players = new();
        
        // Client side events
        public event Action OnRoundLoaded;
        public event Action OnRoundStarted;
        
        // Server side events
        public event Action OnMatchStarted;
        public event Action<MatchPlayerData, MatchPlayerData> OnMatchEnded;
        
        // Both sides events
        public event Action<int> OnCountdown;
        
        
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
            if (!usernames.ContainsKey(conn))
                InitPlayer(conn, username);
            else
            {
                Debug.Log($"Player {username} loaded round");
                
                SetPlayerReady(username, true);
                
                if (AllPlayersReady())
                {
                    RpcOnRoundLoaded();
                    StartCoroutine(StartRoundCountdown());
                }
            }
            Debug.Log("PLAYERS COUNT: " + usernames.Count);
        }

        [Server]
        private void InitPlayer(NetworkConnectionToClient conn, string username)
        {
            usernames[conn] = username;
            
            players[username] = new MatchPlayerData
            {
                Username = username,
                Ready = true
            };
            
            Debug.LogError($"Adding player {username} to match. Current players: {usernames.Count}/{MaxPlayers}");

            if (AllPlayersReady())
                StartCoroutine(StartMatch());
        }

        [Server]
        private IEnumerator StartMatch()
        {
            if (Started)
                yield break;
            
            yield return new WaitForSeconds(2.5F);

            if (AllPlayersReady())
            {
                LoadRound(0);
                Started = true;
                OnMatchStarted?.Invoke();
            }
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
            currentRound = rounds[currentRoundIndex];

            UnreadyAllPlayers();
            RiseNetworkManager.singleton.ServerChangeScene(currentRound.scene);
        }

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

                if (p[1].Score > p[0].Score)
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
            
            var conn = usernames.First(pair => pair.Value == username).Key;
            usernames.Remove(conn);
            players.Remove(username);
        }
        
        [Command(requiresAuthority = false)]
        private void CmdCheckWinningCondition(NetworkConnectionToClient sender = null)
        {
            if (sender == null)
                return;
            
            var player = usernames[sender];
            
            // TODO: check if player has all key fragments
            
            var data = players[player];
            data.Score++;
            players[player] = data;
            
            LoadRound(currentRoundIndex + 1);
        }
        
        #endregion
        
        #region CLIENT
        
        public void CheckWinningCondition()
        {
            CmdCheckWinningCondition();
        }

        [ClientRpc]
        private void RpcOnRoundLoaded()
        {
            OnRoundLoaded?.Invoke();
            Debug.Log("Round loaded");
            StartCoroutine(StartRoundCountdown());
        }

        [ClientRpc]
        private void RpcStartRound()
        {
            OnRoundStarted?.Invoke();
            Player.LocalPlayer.EnableMovement();
        }

        #endregion

        #region UTILS

        private bool counting = false;
        private IEnumerator StartRoundCountdown()
        {
            if (counting) yield break;
            counting = true;
            yield return null;
            
            for (int i = 5; i >= 0; i--)
            {
                Debug.Log($"Round starting in {i}");
                OnCountdown?.Invoke(i);
                yield return new WaitForSeconds(1F);
            }
            
            if (isServer)
                RpcStartRound();
            counting = false;
        }
        
        private bool AllPlayersReady()
        {
            return usernames.Count == MaxPlayers && players.All(player => player.Value.Ready);
        }
        
        [Server]
        private void SetPlayerReady(string username, bool ready)
        {
            if (!players.ContainsKey(username))
                return;

            var data = players[username];
            data.Ready = ready;
            players[username] = data;
        }
        
        [Server]
        private void UnreadyAllPlayers()
        {
            foreach (var username in usernames.Values)
                SetPlayerReady(username, false);
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
    }
}