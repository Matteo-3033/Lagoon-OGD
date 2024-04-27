using Network;
using TMPro;
using UnityEngine;

namespace Round.UI.Winner
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class WinnerText: MonoBehaviour
    {
        private void Awake()
        {
            var textField = GetComponent<TextMeshProUGUI>();
            
            var winner = RoundController.Instance.Winner;

            textField.text = $"<color=red>{winner.Username}</color>\n";
            
            if (MatchController.Instance.HasWinner())
                textField.text += "wins the match!";
            else
                textField.text += "wins the round!";
        }
    }
}