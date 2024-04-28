using Network;
using TMPro;
using UnityEngine;

namespace Round.UI.Winner
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class WinnerSubtext: MonoBehaviour
    {
        private void Awake()
        {
            var textField = GetComponent<TextMeshProUGUI>();
            
            if (MatchController.Instance.HasWinner())
                textField.text += "wins the match!";
            else
                textField.text += "wins the round!";
        }
    }
}