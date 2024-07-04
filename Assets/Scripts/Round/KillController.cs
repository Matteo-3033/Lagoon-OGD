using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using UnityEngine;
using Utils;

namespace Round
{
    public class KillController : NetworkBehaviour
    {
        public const int RESPAWN_TIME = 10;
        private const int WAIT_BEFORE_MINIGAME = 2;
        private const int KEYS_PER_MINIGAME = 3;

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
        

        private bool canKill;
        
        public bool MiniGameRunning { get; private set; }
        private readonly object miniGameLock = new();

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
            RoundController.Instance.OnRoundStarted += () => canKill = true;
            RoundController.Instance.OnRoundEnded += _ => canKill = false;
            ChancellorEffectsController.OnEffectEnabled += OnEffectEnabled;
        }

        private void OnEffectEnabled(object sender, ChancellorEffectsController.OnEffectEnabledArgs e)
        {
            canKill = false;
            FunctionTimer.Create(() => canKill = true, e.Effect.duration > 0 ? e.Effect.duration : 0.1F);
        }

        [Server]
        public void TryKillPlayer(Player killed, object by, bool stealTrap = false)
        {
            if (MiniGameRunning || killed.IsDead || (by is Player p && (killed.FieldOfView.CanSeePlayer || !p.FieldOfView.CanSeePlayer)))
                return;

            KillPlayer(killed, by, stealTrap);
        }

        [Server]
        public void TryKillSentinel(FSMSentinel sentinel, Player player)
        {
            float halfFieldOfViewAngle = sentinel.GetComponentInChildren<FieldOfView>().GetAngle() / 2;
            float dotProduct = Vector3.Dot(player.transform.forward, sentinel.transform.forward);

            float stabAngle = Mathf.Acos(dotProduct) * Mathf.Rad2Deg;
            if ((dotProduct > 0 && stabAngle < halfFieldOfViewAngle) ||
                (dotProduct < 0 && stabAngle < 180 - halfFieldOfViewAngle))
            {
                Debug.Log("Sentinel Killed");
                NetworkServer.Destroy(sentinel.gameObject);
            }
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
            if (!isServer || !canKill || MiniGameRunning)
                return;

            if (Players.TrueForAll(p => p.FieldOfView.IsPlayerIn && !p.IsDead))
                StartCoroutine(StartKillMiniGame());
        }

        [Server]
        private IEnumerator StartKillMiniGame()
        {
            var player1 = Players[0];
            var player2 = Players[1];
            
            if (Vector3.Distance(player1.transform.position, player2.transform.position) > 8)
                yield break;
            
            Debug.Log("Starting kill minigame");
            lock (miniGameLock)
            {
                if (MiniGameRunning)
                    yield break;
                
                MiniGameRunning = true;
            }
            
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
        private IEnumerator EndMiniGame(Player winner)
        {
            lock (miniGameLock)
            {
                if (!MiniGameRunning)
                    yield break;
            
                Debug.Log("Ending kill minigame");

                yield return null;

                var loser = Players.First(p => p.Username != winner.Username);

                KillPlayer(loser, winner, winner.StabManager.CanStealTraps);
                
                RpcEndMinigame();
                var enemies = FindObjectsByType<EnemyFSM>(FindObjectsSortMode.None);
                foreach (var enemy in enemies)
                    enemy.PlayFSM();
                
                MiniGameRunning = false;
            }
        }

        [Server]
        private void KillPlayer(Player killed, object by, bool stealTrap)
        {
            Debug.Log($"Player {killed.Username} killed by {by}");
            
            killed.Kill();
            OnPlayerKilled?.Invoke(killed);
            RpcPlayerKilled(killed);
            
            if (by is Player p)
            {
                if (killed.Inventory.StealKeyFragment())
                    p.Inventory.AddKeyFragment();

                if (stealTrap && !p.Inventory.IsTrapBagFull() && killed.Inventory.StealTrap(out var trap))
                    p.Inventory.AddTrap(trap);
            }

            StartCoroutine(RespawnPlayer(killed));
        }

        [ClientRpc]
        private void RpcBeforeMiniGameStart()
        {
            Debug.Log("Starting kill minigame");
            
            MiniGameRunning = true;
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

            MiniGameRunning = false;
            Player.LocalPlayer.EnableMovement(true);
            OnMiniGameEnded?.Invoke();
        }

        [Client]
        private IEnumerator GetPlayerInput(List<MiniGameKeys> sequence)
        {
            yield return null;
            var startTime = Time.time * 1000;

            var i = 0;
            while (MiniGameRunning && i < sequence.Count)
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
                            Debug.Log($"The next key '{c}' was entered wrong");
                            i = 0;
                        }
                    }
                }
                
                yield return null;
            }
            
            var time = Time.time * 1000 - startTime;
            
            OnMiniGameNextKey?.Invoke(null);
            
            if (MiniGameRunning)
                CmdMiniGameSequenceEnded(Player.LocalPlayer.Username, time);
        }
        
        [Command(requiresAuthority = false)]
        private void CmdMiniGameSequenceEnded(string playerUsername, float time)
        {
            if (!MiniGameRunning || time < 0)
                return;

            Debug.Log($"Player {playerUsername} completed the minigame in {time} milliseconds");

            var player = RoundController.Instance.Players.First(p => p.Username == playerUsername);
            StartCoroutine(EndMiniGame(player));
        }

        private void OnDestroy()
        {
            Debug.Log("Destroying KillController");
            Instance = null;
        }
    }
}