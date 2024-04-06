using Mirror;
using UnityEngine;
using UnityEngine.UI;

namespace MainScene
{
    public class ConnectButton: MonoBehaviour
    {
        private void Start()
        {
            var button = gameObject.GetComponent<Button>();
            button.onClick.AddListener(OnClick);
        }
        
        private static void OnClick()
        {
            NetworkManager.singleton.StartClient();
            UIManager.Instance.ShowMenu(UIManager.Menu.Loading);
        }
    }
}