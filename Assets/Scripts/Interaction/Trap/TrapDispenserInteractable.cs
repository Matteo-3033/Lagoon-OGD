using System;
using Mirror;
using Network;
using TrapModifiers;
using UnityEngine;
using Utils;

namespace Interaction.Trap
{
    public class TrapDispenserInteractable: NetworkBehaviour, IInteractable
    {
        [SerializeField] private TrapModifier trap;
        [SerializeField] private GameObject enabledState;
        [SerializeField] private GameObject disabledState;
        [SerializeField] private SpriteRenderer icon;

        public string InteractionPrompt => trap.modifierName;
        
        public static event EventHandler<TrapModifier> OnTrapNotAdded;

        public static event EventHandler<EventArgs> OnVendingMachineUsed;

        [field: SyncVar(hook = nameof(OnStateChanged))]
        public bool Working { get; private set; }

        private void Awake()
        {
            icon.sprite = trap.icon;
            icon.gameObject.SetActive(false);
        }

        public override void OnStartServer()
        {
            base.OnStartServer();
            
            EnableTrap();
        }

        public bool StartInteraction(Interactor interactor)
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
            
            Debug.Log($"Interacting with {trap.modifierName}");
            
            var player = sender.Player();
            if (player.Inventory.AddTrap(trap))
            {
                DisableTrap();
                TargetShowTrapIcon(sender);
            }
            else
                TargetNotifyTrapAlreadyOwned(sender);
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
        private void TargetNotifyTrapAlreadyOwned(NetworkConnectionToClient target)
        {
            OnTrapNotAdded?.Invoke(this, trap);
        }
        
        [TargetRpc]
        private void TargetShowTrapIcon(NetworkConnectionToClient target)
        {
            icon.gameObject.SetActive(true);
        }
        
        [Client]
        private void OnStateChanged(bool oldValue, bool newValue)
        {
            enabledState.SetActive(newValue);
            disabledState.SetActive(!newValue);
            
            if (!newValue)
                OnVendingMachineUsed?.Invoke(this, EventArgs.Empty);
        }

        #endregion
    }
}