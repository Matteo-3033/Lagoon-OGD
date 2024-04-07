using System.Collections.Generic;
using UnityEngine;

namespace Menu.UI
{
    public class UIManager : MonoBehaviour
    {
        [SerializeField] private Menu main;
        [SerializeField] private Menu search;
        [SerializeField] private Menu lobby;
        
        private readonly Dictionary<MenuKey, Menu> menus = new();
        private Menu currentMenu;
        
        public enum MenuKey
        {
            MainMenu,
            SearchingMatch,
            Lobby
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
            
            if (main == null)
            {
                Debug.LogError("UIManager: MainMenu is not set.");
                return;
            }

            AddMenu(MenuKey.MainMenu, main);
            AddMenu(MenuKey.SearchingMatch, search);
            AddMenu(MenuKey.Lobby, lobby);
            
            ShowMenu(MenuKey.MainMenu);
        }
        
        
        private void AddMenu(MenuKey key, Menu value)
        {
            if (value != null)
                menus.Add(key, value);
        }

        public void ShowMenu(MenuKey menuKey)
        {
            if (currentMenu != null)
                currentMenu.OnUnfocus();
            currentMenu = menus[menuKey];
            currentMenu.OnFocus();
        }
    }
}