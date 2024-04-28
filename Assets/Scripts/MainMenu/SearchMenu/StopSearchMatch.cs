using Network.Client;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using Utils.UI;

namespace MainMenu.SearchMenu
{
    public class StopSearchMatch: ChangeFontOnClickButton
    {
        protected override void OnClick()
        {
            MatchmakingBehaviour.Instance.StopSearch();
            UIManager.Instance.ShowMenu(UIManager.MenuKey.MainMenu);
        }
    }
}