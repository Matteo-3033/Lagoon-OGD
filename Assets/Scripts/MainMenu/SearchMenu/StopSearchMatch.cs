using Network.Client;
using UnityEngine;
using UnityEngine.UI;

namespace MainMenu.SearchMenu
{
    [RequireComponent(typeof(Button))]
    public class StopSearchMatch: MonoBehaviour
    {
        private void Awake()
        {
            var button = GetComponent<Button>();
            button.onClick.AddListener(OnClick);
        }

        private void OnClick()
        {
            MatchmakingBehaviour.Instance.StopSearch();
            UIManager.Instance.ShowMenu(UIManager.MenuKey.MainMenu);
        }
    }
}