using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Network
{
    public class ClientConnector : MonoBehaviour
    {

        [SerializeField] private GameObject dataInput;
        [SerializeField] private GameObject loadingSpinner;
        [SerializeField] private GameObject noConnection;
        
        private void Start()
        {
            loadingSpinner.SetActive(false);
            noConnection.SetActive(false);
            
            if (!Application.isBatchMode) { // Headless build
                Debug.Log ($"=== Client Build ===");
                InitClient();
            } else {
                Debug.Log ($"=== Server Build ===");
            }
        }

        private void InitClient()
        {
            RiseNetworkManager.OnClientConnected += OnClientConnected;
            RiseNetworkManager.OnClientDisconnected += OnClientDisconnected;
            
            dataInput.SetActive(true);

            noConnection.SetActive(PlayerPrefs.HasKey(Utils.PlayerPrefsKeys.PlayerName));
            
            ConnectClient();
        }
        
        public bool ConnectClient()
        {
            if (!PlayerPrefs.HasKey(Utils.PlayerPrefsKeys.PlayerName))
                return false;
                
            var username = PlayerPrefs.GetString(Utils.PlayerPrefsKeys.PlayerName);
            if (username == "") return false;
            
            Debug.Log("Connecting to server...");
            loadingSpinner.SetActive(true);
            dataInput.SetActive(false);
            RiseNetworkManager.singleton.Authenticator.SetUsername(username);
            RiseNetworkManager.singleton.StartClient();
            
            return true;
        }
        
        private void OnClientConnected()
        {
            loadingSpinner.SetActive(false);
            dataInput.SetActive(true);
            SceneManager.LoadScene(Utils.Scenes.Menu);
        }
        
        private void OnClientDisconnected()
        {
            loadingSpinner.SetActive(false);
            dataInput.SetActive(true);
        }

        private void OnDestroy()
        {
            if (!Application.isBatchMode)
            {
                RiseNetworkManager.OnClientConnected -= OnClientConnected;
                RiseNetworkManager.OnClientDisconnected -= OnClientDisconnected;
            }
        }
    }
}