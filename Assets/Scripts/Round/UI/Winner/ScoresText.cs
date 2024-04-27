using Network;
using TMPro;
using UnityEngine;

namespace Round.UI.Winner
{
    public class ScoresText: MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI usernameText;
        [SerializeField] private TextMeshProUGUI scoreText;
        [SerializeField] private bool playerScore;
        
        private Player Player => playerScore ? Player.LocalPlayer : Player.Opponent;

        private void Awake()
        {
            usernameText.text = Player.Username;

            MatchController.Instance.OnPlayersDataChanged += OnPlayerDataChanged;
            
            UpdateScore();
        }

        private void OnPlayerDataChanged(string username)
        {
            if (Player.Username == username)
                UpdateScore();
        }

        private void UpdateScore()
        {
            var data = MatchController.Instance.Players[Player.Username];
            scoreText.text = $"{data.RoundsWon}/{MatchController.Instance.RoundCnt}";
        }

        private void OnDestroy()
        {
            if (MatchController.Instance)
                MatchController.Instance.OnPlayersDataChanged -= OnPlayerDataChanged;
        }
    }
}