using UnityEngine;
using UnityEngine.UI;

namespace Menu.UI.SearchMenu
{
    [RequireComponent(typeof(Button))]
    public class SearchMatch : MonoBehaviour
    {
        private void Awake()
        {
            var button = GetComponent<Button>();
            button.onClick.AddListener(SearchGame);
        }

        public void SearchGame () {
            UIManager.Instance.SearchMatch();
        }
    }
}