using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using Network;
using UnityEngine;

namespace Round
{
    [RequireComponent(typeof(NetworkIdentity))]
    public class RoundController: NetworkBehaviour
    {
        private const int UPDATE_TIMER_EVERY_SECONDS = 10;
        private const float LOAD_AFTER_SECONDS = 3F;

        public static RoundController Instance { get; private set; }
        
        public RoundConfiguration Round => MatchController.Instance.CurrentRound;
        public static bool Loaded { get; private set; } 
        
        
        private readonly Dictionary<string, bool> playersReady = new();
        private bool tie;

        public IEnumerable<Player> Players => NetworkServer.connections.Values.Select(conn => conn.Player());
        
        
        // Client side events
        public event Action OnRoundStarted;
        public event Action OnNoWinningCondition;
        public event Action<int> TimerUpdate;
        
        // Server side events
        public event Action OnLoadNextRound;
        
        // Both sides events
        public static event Action OnRoundLoaded;
        public event Action OnCountdownStart;
        public event Action<int> OnCountdown;
        public event Action<Player> OnRoundEnded;
        
        
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Debug.LogWarning("RoundController already exists in the scene. Deleting duplicate.");
                Destroy(gameObject);
                return;
            }

            Instance = this;
            Debug.Log("RoundController initialized");
        }

        private void Start()
        {
            OnRoundLoaded?.Invoke();
            Loaded = true;
        }

        #region SERVER
        
        public override void OnStartServer()
        {
            base.OnStartServer();

            // Add players already connected
            foreach (var conn in NetworkServer.connections.Values)
            {
                var player = conn.Player();
                if (player != null)
                    OnAddPlayer(conn, player.Username);
            }
            
            RiseNetworkManager.OnServerPlayerAdded += OnAddPlayer;
        }
        
        [ServerCallback]
        private void OnAddPlayer(NetworkConnectionToClient conn, string username)
        {
            if (playersReady.ContainsKey(username))
                return;
            
            Debug.Log($"Player {username} loaded round");
            
            playersReady.Add(username, true);

            if (AllPlayersReady())
                LoadRound();
        }
        
        [Server]
        private void LoadRound()
        {
            RpcOnRoundLoaded();
            StartCoroutine(StartRoundCountdown());
        }
        
        [Server]
        private void StartRound()
        {
            RpcStartRound();
            StartCoroutine(Timer());
        }

        private IEnumerator Timer()
        {
            // Wait one frame after round start
            yield return null;
            
            var time = (int)(Round.timeLimitMinutes * 60);
            
            while (time > UPDATE_TIMER_EVERY_SECONDS)
            {
                Debug.Log($"Remaining time: {time}");
                NotifyRemainingTime(time);
                yield return new WaitForSeconds(UPDATE_TIMER_EVERY_SECONDS);
                time -= UPDATE_TIMER_EVERY_SECONDS;
            }
            
            NotifyRemainingTime(time);
            yield return new WaitForSeconds(time);

            NotifyRemainingTime(0);
            
            if (!CheckIfWinner())
            {
                tie = true;

                foreach (var player in Players)
                    player.Inventory.OnKeyFragmentUpdated += CheckPlayerAdvantage;
            }
        }
        
        [Server]
        private void NotifyRemainingTime(int time)
        {
            TimerUpdate?.Invoke(time);
            RpcNotifyRemainingTime(time);
        }

        [Server]
        private bool CheckIfWinner()
        {
            foreach (var player in Players)
            {
                if (player.Inventory.KeyFragments != Round.keyFragments) continue;
                OnRoundEnded?.Invoke(player);
                return true;
            }

            return false;
        }

        [Server]
        private void CheckPlayerAdvantage(object sender, Inventory.OnKeyFragmentUpdatedArgs args)
        {
            if (!tie || args.NewValue <= args.OldValue)
                return;
            
            tie = false;
            foreach (var player in Players)
                player.Inventory.OnKeyFragmentUpdated -= CheckPlayerAdvantage;
            
            EndRound(args.Player);
        }
        
        [Command(requiresAuthority = false)]
        private void CmdCheckWinningCondition(NetworkConnectionToClient sender = null)
        {
            if (sender == null)
                return;

            var player = sender.Player();

            if (player.Inventory.KeyFragments == Round.keyFragments)
                EndRound(player);
            else
                TargetNotifyNoWinningCondition(sender);
        }
        
        private void EndRound(Player winner)
        {
            RpcEndRound(winner);
            OnRoundEnded?.Invoke(winner);
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
            Debug.Log("Round starting in 5 seconds...");
            StartCoroutine(StartRoundCountdown());
        }

        [ClientRpc]
        private void RpcStartRound()
        {
            OnRoundStarted?.Invoke();
            Player.LocalPlayer.EnableMovement(true);
        }
        
        [ClientRpc]
        private void RpcNotifyRemainingTime(int remainingTime)
        {
            Debug.Log($"Remaining time: {remainingTime}");
            TimerUpdate?.Invoke(remainingTime);
        }
        
        [ClientRpc]
        private void RpcEndRound(Player winner)
        {
            Debug.Log($"Round ended. Winner: {winner.Username}");
            OnRoundEnded?.Invoke(winner);
        }
        
        [TargetRpc]
        private void TargetNotifyNoWinningCondition(NetworkConnectionToClient target)
        {
            OnNoWinningCondition?.Invoke();
        }
        
        #endregion

        #region UTILS

        private bool counting;
        private IEnumerator StartRoundCountdown()
        {
            if (counting) yield break;
            counting = true;
            
            yield return null;
            OnCountdownStart?.Invoke();
            
            for (var i = 5; i >= 0; i--)
            {
                Debug.Log($"Round starting in {i}");
                OnCountdown?.Invoke(i);
                yield return new WaitForSeconds(1F);
            }

            if (isServer)
                StartRound();
            else OnCountdown?.Invoke(-1);
            counting = false;
        }
        
        private bool AllPlayersReady()
        {
            return playersReady.Count == MatchController.MAX_PLAYERS && playersReady.Values.All(ready => ready);
        }

        #endregion

        private void OnDestroy()
        {
            Debug.Log("Destroying RoundController");
            Instance = null;
            Loaded = false;
        }
    }
}