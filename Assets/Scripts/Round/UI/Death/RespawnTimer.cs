using System.Collections;
using TMPro;
using UnityEngine;

namespace Round.UI.Death
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class RespawnTimer: MonoBehaviour
    {
        private TextMeshProUGUI text;
        private Coroutine timerCoroutine;
        
        private void Awake()
        {
            text = GetComponent<TextMeshProUGUI>();
        }

        public void StartTimer()
        {
            timerCoroutine = StartCoroutine(Countdown());
        }

        public void StopTimer()
        {
            StopCoroutine(timerCoroutine);
        }

        private IEnumerator Countdown()
        {
            for (var i = KillController.RESPAWN_TIME; i >= 0; i--)
            {
                UpdateText(i);
                yield return new WaitForSeconds(1);
            }

            while (true)
            {
                for (var i = 0; i < 3; i++)
                {
                    SetDots(i);
                    yield return new WaitForSeconds(0.5F);
                }
            }
        }

        private void UpdateText(int i)
        {
            text.text = $"Respawning in {i}...";
        }
        
        private void SetDots(int i)
        {
            var dots = new string('.', i);
            text.text = $"Waiting for server{dots}";
        }
    }
}