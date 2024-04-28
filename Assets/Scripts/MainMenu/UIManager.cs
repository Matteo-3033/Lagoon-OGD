using UnityEngine;
using UnityEngine.Serialization;
using Screen = Utils.Screen;

namespace MainMenu
{
    public class UIManager : MonoBehaviour
    {
        [SerializeField] private Screen connectionMenu;
        [SerializeField] private TabletUIManager tabletUIManager;
        [SerializeField] private CinemachineManager cinemachineManager;
        
        private MenuKey? currentMenu = null;

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
            Debug.Log("UIManager: Start");
            ShowMenu(MenuKey.Connection);
        }

        public void ShowMenu(MenuKey menuKey)
        {
            if (currentMenu == menuKey) return;
            
            if (currentMenu == MenuKey.Connection)
            {
                cinemachineManager.ShowMainMenu();
                connectionMenu.OnUnfocus();
                tabletUIManager.ShowMenu(MenuKey.MainMenu);
            }
            else if (menuKey == MenuKey.Connection)
            {
                cinemachineManager.ShowConnectionMenu();
                connectionMenu.OnFocus();
            }
            else
                tabletUIManager.ShowMenu(menuKey);
            
            currentMenu = menuKey;
        }
    }
}