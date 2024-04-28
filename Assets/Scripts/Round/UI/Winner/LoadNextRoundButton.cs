using Network;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Round.UI.Winner
{
    [RequireComponent(typeof(Button))]
    public class LoadNextRoundButton : MonoBehaviour
    {
        [SerializeField] private GameObject loading;
        
        private Button button;
        
        private void Awake()
        {
            button = gameObject.GetComponent<Button>();
            button.onClick.AddListener(OnClick);
            
            var textField = button.GetComponentInChildren<TextMeshProUGUI>();

            if (MatchController.Instance.HasWinner())
                textField.text = "Continue";
            else
                textField.text = "Next round";
        }

        private void OnClick()
        {
            RoundController.Instance.AskForNextRound();
            loading.SetActive(true);
            gameObject.SetActive(false);
        }
    }
}