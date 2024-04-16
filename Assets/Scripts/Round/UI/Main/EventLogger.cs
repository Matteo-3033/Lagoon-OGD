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
    }
}