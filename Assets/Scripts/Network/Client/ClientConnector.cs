using MasterServerToolkit.MasterServer;
using MainMenu;
using UnityEngine;

namespace Network
{
    public class ClientConnector : MonoBehaviour
    {
        [SerializeField] private GameObject dataInput;
        [SerializeField] private GameObject loadingSpinner;
        [SerializeField] private GameObject noConnection;
        
        private void Awake()
        {
            loadingSpinner.SetActive(false);
            noConnection.SetActive(false);
            dataInput.SetActive(false);
        }

        public void InitClient()
        {
            if (Mst.Server.Spawners.IsSpawnedProccess)
                return;
            
            if (!AuthBehaviour.Instance)
                Debug.LogError($"No instance of {nameof(AuthBehaviour)} found. Please add {nameof(AuthBehaviour)} to scene to be able to use auth logic");
            
            loadingSpinner.SetActive(true);
            dataInput.SetActive(false);
            noConnection.SetActive(Mst.Client.Auth.HasAuthToken());

            if (ClientToMasterConnector.Instance.IsConnected)
                OnClientConnected();
            else
            {
                ClientToMasterConnector.Instance.OnConnectedEvent.AddListener(OnClientConnected);
                ClientToMasterConnector.Instance.OnFailedConnectEvent.AddListener(OnFailedConnection);
                ClientToMasterConnector.Instance.StartConnection();
            }
        }

        private void OnClientConnected()
        {
            Debug.Log("Client connected to server");
            
            ClientToMasterConnector.Instance.OnConnectedEvent.RemoveListener(OnClientConnected);
            ClientToMasterConnector.Instance.OnFailedConnectEvent.RemoveListener(OnFailedConnection);
            
            ClientToMasterConnector.Instance.OnDisconnectedEvent.AddListener(OnClientDisconnected);
            
            AuthBehaviour.Instance.OnSignedInEvent.AddListener(OnClientAuthenticated);
            AuthBehaviour.Instance.OnSignInFailedEvent.AddListener(OnFailedConnection);
            
            AuthenticateClient();
        }

        private void AuthenticateClient()
        {
            if (Mst.Client.Auth.IsSignedIn)
            {
                OnClientAuthenticated();
                return;
            }
            
            if (!PlayerPrefs.HasKey(Utils.PlayerPrefsKeys.PlayerName))
            {
                OnFailedConnection();
                return;
            }
                
            var username = PlayerPrefs.GetString(Utils.PlayerPrefsKeys.PlayerName);
            if (username == "") return;
            
            Debug.Log("Connecting to server...");
           
            AuthBehaviour.Instance.SignIn(username);
        }
        
        private void OnClientAuthenticated()
        {
            Debug.Log("Client authenticated");
            
            AuthBehaviour.Instance.OnSignedInEvent.RemoveListener(OnClientAuthenticated);
            AuthBehaviour.Instance.OnSignInFailedEvent.RemoveListener(OnClientAuthenticated);
            UIManager.Instance.ShowMenu(UIManager.MenuKey.MainMenu);
        }
        
        private void OnClientDisconnected()
        {
            Debug.Log("Client disconnected from server");
            
            if (loadingSpinner)
                loadingSpinner.SetActive(false);
            if (dataInput)
                dataInput.SetActive(true);
            if (noConnection)
                noConnection.SetActive(true);
            
            UIManager.Instance.ShowMenu(UIManager.MenuKey.Connection);
        }

        private void OnFailedConnection()
        {
            Debug.Log("Failed to connect to server");
            
            loadingSpinner.SetActive(false);
            dataInput.SetActive(true);
            noConnection.SetActive(true);
        }

        private void OnDestroy()
        {
            if (!Application.isBatchMode)
            {
                if (ClientToMasterConnector.Instance)
                {
                    ClientToMasterConnector.Instance.OnConnectedEvent.RemoveListener(OnClientConnected);
                    ClientToMasterConnector.Instance.OnDisconnectedEvent.RemoveListener(OnClientDisconnected);
                }
                
                if (AuthBehaviour.Instance)
                {
                    AuthBehaviour.Instance.OnSignedInEvent.RemoveListener(OnClientAuthenticated);
                    AuthBehaviour.Instance.OnSignInFailedEvent.RemoveListener(OnClientAuthenticated);
                }
            }
        }
    }
}