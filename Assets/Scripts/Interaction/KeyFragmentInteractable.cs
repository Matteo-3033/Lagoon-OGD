using Mirror;
using Network;

namespace Interaction
{
    public class KeyFragmentInteractable: NetworkBehaviour, IInteractable
    {
        public string InteractionPrompt => "Badge Fragment";
        
        public bool Interact(Interactor interactor)
        {
            // TODO: check on client if the player has completed the minigame (prolonged button press)
            CmdInteract();
            return true;
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