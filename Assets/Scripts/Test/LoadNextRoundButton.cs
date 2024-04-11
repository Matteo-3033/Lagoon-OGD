using Network;
using UnityEngine;
using UnityEngine.UI;

namespace Test
{
    [RequireComponent(typeof(Button))]
    public class LoadNextRoundButton : MonoBehaviour
    {
        private void Awake()
        {
            var button = gameObject.GetComponent<Button>();
            button.interactable = true;
            button.onClick.AddListener(OnClick);
        }

        private void OnClick()
        {
            Debug.Log("Load next round button clicked");
            MatchController.Instance.CheckWinningCondition();
        }
    }
}