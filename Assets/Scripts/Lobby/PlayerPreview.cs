using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Lobby
{
    public class PlayerPreview : MonoBehaviour
    {
        [SerializeField] private Image avatar;
        [SerializeField] private TextMeshProUGUI username;
        [SerializeField] private Badge badge;
        
        [SerializeField] private Sprite mangiagalliAvatar;
        [SerializeField] private Sprite golgiAvatar;

        public void SetPlayer(Player player)
        {
            username.text = player.Username;
            badge.SetScore(player.Score);
            avatar.sprite = player.IsMangiagalli ? mangiagalliAvatar : golgiAvatar;
        }
    }
}