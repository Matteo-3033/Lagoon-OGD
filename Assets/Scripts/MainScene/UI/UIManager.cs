using System.Collections.Generic;
using UnityEngine;

namespace MainScene
{
    public class UIManager : MonoBehaviour
    {
        [SerializeField] private GameObject connectionMenu;
        [SerializeField] private GameObject mainMenu;
        [SerializeField] private GameObject loadingMenu;
        
        private readonly Dictionary<Menu, GameObject> menus = new Dictionary<Menu, GameObject>();
        
        public enum Menu
        {
            Connection,
            MainMenu,
            Loading
        }
        
        public static UIManager Instance { get; private set; }

        private void Awake()
        {
            if (connectionMenu == null)
            {
                Debug.LogError("UIManager: ConnectionMenu is not set.");
                return;
            }

            AddMenu(Menu.Connection, connectionMenu);
            AddMenu(Menu.MainMenu, mainMenu);
            AddMenu(Menu.Loading, loadingMenu);
            ShowMenu(Menu.Connection);

            if (Instance != null || Instance == this)
            {
                Debug.LogError("UIManager: Instance already exists.");
                return;
            }
            Instance = this;
        }

        private void AddMenu(Menu key, GameObject value)
        {
            if (value != null)
                menus.Add(key, value);
        }

        public void ShowMenu(Menu menu)
        {
            foreach (var m in menus)
                m.Value.SetActive(m.Key == menu);
        }
    }
}