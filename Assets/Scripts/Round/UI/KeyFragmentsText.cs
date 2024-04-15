using Network;
using TMPro;
using UnityEngine;

namespace Round.UI
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class KeyFragmentsText : MonoBehaviour
    {
        [SerializeField] private bool playerFragments = true;
        private TextMeshProUGUI text;
    
        private void Awake()
        {
            text = GetComponent<TextMeshProUGUI>();
            text.gameObject.SetActive(false);
        
            Player.OnPlayerSpawned += OnPlayerSpawned;
        }

        private void Start()
        {
            MatchController.Instance.OnRoundLoaded += OnRoundLoaded;
        }

        private void OnRoundLoaded()
        {
            text.gameObject.SetActive(true);
            text.text = $"1/{MatchController.Instance.CurrentRound.keyFragments}";
        }

        private void OnPlayerSpawned(bool isLocalPlayer)
        {
            Player player;
            switch (playerFragments)
            {
                case true when isLocalPlayer:
                    player = Player.LocalPlayer;
                    break;
                case false when !isLocalPlayer:
                    player = Player.Opponent;
                    break;
                default:
                    return;
            }
            
            player.Inventory.OnKeyFragmentUpdated += OnKeyFragmentUpdate;
            text.text = $"{player.Inventory.KeyFragments}/{MatchController.Instance.CurrentRound.keyFragments}";
        }

        private void OnKeyFragmentUpdate(object sender, Inventory.OnKeyFragmentUpdatedArgs args)
        {
            text.text = $"{args.NewValue}/{MatchController.Instance.CurrentRound.keyFragments}";
        }

        private void OnDestroy()
        {
            Player.OnPlayerSpawned -= OnPlayerSpawned;
            if (MatchController.Instance)
                MatchController.Instance.OnRoundLoaded -= OnRoundLoaded;
        }
    }
}
