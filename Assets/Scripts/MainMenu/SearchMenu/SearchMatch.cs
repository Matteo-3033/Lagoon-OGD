using Network.Client;
using UnityEngine;
using Utils.UI;

namespace MainMenu.SearchMenu
{
    public class SearchMatch : ChangeFontOnClickButton
    {
        private void Start()
        {
            if (!MatchmakingBehaviour.Instance)
                Debug.LogError($"No instance of {nameof(MatchmakingBehaviour)} found. Please add {nameof(MatchmakingBehaviour)} to scene to be able to use auth logic");
        }

        protected override void OnClick() {
            MatchmakingBehaviour.Instance.SearchMatch(3);
            UIManager.Instance.ShowMenu(UIManager.MenuKey.SearchingMatch);
        }
    }
}