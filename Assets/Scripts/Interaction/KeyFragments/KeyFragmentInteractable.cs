using Mirror;
using Network;
using Utils.UI;

namespace Interaction.KeyFragments
{
    public class KeyFragmentInteractable: NetworkBehaviour
    { 
        private void Awake()
        {
            var longPressHandler = GetComponent<LongPressButton>();
            longPressHandler.OnInteractionCompleted += (_, _) => CmdInteract();
        }

        [Command(requiresAuthority = false)]
        private void CmdInteract(NetworkConnectionToClient sender = null)
        {
            var player = sender.Player();
            player.Inventory.AddKeyFragment();
            NetworkServer.Destroy(gameObject);
        }
    }
}