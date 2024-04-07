using UnityEngine;
using UnityEngine.UI;

namespace Menu.UI.SearchMenu
{
    [RequireComponent(typeof(Button))]
    public class StopSearchMatch: MonoBehaviour
    {
        private void Awake()
        {
            var button = GetComponent<Button>();
            button.onClick.AddListener(StopSearch);
        }

        private static void StopSearch()
        {
            UIManager.Instance.StopSearch();
        }
    }
}