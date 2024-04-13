using Network;
using UnityEngine;

namespace MainMenu.Connection
{
    public class ConnectionMenu: Menu
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