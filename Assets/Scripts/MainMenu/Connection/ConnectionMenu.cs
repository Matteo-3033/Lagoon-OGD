using Network;
using UnityEngine;
using Screen = Utils.UI.Screen;

namespace MainMenu.Connection
{
    public class ConnectionMenu: Screen
    {
        [SerializeField] private ClientConnector clientConnector;
        
        private void Start()
        {
            Debug.Log(!Application.isBatchMode ? $"=== Client Build ===" : $"=== Server Build ==="); // Headless build
        }
        
        public override void OnFocus()
        {
            base.OnFocus();
            
            clientConnector.InitClient();
        }
    }
}