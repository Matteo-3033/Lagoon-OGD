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
        private const int RoundPlayers = 2;

        public static RoundController Instance { get; private set; }
        
        public RoundConfiguration Round => MatchController.Instance.CurrentRound;
        
        private readonly Dictionary<string, bool> playersReady = new();
        public bool Loaded { get; private set; } 
        
        // Client side events
        public event Action OnRoundStarted;
        public event Action OnNoWinningCondition;
        
        // Server side events
        public event Action<Player> OnRoundWon;
        
        // Both sides events
        public static event Action OnRoundLoaded;
        public event Action OnCountdownStart;
        public event Action<int> OnCountdown;
        
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
            {
                RpcOnRoundLoaded();
                StartCoroutine(StartRoundCountdown());
            }
        }
        
        [Server]
        private void StartRound()
        {
            RpcStartRound();
            //Invoke(nameof(), round.timeLimit/2);
            //Invoke(nameof(), round.timeLimit);
        }
        
        [Command(requiresAuthority = false)]
        private void CmdCheckWinningCondition(NetworkConnectionToClient sender = null)
        {
            if (sender == null)
                return;

            var player = sender.Player();
            
            if (player.Inventory.KeyFragments == Round.keyFragments)
                OnRoundWon?.Invoke(player);
            else 
                TargetNotifyNoWinningCondition(sender);
        }

        [TargetRpc]
        private void TargetNotifyNoWinningCondition(NetworkConnectionToClient target)
        {
            OnNoWinningCondition?.Invoke();
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
            Player.LocalPlayer.EnableMovement();
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
            counting = false;
        }
        
        private bool AllPlayersReady()
        {
            return playersReady.Count == RoundPlayers && playersReady.Values.All(ready => ready);
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