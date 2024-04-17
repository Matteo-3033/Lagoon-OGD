using System;
using System.Collections.Generic;
using UnityEngine;
using Screen = Utils.Screen;

namespace Round.UI
{
    public class  UIManager: MonoBehaviour
    {
        [SerializeField] private Screen countdown;
        [SerializeField] private Screen round;
        
        private readonly Dictionary<ScreenKey, Screen> menus = new();
        private Screen currentScreen;

        private enum ScreenKey
        {
            Countdown,
            Main,
        }

        private void Awake()
        {
            RoundController.OnRoundLoaded += RegisterRoundCallbacks;
        }

        private void Start()
        {
            AddMenu(ScreenKey.Countdown, countdown);
            AddMenu(ScreenKey.Main, round);
            
            HideAll();
        }

        private void RegisterRoundCallbacks()
        {
            RoundController.Instance.OnCountdownStart += () => ShowMenu(ScreenKey.Countdown);
            RoundController.Instance.OnRoundStarted += () => ShowMenu(ScreenKey.Main);
        }

        private void AddMenu(ScreenKey key, Screen value)
        {
            if (value != null)
                menus.Add(key, value);
            value.gameObject.SetActive(false);
        }

        private void ShowMenu(ScreenKey key)
        {
            if (currentScreen != null)
                currentScreen.OnUnfocus();
            currentScreen = menus[key];
            currentScreen.OnFocus();
        }

        private void HideAll()
        {
            foreach (var menu in menus.Values)
                menu.gameObject.SetActive(false);
            
            if (currentScreen != null)
                currentScreen.OnUnfocus();
            currentScreen = null;
        }

        private void OnDestroy()
        {
            RoundController.OnRoundLoaded -= RegisterRoundCallbacks;
        }
    }
}