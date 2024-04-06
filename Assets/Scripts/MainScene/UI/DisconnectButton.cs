using Mirror;
using UnityEngine;
using UnityEngine.UI;

namespace MainScene
{
    public class DisconnectButton : MonoBehaviour
    {
        private void Start()
        {
            var button = gameObject.GetComponent<Button>();
            button.onClick.AddListener(OnClick);
        }
        
        private void OnClick()
        {
            NetworkManager.singleton.StopClient();
            UIManager.Instance.ShowMenu(UIManager.Menu.Connection);
        }
    }
}