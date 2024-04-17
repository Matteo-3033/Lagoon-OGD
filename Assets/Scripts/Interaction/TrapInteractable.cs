using Mirror;
using Network;
using TrapModifiers;
using UnityEngine;

namespace Interaction
{
    public class TrapInteractable: NetworkBehaviour, IInteractable
    {
        [SerializeField] private TrapModifier trap;

        public string InteractionPrompt => trap.modifierName;
        
        public bool Interact(Interactor interactor)
        {
            CmdInteract();
            return true;
        }

        [Command(requiresAuthority = false)]
        private void CmdInteract(NetworkConnectionToClient sender = null)
        {
            var player = sender.Player(); 
            player.Inventory.AddTrap(trap);
            NetworkServer.Destroy(gameObject);
        }
    }
}