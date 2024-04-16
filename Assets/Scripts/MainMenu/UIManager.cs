﻿using System.Collections.Generic;
using Mirror;
using UnityEngine;
using Utils;
using Screen = Utils.Screen;

namespace MainMenu
{
    public class UIManager : MonoBehaviour
    {
        [SerializeField] private Screen connection;
        [SerializeField] private Screen main;
        [SerializeField] private Screen search;
        
        private readonly Dictionary<MenuKey, Screen> menus = new();
        private Screen currentMenu;

        public enum MenuKey
        {
            Connection,
            MainMenu,
            SearchingMatch,
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
            
            ShowMenu(MenuKey.Connection);
        }
        
        private void AddMenu(MenuKey key, Screen value)
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