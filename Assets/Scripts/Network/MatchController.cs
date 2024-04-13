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
        
        public event Action OnRoundLoaded;
        public event Action<int> OnCountdown;
        public event Action OnRoundStart;
        
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
            RiseNetworkManager.OnServerDisconnected += OnPlayerDisconnected;
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
                    StartCoroutine(Countdown());
                }
            }
        }

        [Server]
        private void InitPlayer(NetworkConnectionToClient conn, string username)
        {
            Debug.LogError($"Adding player {username} to match. Current players: {usernames.Count}/{MaxPlayers}");
            
            usernames[conn] = username;
            
            players[username] = new MatchPlayerData
            {
                Username = username,
                KeyFragments = 0,
                Ready = true
            };
                
            if (usernames.Count == MaxPlayers && AllPlayersReady())
                StartMatch();
        }

        [Server]
        private void StartMatch()
        {
            if (Started)
                return;

            LoadRound(0);
            Started = true;
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
            
            Debug.Log($"Loading round {currentRoundIndex}");
            currentRound = rounds[currentRoundIndex];

            UnreadyAllPlayers();
            RiseNetworkManager.singleton.ServerChangeScene(currentRound.scene);
        }

        private void EndMatch()
        {
            Debug.Log("Match ended");
            var data = players.Values.ToList();
        }

        [Command(requiresAuthority = false)]
        private void CmdCheckWinningCondition(NetworkConnectionToClient sender = null)
        {
            if (sender == null)
                return;
            
            var player = usernames[sender];
            // TODO: check if player has all key fragments
            
            LoadRound(currentRoundIndex + 1);
        }
        
        [ServerCallback]
        private void OnPlayerDisconnected(NetworkConnectionToClient obj)
        {
            if (!usernames.ContainsKey(obj))
                return;
            
            EndMatch();
            
            var username = usernames[obj];
            usernames.Remove(obj);
            players.Remove(username);
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
            StartCoroutine(Countdown());
        }

        [ClientRpc]
        private void RpcStartRound()
        {
            OnRoundStart?.Invoke();
            Player.LocalPlayer.EnableMovement();
        }

        #endregion

        #region UTILS
        
        private IEnumerator Countdown()
        {
            yield return null;
            
            for (int i = 5; i >= 0; i--)
            {
                Debug.Log($"Round starting in {i}");
                OnCountdown?.Invoke(i);
                yield return new WaitForSeconds(1F);
            }
            
            if (isServer)
                RpcStartRound();
        }
        
        private bool AllPlayersReady()
        {
            return players.All(player => player.Value.Ready);
        }
        
        [Server]
        private void SetPlayerReady(string username, bool ready)
        {
            if (!players.ContainsKey(username))
                return;
            
            players[username] = new MatchPlayerData
            {
                Username = username,
                KeyFragments = players[username].KeyFragments,
                Ready = ready
            };
        }
        
        [Server]
        private void UnreadyAllPlayers()
        {
            foreach (var username in usernames.Values)
            {
                players[username] = new MatchPlayerData
                {
                    Username = players[username].Username,
                    KeyFragments = players[username].KeyFragments,
                    Ready = false
                };
            }
        }

        #endregion
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