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

        private void OnPlayerDespawned(bool obj)
        {
            gameObject.SetActive(true);
        }

        private void OnPlayerSpawned(bool isLocalPlayer)
        {
            if (!isLocalPlayer)
                gameObject.SetActive(false);
        }

        private void OnClick()
        {
            if (Player.Opponent != null)
                return;
            NetworkClient.Disconnect();
        }
    }
}
