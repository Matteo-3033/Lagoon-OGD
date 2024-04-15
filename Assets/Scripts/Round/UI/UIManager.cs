using Network;
using UnityEngine;

namespace Round.UI
{
    public class UIManager: MonoBehaviour
    {
        [SerializeField] private GameObject countdown;
        
        private void Awake()
        {
            countdown.SetActive(false);
            MatchController.Instance.OnRoundLoaded += OnRoundLoaded;
            MatchController.Instance.OnRoundStarted += OnRoundStarted;
        }

        private void OnRoundLoaded()
        {
            countdown.SetActive(true);
        }
        
        private void OnRoundStarted()
        {
            countdown.SetActive(false);
        }

        private void OnDestroy()
        {
            if (MatchController.Instance)
            {
                MatchController.Instance.OnRoundLoaded -= OnRoundLoaded;
                MatchController.Instance.OnRoundStarted -= OnRoundStarted;
            }
        }
    }
}