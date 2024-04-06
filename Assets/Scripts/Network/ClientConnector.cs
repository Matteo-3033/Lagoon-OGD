using Mirror;
using Server;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Network {
    public class ClientConnector : MonoBehaviour
    {

        [SerializeField] private GameObject dataInput;
        [SerializeField] private GameObject loadingSpinner;
        [SerializeField] private GameObject noConnection;
        
        private void Start () {
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
            
            dataInput.SetActive(true);

            noConnection.SetActive(PlayerPrefs.HasKey(Utils.PlayerPrefsKeys.PlayerName));

            if (PlayerPrefs.HasKey(Utils.PlayerPrefsKeys.PlayerName))
                ConnectClient();
        }

        public void ConnectClient()
        {
            Debug.Log("Connecting to server...");
            loadingSpinner.SetActive(true);
            dataInput.SetActive(false);
            RiseNetworkManager.singleton.StartClient();
        }
        
        private void OnClientConnected(NetworkConnectionToClient unused)
        {
            loadingSpinner.SetActive(false);
            dataInput.SetActive(true);
            SceneManager.LoadScene(Utils.Scenes.MainMenu);
        }
    }
}