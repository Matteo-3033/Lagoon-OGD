using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Utils.UI
{
    [RequireComponent(typeof(Button)), RequireComponent(typeof(EventTrigger))]
    public class UnderlineOnHover : MonoBehaviour
    {
        private TextMeshProUGUI textField;
        private string originalText;
        
        private void Awake()
        {
            textField = GetComponentInChildren<TextMeshProUGUI>();
            originalText = textField.text;
            
            var trigger = GetComponent<EventTrigger>();
            
            var onHoverStartEvent = new EventTrigger.Entry
            {
                eventID = EventTriggerType.PointerEnter
            };
            onHoverStartEvent.callback.AddListener(OnHoverStart);
            
            var onHoverEndEvent = new EventTrigger.Entry
            {
                eventID = EventTriggerType.PointerExit
            };
            onHoverEndEvent.callback.AddListener(OnHoverEnd);
            
            trigger.triggers.Add(onHoverStartEvent);
            trigger.triggers.Add(onHoverEndEvent);
        }

        private void OnHoverStart(BaseEventData arg0)
        {
            originalText = textField.text;
            textField.text = $"<u>{originalText}</u>";
        }
        
        private void OnHoverEnd(BaseEventData arg0)
        {
            textField.text = originalText;
        }
    }
}