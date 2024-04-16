using System.Collections.Generic;
using Network;
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

        public enum ScreenKey
        {
            Countdown,
            Main,
        }
        
        public static UIManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Debug.LogWarning("UIManager: Instance already exists.");
                Destroy(this);
                return;
            }
            Instance = this;
            
            MatchController.Instance.OnRoundLoaded += OnRoundLoaded;
            MatchController.Instance.OnRoundStarted += OnRoundStarted;
        }

        private void Start()
        {
            AddMenu(ScreenKey.Countdown, countdown);
            AddMenu(ScreenKey.Main, round);
            
            HideAll();
        }
        
        private void AddMenu(ScreenKey key, Screen value)
        {
            if (value != null)
                menus.Add(key, value);
            value.gameObject.SetActive(false);
        }

        public void ShowMenu(ScreenKey key)
        {
            if (currentScreen != null)
                currentScreen.OnUnfocus();
            currentScreen = menus[key];
            currentScreen.OnFocus();
        }

        private void HideAll()
        {
            currentScreen.OnUnfocus();
            currentScreen = null;
        }
        
        private void OnRoundLoaded()
        {
            ShowMenu(ScreenKey.Countdown);
        }
        
        private void OnRoundStarted()
        {
            ShowMenu(ScreenKey.Main);
        }
        
        private void OnDestroy()
        {
            if (MatchController.Instance)
            {
                MatchController.Instance.OnRoundLoaded -= OnRoundLoaded;
                MatchController.Instance.OnRoundStarted -= OnRoundStarted;
            }
        }
    }
}