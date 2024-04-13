using Network.Client;
using UnityEngine;
using UnityEngine.UI;

namespace MainMenu.SearchMenu
{
    [RequireComponent(typeof(Button))]
    public class SearchMatch : MonoBehaviour
    {
        private void Awake()
        {
            var button = GetComponent<Button>();
            button.onClick.AddListener(OnClick);
        }

        private void Start()
        {
            if (!MatchmakingBehaviour.Instance)
                Debug.LogError($"No instance of {nameof(MatchmakingBehaviour)} found. Please add {nameof(MatchmakingBehaviour)} to scene to be able to use auth logic");
        }

        private void OnClick () {
            MatchmakingBehaviour.Instance.SearchMatch(3);
            UIManager.Instance.ShowMenu(UIManager.MenuKey.SearchingMatch);
        }
    }
}