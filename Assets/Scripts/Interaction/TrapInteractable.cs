using System;
using System.Collections;
using Mirror;
using Network;
using TrapModifiers;
using UnityEngine;
using Utils;

namespace Interaction
{
    public class TrapInteractable: NetworkBehaviour, IInteractable
    {
        [SerializeField] private TrapModifier trap;

        public string InteractionPrompt => trap.modifierName;
        
        public static event EventHandler<TrapModifier> OnTrapNotAdded; 
        
        public bool Interact(Interactor interactor)
        {
            CmdInteract();
            return true;
        }

        [Command(requiresAuthority = false)]
        private void CmdInteract(NetworkConnectionToClient sender = null)
        {
            var player = sender.Player(); 
            if (player.Inventory.AddTrap(trap))
            {
                NetworkServer.UnSpawn(gameObject);
                FunctionTimer.Create(RespawnTrap, trap.respawnAfterSeconds);
            }
            else
                RpcNotifyTrapAlreadyOwned(sender);
        }

        [TargetRpc]
        private void RpcNotifyTrapAlreadyOwned(NetworkConnectionToClient target)
        {
            OnTrapNotAdded?.Invoke(this, trap);
        }

        [Server]
        private void RespawnTrap()
        {
            Debug.Log($"Respawning trap {trap}");
            NetworkServer.Spawn(gameObject);
        }
    }
}