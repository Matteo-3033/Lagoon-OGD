using TMPro;
using UnityEngine;

namespace Menu.UI.LobbyMenu
{
    public class PlayerPreview : MonoBehaviour
    {

        [SerializeField] private TextMeshProUGUI username;

        public void SetPlayer(Player player)
        {
            username.text = player.username;
        }
    }
}