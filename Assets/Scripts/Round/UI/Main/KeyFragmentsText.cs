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
            
            Player.OnPlayerSpawned += OnPlayerSpawned;
            RoundController.Instance.OnRoundStarted += UpdateText;
        }
        
        private void OnPlayerSpawned(bool isLocalPlayer)
        {
            if ((playerFragments && !isLocalPlayer) || (!playerFragments && isLocalPlayer))
                return;
            
            var player = Player;
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
        }
    }
}
