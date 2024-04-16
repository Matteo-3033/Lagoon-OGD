using Network;
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
            MatchController.Instance.OnRoundLoaded += OnRoundLoaded;
            
            UpdateText();
        }

        private void OnRoundLoaded()
        {
            UpdateText();
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

            var playerFragments = 1;
            if (player != null)
                playerFragments = player.Inventory.KeyFragments;

            var totalFragments = 3;
            if (MatchController.Instance.CurrentRound != null)
                totalFragments = MatchController.Instance.CurrentRound.keyFragments;
            
            text.text = $"{playerFragments}/{totalFragments}";
        }

        private void OnDestroy()
        {
            Player.OnPlayerSpawned -= OnPlayerSpawned;
            if (MatchController.Instance)
                MatchController.Instance.OnRoundLoaded -= OnRoundLoaded;
        }
    }
}
