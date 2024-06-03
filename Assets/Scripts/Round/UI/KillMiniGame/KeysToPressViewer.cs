using TMPro;
using UnityEngine;

namespace Round.UI.KillMiniGame
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class KeysToPressViewer : MonoBehaviour
    {
        private TextMeshProUGUI text;

        private void Awake()
        {
            text = GetComponent<TextMeshProUGUI>();
            text.text = "";
            
            KillController.OnMiniGameNextKey += OnMiniGameNextKey;
        }

        private void OnMiniGameNextKey(KillController.MiniGameKeys? key)
        {
            if (key == null)
                return;
                
            text.text = $"{(char) key}".ToUpper();
        }
    }
}