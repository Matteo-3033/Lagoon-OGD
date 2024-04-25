

using System.Collections.Generic;
using UnityEngine;
using Screen = Utils.Screen;

namespace MainMenu
{
    public class TabletUIManager: MonoBehaviour
    {
        [SerializeField] private Screen mainMenu;
        [SerializeField] private Screen searchMatchMenu;
        
        private readonly Dictionary<UIManager.MenuKey, Screen> menus = new();
        private Screen currentMenu;
        
        private void Start()
        {
            if (mainMenu == null)
            {
                Debug.LogError("UIManager: Main Menu is not set.");
                return;
            }
            
            AddMenu(UIManager.MenuKey.MainMenu, mainMenu);
            AddMenu(UIManager.MenuKey.SearchingMatch, searchMatchMenu);
            
            ShowMenu(UIManager.MenuKey.MainMenu);
        }
        
        private void AddMenu(UIManager.MenuKey key, Screen value)
        {
            if (value != null)
                menus.Add(key, value);
            value.gameObject.SetActive(false);
        }

        public void ShowMenu(UIManager.MenuKey menuKey)
        {
            if (currentMenu != null)
                currentMenu.OnUnfocus();
            
            currentMenu = menus[menuKey];
            currentMenu.OnFocus();
        }
    }
}