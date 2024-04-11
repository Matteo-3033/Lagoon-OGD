using System.Collections.Generic;
using Mirror;
using UnityEngine;

namespace Menu.UI
{
    public class UIManager : MonoBehaviour
    {
        [SerializeField] private Menu connection;
        [SerializeField] private Menu main;
        [SerializeField] private Menu search;
        [SerializeField] private Menu lobby;
        
        private readonly Dictionary<MenuKey, Menu> menus = new();
        private Menu currentMenu;
        private bool searchingMatch;

        public enum MenuKey
        {
            Connection,
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
        }

        private void Start()
        {
            if (connection == null)
            {
                Debug.LogError("UIManager: Connection is not set.");
                return;
            }

            AddMenu(MenuKey.Connection, connection);
            AddMenu(MenuKey.MainMenu, main);
            AddMenu(MenuKey.SearchingMatch, search);
            AddMenu(MenuKey.Lobby, lobby);
            
            ShowMenu(MenuKey.Connection);
        }
        
        private void AddMenu(MenuKey key, Menu value)
        {
            if (value != null)
                menus.Add(key, value);
            value.gameObject.SetActive(false);
        }

        public void ShowMenu(MenuKey menuKey)
        {
            if (currentMenu != null)
                currentMenu.OnUnfocus();
            currentMenu = menus[menuKey];
            currentMenu.OnFocus();
        }

        private void HideAll()
        {
            currentMenu.OnUnfocus();
            currentMenu = null;
        }
    }
}