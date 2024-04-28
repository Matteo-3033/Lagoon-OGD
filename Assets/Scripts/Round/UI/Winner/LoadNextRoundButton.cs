using Network;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils.UI;

namespace Round.UI.Winner
{
    [RequireComponent(typeof(Button))]
    public class LoadNextRoundButton : ChangeFontOnClickButton
    {
        [SerializeField] private GameObject loading;
        
        private Button button;

        protected override void Awake()
        {
            base.Awake();
            loading.SetActive(false);
            
            var textField = button.GetComponentInChildren<TextMeshProUGUI>();

            if (MatchController.Instance.HasWinner())
                textField.text = "Continue";
            else
                textField.text = "Next round";
        }

        protected override void OnClick()
        {
            RoundController.Instance.AskForNextRound();
            loading.SetActive(true);
            gameObject.SetActive(false);
        }
    }
}