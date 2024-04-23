using Mirror;
using UnityEngine;
using UnityEngine.UI;

namespace Lobby
{
    [RequireComponent(typeof(Button))]
    public class ExitLobbyButton : MonoBehaviour
    {
        [SerializeField] private GameObject loading;
        
        private void Awake()
        {
            var button = GetComponent<Button>();
            button.onClick.AddListener(OnClick);
            
            loading.SetActive(false);

            if (Player.Opponent != null)
                ShowLoading();
            
            Player.OnPlayerSpawned += OnPlayerSpawned;
        }

        private void OnPlayerSpawned(Player player)
        {
            if (!player.isLocalPlayer)
                ShowLoading();
        }

        private void ShowLoading()
        {
            loading.SetActive(true);
            gameObject.SetActive(false);
        }

        private void OnClick()
        {
            if (Player.Opponent != null)
                return;
            
            NetworkClient.Disconnect();
        }

        private void OnDestroy()
        {
            Player.OnPlayerSpawned -= OnPlayerSpawned;
        }
    }
}
