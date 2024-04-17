using TMPro;
using UnityEngine;

namespace Round.UI.Main
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class Timer : MonoBehaviour
    {
        private TextMeshProUGUI text;
        
        private float time;
        private bool running;

        private void Awake()
        {
            text = GetComponent<TextMeshProUGUI>();
        }

        public void StartTimer()
        {
            running = true;
            RoundController.Instance.TimerUpdate += remainingTime=> time = remainingTime;
            time = RoundController.Instance.Round.timeLimitMinutes * 60;
        }

        private void Update()
        {
            if (!running)
                return;
            
            if (time <= 0)
            {
                gameObject.SetActive(false);
                return;
            }
            
            time -= Mathf.Max(Time.deltaTime, 0F);
            
            var minutes = Mathf.Max(Mathf.FloorToInt(time / 60), 0F);
            var seconds = Mathf.Max(Mathf.FloorToInt(time % 60), 0F);
            
            var timeStr = $"{minutes:00}:{seconds:00}";
            if (minutes < 1)
                timeStr = $"<color=red>{timeStr}</color>";
            
            text.text = timeStr;
        }
    }
}