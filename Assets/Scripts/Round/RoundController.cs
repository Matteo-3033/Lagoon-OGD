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
        private const int UPDATE_TIMER_EVERY_SECONDS = 5;
        private const float LOAD_NEXT_ROUND_AFTER_SECONDS = 10F;

        public static RoundController Instance { get; private set; }
        public static RoundConfiguration Round => MatchController.Instance.CurrentRound;
        
        public enum RoundState
        {
            None,
            Loaded,
            Starting,
            Started,
            Ended,
            LoadingNext
        }
        
        public static RoundState State { get; private set; } = RoundState.None;
        public static bool HasLoaded() => Instance != null && State >= RoundState.Loaded;


        private readonly HashSet<string> playersReady = new();
        private bool tie;

        public IEnumerable<Player> Players => NetworkServer.connections.Values.Select(conn => conn.Player());
        [field: SyncVar] public Player Winner { get; private set; }
        
        
        // Client side events
        public event Action OnNoWinningCondition;
        public event Action<int> TimerUpdate;
        
        // Server side events
        public event Action OnLoadNextRound;
        
        // Both sides events
        public static event Action OnRoundLoaded;
        public event Action OnCountdownStart;
        public event Action<int> OnCountdown;
        public event Action OnRoundStarted;
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
            State = RoundState.Loaded;
            OnRoundLoaded?.Invoke();
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
            if (playersReady.Contains(username))
                return;
            
            Debug.Log($"Player {username} loaded round");
            
            playersReady.Add(username);

            if (AllPlayersReady())
                StartCountdown();
        }
        
        [Server]
        private void StartCountdown()
        {
            if (State != RoundState.Loaded)
            {
                Debug.LogWarning("Round not loaded yet. Cannot start countdown.");
                return;
            }
            
            playersReady.Clear();
            State = RoundState.Starting;
            RpcStartCountdown();
            StartCoroutine(Countdown());
        }
        
        [Server]
        private void StartRound()
        {
            if (State != RoundState.Starting)
            {
                Debug.LogWarning("Round not starting yet. Cannot start round.");
                return;
            }
            
            State = RoundState.Started;
            RpcStartRound();
            OnRoundStarted?.Invoke();
            StartCoroutine(Timer());
        }

        private int timer;

        private IEnumerator Timer()
        {
            // Wait one frame after round start
            yield return null;
            
            timer = (int)(Round.timeLimitMinutes * 60);
            
            while (timer > UPDATE_TIMER_EVERY_SECONDS)
            {
                Debug.Log($"Remaining time: {timer}");
                NotifyRemainingTime(timer);
                yield return new WaitForSeconds(UPDATE_TIMER_EVERY_SECONDS);
                timer -= UPDATE_TIMER_EVERY_SECONDS;
            }
            
            NotifyRemainingTime(timer);
            yield return new WaitForSeconds(timer);

            NotifyRemainingTime(0);

            OnTimerEnd();
        }

        [Server]
        private void NotifyRemainingTime(int time)
        {
            TimerUpdate?.Invoke(time);
            RpcNotifyRemainingTime(time);
        }
        
        [Server]
        private void OnTimerEnd()
        {
            Debug.Log("Round time ended");
            if (CheckIfAdvantage(out var winner))
                EndRound(winner);
            else
            {
                tie = true;
                foreach (var player in Players)
                    player.Inventory.OnKeyFragmentUpdated += CheckPlayerAdvantage;
            }
        }
        
        [Server]
        public void AddTime(int bonusTime)
        {
            timer += bonusTime;
        }

        [Server]
        private bool CheckIfAdvantage(out Player winner)
        {
            winner = null;

            var players = Players.ToList();
            Debug.Log(players.Count);
            if (players.Count == 0)
                return false;
            
            if (players.Count == 1)
            {
                winner = players[0];
                return true;
            }
            
            var player1 = players[0];
            var player2 = players[1];
            
            if (player1.Inventory.KeyFragments > player2.Inventory.KeyFragments)
            {
                winner = player1;
                return true;
            }
            
            if (player1.Inventory.KeyFragments < player2.Inventory.KeyFragments)
            {
                winner = player2;
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
        
        [Server]
        private void EndRound(Player winner)
        {
            if (State != RoundState.Started)
            {
                Debug.LogWarning("Round not started yet. Cannot end round.");
                return;
            }
            
            Debug.Log($"Round ended. Winner: {winner.Username}");

            Winner = winner;
            State = RoundState.Ended;
            OnRoundEnded?.Invoke(winner);
            RpcEndRound(winner);
        }
        
        [Command(requiresAuthority = false)]
        private void CmdRegisterNextRoundRequest(NetworkConnectionToClient sender = null)
        {
            if (sender == null) return;
            
            var player = sender.Player();
            playersReady.Add(player.Username);
            
            if (AllPlayersReady())
                LoadNextRound();
            else Invoke(nameof(LoadNextRound), LOAD_NEXT_ROUND_AFTER_SECONDS);
        }

        [Server]
        private void LoadNextRound()
        {
            if (State < RoundState.Ended)
            {
                Debug.LogWarning("Round not ended yet. Cannot load next round.");
                return;
            }
            
            if (State == RoundState.LoadingNext)
                return;
            
            State = RoundState.LoadingNext;
            OnLoadNextRound?.Invoke();
        }

        #endregion
        
        #region CLIENT
        
        public void CheckWinningCondition()
        {
            CmdCheckWinningCondition();
        }

        [ClientRpc]
        private void RpcStartCountdown()
        {
            Debug.Log("Round starting in 5 seconds...");
            State = RoundState.Starting;
            StartCoroutine(Countdown());
        }

        [ClientRpc]
        private void RpcStartRound()
        {
            State = RoundState.Started;
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
            State = RoundState.Ended;
            OnRoundEnded?.Invoke(winner);
        }
        
        public void AskForNextRound()
        {
            CmdRegisterNextRoundRequest();
        }
        
        [TargetRpc]
        private void TargetNotifyNoWinningCondition(NetworkConnectionToClient target)
        {
            OnNoWinningCondition?.Invoke();
        }
        
        #endregion

        #region UTILS

        private bool counting;
        private IEnumerator Countdown()
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
            return playersReady.Count == MatchController.MAX_PLAYERS;
        }

        #endregion

        private void OnDestroy()
        {
            Debug.Log("Destroying RoundController");
            Instance = null;
            State = RoundState.None;
        }
    }
}