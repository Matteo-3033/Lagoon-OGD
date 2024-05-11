using System.Collections;
using System.Collections.Generic;
using Network;
using UnityEngine;
using Screen = Utils.UI.Screen;

namespace Round.UI
{
    public class  UIManager: MonoBehaviour
    {
        [SerializeField] private Screen waiting;
        [SerializeField] private Screen countdown;
        [SerializeField] private Screen round;
        [SerializeField] private Screen winner;
        
        private readonly Dictionary<ScreenKey, Screen> menus = new();
        private Screen currentScreen;

        private enum ScreenKey
        {
            Waiting,
            Countdown,
            Main,
            Winner
        }
        
        private void Start()
        {
            if (!RiseNetworkManager.IsClient)
                return;
            
            AddMenu(ScreenKey.Waiting, waiting);
            AddMenu(ScreenKey.Countdown, countdown);
            AddMenu(ScreenKey.Main, round);
            AddMenu(ScreenKey.Winner, winner);
            
            ShowMenu(ScreenKey.Waiting);
            
            if (RoundController.HasLoaded())
                RegisterRoundCallbacks();
            else
                RoundController.OnRoundLoaded += RegisterRoundCallbacks;
        }

        private void RegisterRoundCallbacks()
        {
            RoundController.Instance.OnCountdownStart += () => ShowMenu(ScreenKey.Countdown);
            RoundController.Instance.OnRoundStarted += () => ShowMenu(ScreenKey.Main);
            RoundController.Instance.OnRoundEnded += OnRoundEnded;
        }

        private void OnRoundEnded(Player unused)
        {
            Player.LocalPlayer.EnableMovement(false);
            
            StartCoroutine(DoShowWinnerScreen());
        }

        private IEnumerator DoShowWinnerScreen()
        {
            yield return new WaitForSeconds(1);
            
            ShowMenu(ScreenKey.Winner);
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

        private void OnDestroy()
        {
            RoundController.OnRoundLoaded -= RegisterRoundCallbacks;
        }
    }
}