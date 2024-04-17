using TMPro;
using UnityEngine;

namespace Round.UI.Main
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class KeyFragmentsText : MonoBehaviour
    {
        [SerializeField] private bool playerFragments = true;
        private TextMeshProUGUI text;
        
        private Player Player => playerFragments ? Player.LocalPlayer : Player.Opponent;
    
        private void Awake()
        {
            text = GetComponent<TextMeshProUGUI>();
            
            if (Player.LocalPlayer != null)
                OnPlayerSpawned(Player.LocalPlayer);
            if (Player.Opponent != null)
                OnPlayerSpawned(Player.Opponent);
            Player.OnPlayerSpawned += OnPlayerSpawned;
            
            if (RoundController.Loaded)
                RegisterRoundControllerCallbacks();
            else
                RoundController.OnRoundLoaded += RegisterRoundControllerCallbacks;
        }

        private void RegisterRoundControllerCallbacks()
        {
            RoundController.Instance.OnRoundStarted += UpdateText;
        }

        private void OnPlayerSpawned(Player player)
        {
            if ((playerFragments && !player.isLocalPlayer) || (!playerFragments && player.isLocalPlayer))
                return;
            
            player.Inventory.OnKeyFragmentUpdated += OnKeyFragmentUpdate;
            
            UpdateText();
        }

        private void OnKeyFragmentUpdate(object sender, Inventory.OnKeyFragmentUpdatedArgs args)
        {
            UpdateText();
        }

        private void UpdateText()
        {
            var player = Player;

            var owned = 1;
            if (player != null)
                owned = player.Inventory.KeyFragments;

            var total = 3;
            if (RoundController.Instance != null && RoundController.Instance.Round != null)
                total = RoundController.Instance.Round.keyFragments;
            
            text.text = $"{owned}/{total}";
        }

        private void OnDestroy()
        {
            Player.OnPlayerSpawned -= OnPlayerSpawned;
            RoundController.OnRoundLoaded -= RegisterRoundControllerCallbacks;
        }
    }
}
