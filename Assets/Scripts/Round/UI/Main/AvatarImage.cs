using UnityEngine;
using UnityEngine.UI;

namespace Round.UI.Main
{
    [RequireComponent(typeof(Image))]
    public class AvatarImage : MonoBehaviour
    {
        [SerializeField] private bool playerAvatar = true;
        
        [SerializeField] private Sprite mangiagalliSprite;
        [SerializeField] private Sprite golgiSprite;
        
        private Image image;

        private void Awake()
        {
            image = GetComponent<Image>();

            if (Player.LocalPlayer != null)
                OnPlayerSpawned(Player.LocalPlayer);
            if (Player.Opponent != null)
                OnPlayerSpawned(Player.Opponent);
            Player.OnPlayerSpawned += OnPlayerSpawned;
        }

        private void OnPlayerSpawned(Player player)
        {
            if ((playerAvatar && !player.isLocalPlayer) || (!playerAvatar && player.isLocalPlayer))
                return;

            image.sprite = player.IsMangiagalli ? mangiagalliSprite : golgiSprite;
        }

        private void OnDestroy()
        {
            Player.OnPlayerSpawned -= OnPlayerSpawned;
        }
    }
}