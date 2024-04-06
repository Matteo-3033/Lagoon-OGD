using System.Collections.Generic;
using UnityEngine;

namespace MainScene
{
    public class UIManager : MonoBehaviour
    {
        [SerializeField] private GameObject mainMenu;
        
        private readonly Dictionary<Menu, GameObject> menus = new();
        
        public enum Menu
        {
            MainMenu
        }
        
        public static UIManager Instance { get; private set; }

        private void Awake()
        {
            if (mainMenu == null)
            {
                Debug.LogError("UIManager: MainMenu is not set.");
                return;
            }

            AddMenu(Menu.MainMenu, mainMenu);
            ShowMenu(Menu.MainMenu);

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