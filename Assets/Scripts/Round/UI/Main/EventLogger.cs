using System;
using System.Collections;
using System.Collections.Concurrent;
using Network;
using UnityEngine;
using UnityEngine.Serialization;

namespace Round.UI.Main
{
    public class EventLogger : MonoBehaviour
    {
        [FormerlySerializedAs("eventLogPrefab")] [SerializeField] private EventLoggerText eventLogTemplate;
        [SerializeField] private float onScreenDuration = 5F;
        
        private readonly ConcurrentQueue<string> eventQueue = new();
        
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
                    LogEvent(msg);
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
            player.Inventory.OnModifierUpdate += LogModifierUpdate;
        }

        private void LogModifierUpdate(object sender, Inventory.OnModifierUpdatedArgs args)
        {
            if (args.Enabled)
            {
                if (args.OnLocalPlayer)
                    LogEvent($"{args.Modifier.modifierName} activated!");
                else
                    LogEvent($"<color=#FF0000>{Player.Opponent.Username}</color> activated {args.Modifier.modifierName}!");
            }
            else
            {
                if (args.OnLocalPlayer)
                    LogEvent($"{args.Modifier.modifierName} disabled!");
                else
                    LogEvent($"<color=#FF0000>{Player.Opponent.Username}</color> disabled {args.Modifier.modifierName}!");
            }
        }

        private void LogKeyFragmentUpdate(object sender, Inventory.OnKeyFragmentUpdatedArgs args)
        {
            if (args.NewValue < args.OldValue) return;
            
            if (args.OnLocalPlayer)
                LogEvent($"Key fragment acquired!");
            else
                LogEvent($"<color=#FF0000>{Player.Opponent.Username}</color> found a key fragment!");
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

        private void LogEvent(string msg)
        {
            if (!CanSpawn)
                eventQueue.Enqueue(msg);
            else
                StartCoroutine(SpawnEvent(msg));
        }

        private IEnumerator SpawnEvent(string msg)
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