using TMPro;
using UnityEngine;

namespace Round.UI.Countdown
{
    public class CountdownText: MonoBehaviour
    {
        private TextMeshProUGUI text;
        
        private void Awake()
        {
            text = GetComponent<TextMeshProUGUI>();
            RoundController.Instance.OnCountdown += OnCountdown;
        }

        private void OnCountdown(int time)
        {
            text.text = time.ToString();
        }
    }
}