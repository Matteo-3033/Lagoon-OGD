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
            MatchController.Instance.OnRoundLoaded += () => countdown.SetActive(true);
            MatchController.Instance.OnRoundStarted += () => countdown.SetActive(false);
        }
    }
}