﻿using System;
using System.Collections.Generic;
using UnityEngine;

namespace Utils
{
    public class FunctionTimer {
        private static List<FunctionTimer> activeTimerList;
        private static GameObject initGameObject;

        private static void InitIfNeeded() {
            if (initGameObject == null) {
                initGameObject = new GameObject("FunctionTimer_InitGameObject");
                activeTimerList = new List<FunctionTimer>();
            }
        }

        public static FunctionTimer Create(Action action, float timer, string timerName = null) {
            InitIfNeeded();
            var gameObject = new GameObject("FunctionTimer", typeof(MonoBehaviourHook));

            var functionTimer = new FunctionTimer(action, timer, timerName, gameObject);

            gameObject.GetComponent<MonoBehaviourHook>().onUpdate = functionTimer.Update;

            activeTimerList.Add(functionTimer);

            return functionTimer;
        }

        private static void RemoveTimer(FunctionTimer functionTimer) {
            InitIfNeeded();
            activeTimerList.Remove(functionTimer);
        }

        public static void StopTimer(string timerName) {
            for (var i = 0; i < activeTimerList.Count; i++) {
                if (activeTimerList[i].timerName == timerName) {
                    // Stop this timer
                    activeTimerList[i].DestroySelf();
                    i--;
                }
            }
        }

        // Dummy class to have access to MonoBehaviour functions
        private class MonoBehaviourHook : MonoBehaviour {
            public Action onUpdate;
            private void Update() => onUpdate?.Invoke();
        }

        private readonly Action action;
        private float timer;
        private readonly string timerName;
        private readonly GameObject gameObject;
        private bool isDestroyed;

        private FunctionTimer(Action action, float timer, string timerName, GameObject gameObject) {
            this.action = action;
            this.timer = timer;
            this.timerName = timerName;
            this.gameObject = gameObject;
            isDestroyed = false;
        }

        private void Update() {
            if (!isDestroyed) {
                timer -= Time.deltaTime;
                if (timer < 0) {
                    // Trigger the action
                    action();
                    DestroySelf();
                }
            }
        }

        private void DestroySelf() {
            isDestroyed = true;
            UnityEngine.Object.Destroy(gameObject);
            RemoveTimer(this);
        }
    }
}
