using System;
using Mirror;
using Network;
using TrapModifiers;
using UnityEngine;
using Utils;

namespace Interaction.Trap
{
    public class TrapInteractable: NetworkBehaviour, IInteractable
    {
        [SerializeField] private TrapModifier trap;
        [SerializeField] private GameObject enabledState;
        [SerializeField] private GameObject disabledState;

        public string InteractionPrompt => trap.modifierName;
        
        public static event EventHandler<TrapModifier> OnTrapNotAdded;

        [field: SyncVar(hook = nameof(OnStateChanged))]
        public bool Working { get; private set; }

        public override void OnStartServer()
        {
            base.OnStartServer();
            
            EnableTrap();
        }

        public bool Interact(Interactor interactor)
        {
            if (!Working)
                return false;
            
            CmdInteract();
            return true;
        }

        #region SERVER
        
        [Command(requiresAuthority = false)]
        private void CmdInteract(NetworkConnectionToClient sender = null)
        {
            if (!Working)
                return;
            
            var player = sender.Player();
            if (player.Inventory.AddTrap(trap))
                DisableTrap();
            else
                RpcNotifyTrapAlreadyOwned(sender);
        }
        
        [Server]
        private void DisableTrap()
        {
            Working = false;
            FunctionTimer.Create(EnableTrap, trap.respawnAfterSeconds);
        }

        [Server]
        private void EnableTrap()
        {
            Debug.Log($"Enabling trap {trap}");
            Working = true;
        }
        
        #endregion

        #region CLIENT

        [TargetRpc]
        private void RpcNotifyTrapAlreadyOwned(NetworkConnectionToClient target)
        {
            OnTrapNotAdded?.Invoke(this, trap);
        }
        
        private void OnStateChanged(bool oldValue, bool newValue)
        {
            enabledState.SetActive(newValue);
            disabledState.SetActive(!newValue);
        }

        #endregion
    }
}