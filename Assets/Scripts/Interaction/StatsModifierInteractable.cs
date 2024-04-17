﻿using Mirror;
using Modifiers;
using Network;
using UnityEngine;

namespace Interaction
{
    public class StatsModifierInteractable : NetworkBehaviour, IInteractable
    {
        [SerializeField] private StatsModifier modifier;

        public string InteractionPrompt => modifier.modifierName;
        
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