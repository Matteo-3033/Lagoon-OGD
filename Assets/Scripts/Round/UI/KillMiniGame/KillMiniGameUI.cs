using UnityEngine;
using Screen = Utils.UI.Screen;

namespace Round.UI.KillMiniGame
{
    public class KillMiniGameUI : Screen
    {
        [SerializeField] private GameObject exclamationMark;
        [SerializeField] private GameObject keysToPress;
        [SerializeField] private GameObject waitingOpponent;
        
        private void Awake()
        {
            KillController.OnMiniGameNextKey += OnMiniGameNextKey;
        }

        private void OnMiniGameNextKey(KillController.MiniGameKeys? key)
        {
            if (!isActiveAndEnabled)
                return;
            
            var wait = key == null;
            
            exclamationMark.SetActive(false);
            waitingOpponent.SetActive(wait);
            keysToPress.SetActive(!wait);
        }

        public override void OnFocus()
        {
            base.OnFocus();
            
            exclamationMark.SetActive(true);
        }

        public override void OnUnfocus()
        {
            base.OnUnfocus();
            
            exclamationMark.SetActive(false);
            keysToPress.SetActive(false);
            waitingOpponent.SetActive(false);
        }
        
        private void OnDestroy()
        {
            KillController.OnMiniGameNextKey -= OnMiniGameNextKey;
        }
    }
}