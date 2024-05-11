using Mirror;
using Modifiers;
using Network;
using UnityEngine;

namespace Interaction
{
    public class StatsModifierInteractable : NetworkBehaviour, IInteractable
    {
        [SerializeField] private StatsModifier modifier;

        public string InteractionPrompt => modifier.modifierName;
        private bool used;

        private void Awake()
        {
            if (!modifier.canBeFoundInGame)
                Debug.LogError(
                    $"Stats modifier {modifier.modifierName} is not meant to be found in game.\n" +
                    "You should use it as a synergy."
                );
        }

        public bool StartInteraction(Interactor interactor)
        {
            Debug.Log("Interacting with " + modifier.modifierName);
            CmdInteract();
            return true;
        }

        [Command(requiresAuthority = false)]
        private void CmdInteract(NetworkConnectionToClient sender = null)
        {
            if (used) return;
            
            var target = modifier.isBuff ? sender.Player() : sender.Opponent();
            
            if (target.Inventory.AddStatsModifier(modifier))
                used = true;
                
            NetworkServer.Destroy(gameObject);
        }
    }
}