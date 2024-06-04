using System.Collections;
using System.Collections.Concurrent;
using Interaction.Trap;
using Network;
using TrapModifiers;
using UnityEngine;

namespace Round.UI.Main
{
    public class EventLogger : MonoBehaviour
    {
        [SerializeField] private EventLoggerText eventLogTemplate;
        [SerializeField] private float onScreenDuration = 5F;
        
        private readonly ConcurrentQueue<EventLoggerText.LogMessage> eventQueue = new();
        
        private float height;
        
        private bool canSpawnNext = true;
        private bool CanSpawnNext
        {
            get => canSpawnNext;
            set
            {
                canSpawnNext = value;
                if (!canSpawnNext) return;
                if (eventQueue.TryDequeue(out var msg))
                    StartCoroutine(SpawnEvent(msg));
            }
        }

        private static class Duration
        {
            public const float SHORT = 0.05F;
            public const float MEDIUM = 0.2F;
            public const float LONG = 0.4F;
        }

        private void Awake()
        {
            if (!RiseNetworkManager.IsClient)
                return;
            
            height = GetComponent<RectTransform>().rect.height;
            eventLogTemplate.gameObject.SetActive(false);
            
            if (RoundController.HasLoaded())
                RegisterRoundControllerCallbacks();
            else
                RoundController.OnRoundLoaded += RegisterRoundControllerCallbacks;
            
            if (Player.LocalPlayer != null)
                OnPlayerSpawned(Player.LocalPlayer);
            if (Player.Opponent != null)
                OnPlayerSpawned(Player.Opponent);
            
            Player.OnPlayerSpawned += OnPlayerSpawned;
            
            TrapDispenserInteractable.OnTrapNotAdded += LogTrapNotAdded;

            ChancellorEffectsController.OnEffectEnabled += LogChancellorEffect;

            KillController.OnPlayerKilled += LogPlayerKilled;
            KillController.OnPlayerRespawned += LogPlayerRespawned;
        }

        private void RegisterRoundControllerCallbacks()
        {
            RoundController.Instance.OnNoWinningCondition += LogNoWinningCondition;
            RoundController.Instance.TimerUpdate += LogTimerUpdate;
        }

        private void OnPlayerSpawned(Player player)
        {
            player.Inventory.OnKeyFragmentUpdated += LogKeyFragmentUpdate;
            player.Inventory.OnStatsUpdate += LogStatsUpdate;
            
            if (player.isLocalPlayer)
                player.Inventory.OnTrapsUpdated += LogTrapsUpdate;
        }

        private void LogStatsUpdate(object sender, Inventory.OnStatsUpdatedArgs args)
        {
            if (args.Player.isLocalPlayer)
                LogStatsUpdateLocalPlayer(args);
            else LogStatsUpdateOpponent(args);
           
        }

        private void LogStatsUpdateOpponent(Inventory.OnStatsUpdatedArgs args)
        {
            switch (args.Op)
            {
                case Inventory.InventoryOp.Acquired when args.Modifier.canBeFoundInGame:
                    LogEvent($"<color=#FF0000>{Player.Opponent.Username}</color> found {args.Modifier.modifierName}!", Duration.LONG);
                    break;
                case Inventory.InventoryOp.Acquired:
                    LogEvent($"<color=#FF0000>{Player.Opponent.Username}</color> activated a super effect!");
                    break;
                case Inventory.InventoryOp.Removed:
                    LogEvent($"<color=#FF0000>{Player.Opponent.Username}</color> lost {args.Modifier.modifierName}!");
                    break;
            }
        }

        private void LogStatsUpdateLocalPlayer(Inventory.OnStatsUpdatedArgs args)
        {
            switch (args.Op)
            {
                case Inventory.InventoryOp.Acquired when args.Modifier.canBeFoundInGame:
                    LogEvent($"{args.Modifier.modifierName} activated!", Duration.LONG);
                    break;
                case Inventory.InventoryOp.Acquired:
                    LogEvent($"Super effect activated!", Duration.SHORT);
                    LogEvent($"<color=red>{args.Modifier.modifierName}</color>", Duration.LONG);
                    break;
                case Inventory.InventoryOp.Removed:
                    LogEvent($"{args.Modifier.modifierName} deactivated...");
                    break;
            }
        }

        private void LogTrapsUpdate(object sender, Inventory.OnTrapsUpdatedArgs args)
        {
            if (args.Op == Inventory.InventoryOp.Acquired)
                LogEvent($"{args.Trap.modifierName} trap acquired!", Duration.SHORT);
            else if (args.Op == Inventory.InventoryOp.Removed)
                LogEvent($"{args.Trap.modifierName} trap placed");
        }
        
        private void LogChancellorEffect(object sender, ChancellorEffectsController.OnEffectEnabledArgs args)
        {
            LogEvent("The <color=#FF0000>Chancellor</color> is awake!", Duration.SHORT);
            LogEvent($"<color=#FF0000>{args.Effect.modifierName}</color>", Duration.LONG);
        }

        private void LogKeyFragmentUpdate(object sender, Inventory.OnKeyFragmentUpdatedArgs args)
        {
            if (args.NewValue > args.OldValue)
            {
                LogEvent(args.Player.isLocalPlayer
                    ? $"Badge fragment acquired!"
                    : $"<color=#FF0000>{Player.Opponent.Username}</color> acquired a badge fragment!");

                if (args.NewValue == RoundController.Round.keyFragments)
                    LogEvent(args.Player.isLocalPlayer
                        ? "Next round teleport enabled"
                        : $"<color=#FF0000>{args.Player.Username}</color> has all badge fragments!"
                    );
            } else if (args.Player.isLocalPlayer && args.OldValue == RoundController.Round.keyFragments)
                LogEvent("Next round teleport disabled");
        }

        private void LogNoWinningCondition()
        {
            var totalFragments = RoundController.Round.keyFragments;
            var missingFragments = totalFragments - Player.LocalPlayer.Inventory.KeyFragments;

            LogEvent($"Next round teleport disabled", Duration.SHORT);
            LogEvent($"Find other <color=#FF0000>{missingFragments}/{totalFragments} badge fragments</color>", Duration.SHORT);
        }
         
        private void LogTimerUpdate(int remainingTime)
        {
            if (remainingTime > 1)
                return;
            
            if (Player.LocalPlayer.Inventory.KeyFragments == Player.Opponent.Inventory.KeyFragments)
            {
                LogEvent("Time limit reached!", Duration.SHORT);
                LogEvent("Find a badge fragment to win the round");
            }
        }
        
        private void LogTrapNotAdded(object sender, TrapModifier trap)
        {
            if (Player.LocalPlayer.Inventory.IsTrapBagFull())
                LogEvent("Trap bag is full!");
            else
                LogEvent($"{trap.modifierName} already in your inventory!");
        }

        private void LogEvent(string msg, float duration = Duration.MEDIUM)
        {
            var logMsg = new EventLoggerText.LogMessage(msg, duration);
            
            if (!CanSpawnNext)
                eventQueue.Enqueue(logMsg);
            else
                StartCoroutine(SpawnEvent(logMsg));
        }
        
        private void LogPlayerRespawned(Player player)
        {
            LogEvent($"<color=#FF0000>{player.Username}</color> respawned!", Duration.SHORT);
        }

        private void LogPlayerKilled(Player player)
        {
            LogEvent($"<color=#FF0000>{player.Username}</color> was killed!", Duration.SHORT);
        }

        private IEnumerator SpawnEvent(EventLoggerText.LogMessage msg)
        {
            CanSpawnNext = false;
            var eventLog = Instantiate(eventLogTemplate, transform);
            
            var eventLogTransform = eventLog.GetComponent<RectTransform>();
            eventLogTransform.anchorMax = new Vector2(0.5F, 0);
            eventLogTransform.anchorMin = new Vector2(0.5F, 0);
            eventLogTransform.pivot = new Vector2(0.5F, 0);
            eventLogTransform.anchoredPosition = new Vector2(0.5F, 0);
            
            var maxHeight = height - eventLogTransform.rect.height;

            // Wait one frame for the event log to be fully instantiated
            yield return null;

            eventLog.gameObject.SetActive(true);
            eventLog.OnHeightSurpassed += () => CanSpawnNext = true;
            eventLog.Init(msg, maxHeight, onScreenDuration);
        }
        
        [ContextMenu("Short test event")]
        public void ShortLogEvent()
        {
            LogEvent("Short test event", Duration.SHORT);
        }
        
        [ContextMenu("Medium test event")]
        public void MediumLogEvent()
        {
            LogEvent("Medium test event");
        }
        
        [ContextMenu("Long test event")]
        public void LongLogEvent()
        {
            LogEvent("Long test event", Duration.LONG);
        }
        
        private void OnDestroy()
        {
            RoundController.OnRoundLoaded -= RegisterRoundControllerCallbacks;
            Player.OnPlayerSpawned -= OnPlayerSpawned;
            TrapDispenserInteractable.OnTrapNotAdded -= LogTrapNotAdded;
            ChancellorEffectsController.OnEffectEnabled -= LogChancellorEffect;
            KillController.OnPlayerKilled -= LogPlayerKilled;
            KillController.OnPlayerRespawned -= LogPlayerRespawned;
        }
    }
}