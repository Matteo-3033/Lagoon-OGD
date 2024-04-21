using System.Collections;
using System.Collections.Concurrent;
using Interaction.Trap;
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

        private void Awake()
        {
            height = GetComponent<RectTransform>().rect.height;
            eventLogTemplate.gameObject.SetActive(false);
            
            if (RoundController.Loaded)
                RegisterRoundControllerCallbacks();
            else
                RoundController.OnRoundLoaded += RegisterRoundControllerCallbacks;
            
            if (Player.LocalPlayer != null)
                OnPlayerSpawned(Player.LocalPlayer);
            if (Player.Opponent != null)
                OnPlayerSpawned(Player.Opponent);
            
            Player.OnPlayerSpawned += OnPlayerSpawned;
            
            TrapInteractable.OnTrapNotAdded += LogTrapNotAdded;
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
            if (args.Enabled)
            {
                LogEvent(args.Player.isLocalPlayer
                    ? $"{args.Modifier.modifierName} activated!"
                    : $"<color=#FF0000>{Player.Opponent.Username}</color> activated {args.Modifier.modifierName}!");
            }
            else
            {
                LogEvent(args.Player.isLocalPlayer
                    ? $"{args.Modifier.modifierName} disabled!"
                    : $"<color=#FF0000>{Player.Opponent.Username}</color> disabled {args.Modifier.modifierName}!");
            }
        }

        private void LogTrapsUpdate(object sender, Inventory.OnTrapsUpdatedArgs args)
        {
            if (args.Acquired)
            {
                LogEvent($"{args.Trap.modifierName} acquired!", 0.1F);
                LogEvent($"{args.Trap.description}");
            }
            else
            {
                LogEvent($"{args.Trap.modifierName} placed");
            }
        }

        private void LogKeyFragmentUpdate(object sender, Inventory.OnKeyFragmentUpdatedArgs args)
        {
            if (args.NewValue < args.OldValue) return;

            LogEvent(args.Player.isLocalPlayer
                ? $"Key fragment acquired!"
                : $"<color=#FF0000>{Player.Opponent.Username}</color> found a badge fragment!");
        }

        private void LogNoWinningCondition()
        {
            var totalFragments = RoundController.Instance.Round.keyFragments;
            var missingFragments = totalFragments - Player.LocalPlayer.Inventory.KeyFragments;
            
            LogEvent($"You're missing <color=#FF0000>{missingFragments}/{totalFragments} badge fragments</color> to win the round");
        }
        
        private void LogTimerUpdate(float remainingTime)
        {
            if (remainingTime > 1)
                return;
            
            if (Player.LocalPlayer.Inventory.KeyFragments == Player.Opponent.Inventory.KeyFragments)
            {
                LogEvent("Time limit reached!", 0.1F);
                LogEvent("Find a badge fragment to win the round");
            }
        }
        
        private void LogTrapNotAdded(object sender, TrapModifier trap)
        {
            LogEvent($"{trap.modifierName} already in your inventory!");
        }

        [ContextMenu("Test event")]
        public void LogEvent()
        {
            LogEvent("Test event");
        }

        private void LogEvent(string msg, float fadeOutAfter = 1F)
        {
            var logMsg = new EventLoggerText.LogMessage(msg, fadeOutAfter);
            
            if (!CanSpawnNext)
                eventQueue.Enqueue(logMsg);
            else
                StartCoroutine(SpawnEvent(logMsg));
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

        private void OnDestroy()
        {
            RoundController.OnRoundLoaded -= RegisterRoundControllerCallbacks;
            Player.OnPlayerSpawned -= OnPlayerSpawned;
            TrapInteractable.OnTrapNotAdded -= LogTrapNotAdded;
        }
    }
}