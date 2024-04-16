using Mirror;
using Network;
using UnityEngine;
using UnityEngine.UI;

namespace Test
{
    [RequireComponent(typeof(Button))]
    public class AddKeyFragment : NetworkBehaviour
    {
        private Button button;
        
        private void Awake()
        {
            button = gameObject.GetComponent<Button>();
            button.interactable = false;
            button.onClick.AddListener(() => OnClick());
            MatchController.Instance.OnRoundStarted += OnRoundStarted;
        }

        private void OnRoundStarted()
        {
            button.interactable = true;    
        }
        
        [Command(requiresAuthority = false)]
        private void OnClick(NetworkConnectionToClient sender = null)
        {
            sender.Player().Inventory.AddKeyFragment();
        }
    }
}