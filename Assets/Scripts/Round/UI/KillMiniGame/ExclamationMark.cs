using UnityEngine;

namespace Round.UI.KillMiniGame
{
    public class ExclamationMark : MonoBehaviour
    {
        private void Awake()
        {
            KillController.OnMiniGameNextKey += OnMiniGameNextKey;
        }

        private void OnMiniGameNextKey(KillController.MiniGameKeys? key)
        {
            gameObject.SetActive(false);
        }

        private void OnDestroy()
        {
            KillController.OnMiniGameNextKey -= OnMiniGameNextKey;
        }
    }
}