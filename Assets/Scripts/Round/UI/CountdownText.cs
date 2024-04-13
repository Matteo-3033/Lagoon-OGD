using Network;
using TMPro;
using UnityEngine;

namespace Round.UI
{
    public class CountdownText: MonoBehaviour
    {
        private TextMeshProUGUI text;
        
        private void Awake()
        {
            text = GetComponent<TextMeshProUGUI>();
            MatchController.Instance.OnCountdown += OnCountdown;
        }

        private void OnCountdown(int time)
        {
            text.text = time.ToString();
        }
    }
}