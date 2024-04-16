using System;
using System.Collections;
using TMPro;
using UnityEngine;

namespace Round.UI.Main
{
    [RequireComponent(typeof(TextMeshProUGUI)), RequireComponent(typeof(RectTransform))]
    public class EventLoggerText: MonoBehaviour
    {
        [SerializeField] private float fadeOutAfterSeconds = 1F;
        
        private RectTransform rectTransform;
        private TextMeshProUGUI text;
        
        private float Height => rectTransform.rect.height;
        private Vector2 endPosition;
        
        
        public event Action OnHeightSurpassed;
        
        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            text = GetComponent<TextMeshProUGUI>();
        }
        
        public void Init(string msg, float endHeight, float duration)
        {
            text.text = msg;
            
            StartCoroutine(FadeOut(duration, endHeight));
        }

        private IEnumerator FadeOut(float duration, float endHeight)
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