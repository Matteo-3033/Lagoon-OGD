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
        [SerializeField] private Screen death;
        [SerializeField] private Screen killMiniGame;
        
        private readonly Dictionary<ScreenKey, Screen> menus = new();
        private readonly List<Screen> screenStack = new();

        private enum ScreenKey
        {
            Waiting,
            Countdown,
            Main,
            Winner,
            Death,
            KillMiniGame
        }
        
        private void Start()
        {
            if (!RiseNetworkManager.IsClient)
                return;
            
            AddMenu(ScreenKey.Waiting, waiting);
            AddMenu(ScreenKey.Countdown, countdown);
            AddMenu(ScreenKey.Main, round);
            AddMenu(ScreenKey.Winner, winner);
            AddMenu(ScreenKey.Death, death);
            AddMenu(ScreenKey.KillMiniGame, killMiniGame);
            
            ShowMenu(ScreenKey.Waiting);

            RegisterCallbacks();
        }
        
        private void RegisterCallbacks()
        {
            if (RoundController.HasLoaded())
                RegisterRoundCallbacks();
            else
                RoundController.OnRoundLoaded += RegisterRoundCallbacks;

            KillController.OnPlayerKilled += OnPlayerKilled;
            KillController.OnPlayerRespawned += OnPlayerRespawned;
            KillController.OnMiniGameStarting += OnKillMiniGameStarting;
            KillController.OnMiniGameEnded += OnKillMiniGameEnded;
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
        
        private void OnPlayerKilled(Player player)
        {
            if (!player.isLocalPlayer)
                return;
            
            ShowMenu(ScreenKey.Death, false);
        }
        
        private void OnPlayerRespawned(Player player)
        {
            if (!player.isLocalPlayer)
                return;
            
            ShowMenu(ScreenKey.Main);
        }
        
        private void OnKillMiniGameStarting()
        {
            ShowMenu(ScreenKey.KillMiniGame, false);
        }
        
        private void OnKillMiniGameEnded()
        {
            ShowMenu(ScreenKey.Main);
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

        private void ShowMenu(ScreenKey key, bool clearStack = true)
        {
            var newScreen = menus[key];
            var wasActive = false;
            
            if (clearStack)
            {
                foreach (var s in screenStack)
                {
                    if (s != newScreen)
                        s.OnUnfocus();
                    else wasActive = true;
                }
                screenStack.Clear();
            }
            
            screenStack.Add(newScreen);
            if (!wasActive)
                newScreen.OnFocus();
        }

        private void OnDestroy()
        {
            RoundController.OnRoundLoaded -= RegisterRoundCallbacks;
            KillController.OnPlayerKilled -= OnPlayerKilled;
            KillController.OnPlayerRespawned -= OnPlayerRespawned;
            KillController.OnMiniGameStarting -= OnKillMiniGameStarting;
            KillController.OnMiniGameEnded -= OnKillMiniGameEnded;
        }
    }
}