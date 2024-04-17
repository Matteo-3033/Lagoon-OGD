using System;
using System.Collections;
using TMPro;
using UnityEngine;

namespace Round.UI.Main
{
    [RequireComponent(typeof(TextMeshProUGUI)), RequireComponent(typeof(RectTransform))]
    public class EventLoggerText: MonoBehaviour
    {
        public class LogMessage
        {
            public readonly string Msg;
            public readonly float FadeOutAfterSeconds;
            
            public LogMessage(string msg, float fadeOutAfterSeconds)
            {
                Msg = msg;
                FadeOutAfterSeconds = fadeOutAfterSeconds;
            }
        }
        
        private RectTransform rectTransform;
        private TextMeshProUGUI text;
        
        private float Height => rectTransform.rect.height;
        
        public event Action OnHeightSurpassed;
        
        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            text = GetComponent<TextMeshProUGUI>();
        }
        
        public void Init(LogMessage msg, float endHeight, float duration)
        {
            text.text = msg.Msg;
            
            StartCoroutine(FadeOut(duration, endHeight, msg.FadeOutAfterSeconds));
        }

        private IEnumerator FadeOut(float duration, float endHeight, float fadeOutAfterSeconds)
        {
            yield return new WaitForSeconds(fadeOutAfterSeconds);
            
            var elapsedTime = 0F;
            var color = text.color;
            
            var startPosition = rectTransform.anchoredPosition;
            var endPosition = new Vector2(rectTransform.anchoredPosition.x, endHeight);
            
            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                
                var newPosition = Vector3.Lerp(startPosition, endPosition, elapsedTime / duration);
                if (newPosition.y - startPosition.y > Height && rectTransform.anchoredPosition.y - startPosition.y <= Height)
                    OnHeightSurpassed?.Invoke();
                rectTransform.anchoredPosition = newPosition;
                
                color.a = Mathf.Lerp(1, 0, elapsedTime / duration);
                text.color = color;
                
                yield return null;
            }
            
            Destroy(gameObject);
        }
    }
}