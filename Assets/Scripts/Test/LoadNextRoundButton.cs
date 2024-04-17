using Round;
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
            button.onClick.AddListener(OnClick);
        }

        private void OnClick()
        {
            RoundController.Instance.CheckWinningCondition();
        }
    }
}