using Network;
using UnityEngine;
using UnityEngine.UI;

namespace MainScene
{
    [RequireComponent(typeof(Button))]
    public class ConnectButton: MonoBehaviour
    {
        [SerializeField] private ClientConnector connector;
        
        private void Start()
        {
            var button = gameObject.GetComponent<Button>();
            button.onClick.AddListener(OnClick);
        }
        
        private void OnClick()
        {
            connector.ConnectClient();
        }
    }
}