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
        
        // Client side events
        public event Action OnRoundLoaded;
        public event Action OnRoundStarted;
        public event Action OnNoWinningCondition;
        
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
            if (!Usernames.ContainsKey(conn))
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
            Debug.Log("PLAYERS COUNT: " + Usernames.Count);
        }

        [Server]
        private void InitPlayer(NetworkConnectionToClient conn, string username)
        {
            Usernames[conn] = username;
            
            players[username] = new MatchPlayerData
            {
                Username = username,
                Ready = true
            };
            
            Debug.LogError($"Adding player {username} to match. Current players: {Usernames.Count}/{MaxPlayers}");

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
            CurrentRound = rounds[currentRoundIndex];

            UnreadyAllPlayers();
            RiseNetworkManager.singleton.ServerChangeScene(CurrentRound.scene);
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
        
        [Command(requiresAuthority = false)]
        private void CmdCheckWinningCondition(NetworkConnectionToClient sender = null)
        {
            if (sender == null)
                return;
            
            var player = Usernames[sender];
            
            if (sender.Player().Inventory.KeyFragments == CurrentRound.keyFragments)
            {
                var data = players[player];
                data.RoundsWon++;
                players[player] = data;
                OnRoundWon();
            } else 
                TargetNotifyNoWinningCondition(sender);
        }

        [TargetRpc]
        private void TargetNotifyNoWinningCondition(NetworkConnectionToClient sender)
        {
            OnNoWinningCondition?.Invoke();
        }

        private void OnRoundWon()
        {
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
            return Usernames.Count == MaxPlayers && players.All(player => player.Value.Ready);
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
            foreach (var username in Usernames.Values)
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
        
        public static Player Opponent(this NetworkConnectionToClient conn)
        {
            return MatchController.Instance.Usernames.First(kp => kp.Value != conn.Player().Username).Key.Player();
        }
    }
}