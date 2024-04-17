using System.Collections;
using System.Collections.Concurrent;
using Network;
using UnityEngine;

namespace Round.UI.Main
{
    public class EventLogger : MonoBehaviour
    {
        [SerializeField] private EventLoggerText eventLogTemplate;
        [SerializeField] private float onScreenDuration = 5F;
        
        private readonly ConcurrentQueue<EventLoggerText.LogMessage> eventQueue = new();
        
        private float height;
        
        private bool canSpawn = true;
        private bool CanSpawn
        {
            get => canSpawn;
            set
            {
                canSpawn = value;
                if (!canSpawn) return;
                if (eventQueue.TryDequeue(out var msg))
                    StartCoroutine(SpawnEvent(msg));
            }
        }

        private void Awake()
        {
            height = GetComponent<RectTransform>().rect.height;
            eventLogTemplate.gameObject.SetActive(false);
        }

        private void Start()
        {
            MatchController.Instance.OnNoWinningCondition += LogNoWinningCondition;
            Player.OnPlayerSpawned += OnPlayerSpawned;
        }

        private void OnPlayerSpawned(bool isLocalPlayer)
        {
            var player = isLocalPlayer ? Player.LocalPlayer : Player.Opponent;
            player.Inventory.OnKeyFragmentUpdated += LogKeyFragmentUpdate;
            player.Inventory.OnStatsUpdate += LogStatsUpdate;
            
            if (isLocalPlayer)
                player.Inventory.OnTrapsUpdated += LogTrapsUpdate;
        }

        private void LogStatsUpdate(object sender, Inventory.OnStatsUpdatedArgs args)
        {
            if (args.Enabled)
            {
                LogEvent(args.OnLocalPlayer
                    ? $"{args.Modifier.modifierName} activated!"
                    : $"<color=#FF0000>{Player.Opponent.Username}</color> activated {args.Modifier.modifierName}!");
            }
            else
            {
                LogEvent(args.OnLocalPlayer
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

            LogEvent(args.OnLocalPlayer
                ? $"Key fragment acquired!"
                : $"<color=#FF0000>{Player.Opponent.Username}</color> found a key fragment!");
        }

        private void LogNoWinningCondition()
        {
            var totalFragments = MatchController.Instance.CurrentRound.keyFragments;
            var missingFragments = totalFragments - Player.LocalPlayer.Inventory.KeyFragments;
            
            LogEvent($"You're missing <color=#FF0000>{missingFragments}/{totalFragments} key fragments</color> to win the round");
        }

        [ContextMenu("Test event")]
        public void LogEvent()
        {
            LogEvent("Test event");
        }

        private void LogEvent(string msg, float fadeOutAfter = 1F)
        {
            var logMsg = new EventLoggerText.LogMessage(msg, fadeOutAfter);
            
            if (!CanSpawn)
                eventQueue.Enqueue(logMsg);
            else
                StartCoroutine(SpawnEvent(logMsg));
        }

        private IEnumerator SpawnEvent(EventLoggerText.LogMessage msg)
        {
            CanSpawn = false;
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
            eventLog.OnHeightSurpassed += () => CanSpawn = true;
            eventLog.Init(msg, maxHeight, onScreenDuration);
        }

        private void OnDestroy()
        {
            if (MatchController.Instance)
                MatchController.Instance.OnNoWinningCondition -= LogNoWinningCondition;
            Player.OnPlayerSpawned -= OnPlayerSpawned;
        }
    }
}