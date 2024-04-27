using TMPro;
using UnityEngine;

namespace Round.UI.Countdown
{
    public class CountdownText: MonoBehaviour
    {
        [SerializeField] private GameObject loading;
        
        private TextMeshProUGUI text;
        
        private void Awake()
        {
            text = GetComponent<TextMeshProUGUI>();
            loading.SetActive(false);
            
            RoundController.Instance.OnCountdown += OnCountdown;
        }

        private void OnCountdown(int time)
        {
            if (time < 0)
            {
                ShowLoading();
                return;
            }
            
            text.text = time.ToString();
        }
        
        private void ShowLoading()
        {
            text.text = string.Empty;
            loading.SetActive(true);
            gameObject.SetActive(false);
        }
    }
}