using Network;
using UnityEngine;
using UnityEngine.UI;

namespace Test
{
    [RequireComponent(typeof(Button))]
    public class LoadNextRoundButton : MonoBehaviour
    {
        private Button button;
        
        private void Awake()
        {
            button = gameObject.GetComponent<Button>();
            button.interactable = false;
            button.onClick.AddListener(OnClick);
            MatchController.Instance.OnRoundStarted += OnRoundStarted;
        }

        private void OnRoundStarted()
        {
            button.interactable = true;    
        }

        private void OnClick()
        {
            MatchController.Instance.CheckWinningCondition();
        }
    }
}