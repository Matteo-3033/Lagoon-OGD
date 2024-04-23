using TMPro;
using UnityEngine;

namespace Round.UI.Main
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class UsernameText : MonoBehaviour
    {
        [SerializeField] private bool playerUsername = true;
        private TextMeshProUGUI text;

        private void Awake()
        {
            text = GetComponent<TextMeshProUGUI>();

            if (Player.LocalPlayer != null)
                OnPlayerSpawned(Player.LocalPlayer);
            if (Player.Opponent != null)
                OnPlayerSpawned(Player.Opponent);
            Player.OnPlayerSpawned += OnPlayerSpawned;
        }

        private void OnPlayerSpawned(Player player)
        {
            if ((playerUsername && !player.isLocalPlayer) || (!playerUsername && player.isLocalPlayer))
                return;

            text.text = player.Username;
        }

        private void OnDestroy()
        {
            Player.OnPlayerSpawned -= OnPlayerSpawned;
        }
    }
}