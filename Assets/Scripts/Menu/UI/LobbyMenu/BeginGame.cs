using UnityEngine;
using UnityEngine.UI;

namespace Menu.UI.LobbyMenu
{
    [RequireComponent(typeof(Button))]
    public class BeginGame : MonoBehaviour
    {
        [SerializeField] private GameObject loading;
    
        private void Start()
        {
            var button = gameObject.GetComponent<Button>();
            button.onClick.AddListener(OnClick);
        }

        private void OnClick()
        {
            loading.SetActive(true);
        }
    }
}