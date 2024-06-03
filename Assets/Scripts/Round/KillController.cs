﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using UnityEngine;

namespace Round
{
    public class KillController : NetworkBehaviour
    {
        public const int RESPAWN_TIME = 10;
        private const int WAIT_BEFORE_MINIGAME = 1;
        private const int KEYS_PER_MINIGAME = 3;
        private const float MINIGAME_TIME = 15;

        public enum MiniGameKeys
        {
            Up = 'w',
            Down = 's',
            Left = 'a',
            Right = 'd'
        }

        public static KillController Instance { get; private set; }

        public static event Action<Player> OnPlayerKilled;
        public static event Action<Player> OnPlayerRespawned;
        public static event Action OnMiniGameStarting;
        public static event Action OnMiniGameEnded;
        public static event Action<MiniGameKeys?> OnMiniGameNextKey;
        

        private bool roundInProgress;
        private bool miniGameRunning;
        private readonly Dictionary<string, float> playerTimes = new();

        private List<Player> _players;
        private List<Player> Players => _players ??= RoundController.Instance.Players.ToList();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Debug.LogWarning("KillController already exists in the scene. Deleting duplicate.");
                Destroy(gameObject);
                return;
            }

            Instance = this;
            Debug.Log("KillController initialized");
        }

        public override void OnStartServer()
        {
            base.OnStartServer();

            RoundController.OnRoundLoaded += OnRoundLoaded;
        }

        private void OnRoundLoaded()
        {
            RoundController.Instance.OnRoundStarted += () => roundInProgress = true;
            RoundController.Instance.OnRoundEnded += _ => roundInProgress = false;
        }


        [Server]
        public void TryKillPlayer(Player killed, object by, bool stealTrap = false)
        {
            if (miniGameRunning || killed.IsDead || (by is Player p && (killed.FieldOfView.CanSeePlayer || !p.FieldOfView.CanSeePlayer)))
                return;

            KillPlayer(killed, by, stealTrap);
        }

        [Server]
        private IEnumerator RespawnPlayer(Player player)
        {
            yield return new WaitForSeconds(RESPAWN_TIME);

            player.Respawn();
            OnPlayerRespawned?.Invoke(player);
            RpcPlayerRespawned(player);
        }
                
        [ClientRpc]
        private void RpcPlayerKilled(Player player)
        {
            OnPlayerKilled?.Invoke(player);
        }
        
        [ClientRpc]
        private void RpcPlayerRespawned(Player player)
        {
            OnPlayerRespawned?.Invoke(player);
        }

        private void Update()
        {
            if (!isServer || !roundInProgress || miniGameRunning)
                return;

            if (Players.TrueForAll(p => p.FieldOfView.CanSeePlayer && !p.IsDead)) // TODO: controllare solo il triangolo interno
                StartCoroutine(StartKillMiniGame());
        }

        [Server]
        private IEnumerator StartKillMiniGame()
        {
            Debug.Log("Starting kill minigame");
            miniGameRunning = true;
            playerTimes.Clear();
            
            var enemies = FindObjectsByType<EnemyFSM>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            foreach (var enemy in enemies)
                enemy.StopFSM();
            RpcBeforeMiniGameStart();

            yield return new WaitForSeconds(WAIT_BEFORE_MINIGAME);

            var sequence = new List<MiniGameKeys>
            {
                MiniGameKeys.Up, MiniGameKeys.Down, MiniGameKeys.Left, MiniGameKeys.Right
            }.OrderBy(_ => Guid.NewGuid()).ToList().GetRange(0, KEYS_PER_MINIGAME);

            RpcStartMiniGame(sequence);
        }

        [Server]
        private IEnumerator EndMiniGame()
        {
            Debug.Log("Ending kill minigame");
                        
            RpcEndMinigame();
            var enemies = FindObjectsByType<EnemyFSM>(FindObjectsSortMode.None);
            foreach (var enemy in enemies)
                enemy.PlayFSM();
            
            yield return new WaitForSeconds(1);

            var players = RoundController.Instance.Players.ToList();

            var winner = players[0];
            var loser = players[1];
            if (playerTimes[winner.Username] > playerTimes[loser.Username])
            {
                winner = players[1];
                loser = players[0];
            }

            KillPlayer(loser, winner, winner.StabManager.CanStealTraps);
            miniGameRunning = false;
        }

        [Server]
        private void KillPlayer(Player killed, object by, bool stealTrap)
        {
            Debug.Log($"Player {killed.Username} killed by {by}");
            if (by is Player p)
            {
                if (killed.Inventory.StealKeyFragment())
                    p.Inventory.AddKeyFragment();

                if (stealTrap && !p.Inventory.IsTrapBagFull() && killed.Inventory.StealTrap(out var trap))
                    p.Inventory.AddTrap(trap);
            }

            killed.Kill();
            OnPlayerKilled?.Invoke(killed);
            RpcPlayerKilled(killed);

            StartCoroutine(RespawnPlayer(killed));
        }

        [ClientRpc]
        private void RpcBeforeMiniGameStart()
        {
            Debug.Log("Starting kill minigame");
            
            miniGameRunning = true;
            Player.LocalPlayer.EnableMovement(false);
            OnMiniGameStarting?.Invoke();
        }

        [ClientRpc]
        private void RpcStartMiniGame(List<MiniGameKeys> sequence)
        {
            Debug.Log("Kill minigame sequence: " + string.Join(", ", sequence));
            StartCoroutine(GetPlayerInput(sequence));
        }
        
        [ClientRpc]
        private void RpcEndMinigame()
        {
            Debug.Log("Ending kill minigame");

            miniGameRunning = false;
            Player.LocalPlayer.EnableMovement(true);
            OnMiniGameEnded?.Invoke();
        }

        [Client]
        private IEnumerator GetPlayerInput(List<MiniGameKeys> sequence)
        {
            // TODO: aggiungere timeout
            yield return null;
            var startTime = Time.time * 1000;

            Debug.Log("Starting minigame sequence");
            var i = 0;
            while (miniGameRunning && i < sequence.Count)
            {
                Debug.Log($"Waiting for key {sequence[i]}");
                OnMiniGameNextKey?.Invoke(sequence[i]);
                if (Input.anyKeyDown)
                {
                    foreach (var c in Input.inputString.ToLower())
                    {
                        if(c == (char) sequence[i])
                        {
                            Debug.Log($"The next key '{c}' was entered correctly");
                            i++;
                        }
                        else
                        {
                            Debug.Log($"The next key '{c}' was entered correctly");
                            i++;
                        }
                    }
                }
                
                yield return null;
            }
            
            Debug.Log("The minigame sequence ended");
            var time = Time.time * 1000 - startTime;
            
            OnMiniGameNextKey?.Invoke(null);
            
            if (miniGameRunning)
                CmdMiniGameSequenceEnded(Player.LocalPlayer.Username, time);
        }


        [Command(requiresAuthority = false)]
        private void CmdMiniGameSequenceEnded(string playerUsername, float time)
        {
            if (!miniGameRunning || time < 0)
                return;

            Debug.Log($"Player {playerUsername} completed the minigame in {time} milliseconds");
            
            var player = RoundController.Instance.Players.First(p => p.Username == playerUsername);
            playerTimes[player.Username] = time;

            if (playerTimes.Count == 2)
                StartCoroutine(EndMiniGame());
        }

        private void OnDestroy()
        {
            Debug.Log("Destroying KillController");
            Instance = null;
        }
    }
}