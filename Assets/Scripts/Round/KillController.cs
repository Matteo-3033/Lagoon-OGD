using System;
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

        private enum MiniGameKeys
        {
            UpArrow,
            DownArrow,
            LeftArrow,
            RightArrow
        }

        public static KillController Instance { get; private set; }

        public static event Action<Player> OnPlayerKilled;
        public static event Action<Player> OnPlayerRespawned;
        public static event Action OnMinigameStarting;
        public static event Action OnMinigameEnded;
        

        private bool roundInProgress;
        private bool miniGameStarted;
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
            if (miniGameStarted || killed.IsDead || (by is Player p && (killed.FieldOfView.CanSeePlayer || !p.FieldOfView.CanSeePlayer)))
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
            if (!isServer || !roundInProgress || miniGameStarted)
                return;

            if (Players.TrueForAll(p => p.FieldOfView.CanSeePlayer && !p.IsDead)) // TODO: controllare solo il triangolo interno
                StartCoroutine(StartKillMiniGame());
        }

        [Server]
        private IEnumerator StartKillMiniGame()
        {
            Debug.Log("Starting kill minigame");
            miniGameStarted = true;
            playerTimes.Clear();

            RpcDisableMovement();

            yield return new WaitForSeconds(WAIT_BEFORE_MINIGAME);

            var sequence = new List<MiniGameKeys>
            {
                MiniGameKeys.UpArrow, MiniGameKeys.DownArrow, MiniGameKeys.LeftArrow, MiniGameKeys.RightArrow
            }.OrderBy(_ => Guid.NewGuid()).ToList().GetRange(0, KEYS_PER_MINIGAME);

            RpcStartMiniGame(sequence);
        }

        [Server]
        private IEnumerator EndMiniGame()
        {
            Debug.Log("Ending kill minigame");
            RpcEnableMovement();
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
            miniGameStarted = false;
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
        private void RpcDisableMovement()
        {
            Debug.Log("Starting kill minigame");
            
            var enemies = FindObjectsByType<EnemyFSM>(FindObjectsSortMode.None);
            foreach (var enemy in enemies)
                enemy.PlayFSM();
            
            Player.LocalPlayer.EnableMovement(false);
            
            OnMinigameStarting?.Invoke();
        }

        [ClientRpc]
        private void RpcEnableMovement()
        {
            Debug.Log("Ending kill minigame");
            
            var enemies = FindObjectsByType<EnemyFSM>(FindObjectsSortMode.None);
            foreach (var enemy in enemies)
                enemy.PlayFSM();
            
            Player.LocalPlayer.EnableMovement(true);
            
            OnMinigameEnded?.Invoke();
        }

        [ClientRpc]
        private void RpcStartMiniGame(List<MiniGameKeys> sequence)
        {
            Debug.Log("Kill minigame sequence: " + string.Join(", ", sequence));
        }

        [Client]
        public void OnMiniGameSequenceCompleted(float time)
        {
            CmdMiniGameSequenceEnded(Player.LocalPlayer.Username, time);
        }

        [Command(requiresAuthority = false)]
        private void CmdMiniGameSequenceEnded(string playerUsername, float time)
        {
            if (!miniGameStarted || time < 0)
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