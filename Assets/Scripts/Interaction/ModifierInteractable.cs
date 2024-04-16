using Mirror;
using Modifiers;
using Network;
using UnityEngine;

namespace Interaction
{
    public class ModifierInteractable : NetworkBehaviour, IInteractable
    {
        [SerializeField] private Modifier modifier;
        
        public string InteractionPrompt { get; }
        public bool Interact(Interactor interactor)
        {
            CmdInteract();
            return true;
        }

        [Command(requiresAuthority = false)]
        private void CmdInteract(NetworkConnectionToClient sender = null)
        {
            var target = modifier.isBuff ? sender.Player() : sender.Opponent();
            
            target.Inventory.AddModifier(modifier);
            
            NetworkServer.Destroy(gameObject);
        }
    }
}