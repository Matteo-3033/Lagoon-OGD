using Mirror;
using UnityEngine;
using Utils.UI;

namespace Lobby
{
    public class ExitLobbyButton : ChangeFontOnClickButton
    {
        [SerializeField] private GameObject loading;
        
        protected override void Awake()
        {
            base.Awake();
            
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

        protected override void OnClick()
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
