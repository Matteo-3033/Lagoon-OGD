using Mirror;
using UnityEngine;
using UnityEngine.UI;

namespace Lobby
{
    [RequireComponent(typeof(Button))]
    public class ExitLobbyButton : MonoBehaviour
    {
        private void Awake()
        {
            var button = GetComponent<Button>();
            button.onClick.AddListener(OnClick);
            Player.OnPlayerSpawned += OnPlayerSpawned;
            Player.OnPlayerDespawned += OnPlayerDespawned;
        }

        private void OnPlayerDespawned(Player player)
        {
            gameObject.SetActive(true);
        }

        private void OnPlayerSpawned(Player player)
        {
            if (!player.isLocalPlayer)
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
            Player.OnPlayerDespawned -= OnPlayerDespawned;
        }
    }
}
