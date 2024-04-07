using System.Collections.Generic;
using Network.Messages;
using UnityEngine;

namespace Menu.UI.LobbyMenu
{
    public class LobbyMenu : MonoBehaviour
    {
        [SerializeField] private PlayerPreviewer playerPreviewer;

        public void SetPlayers(IEnumerable<PlayerInfo> players)
        {
            playerPreviewer.Clear();
            playerPreviewer.ShowPlayerPreviews(players);
        }
    }
}