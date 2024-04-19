using System;
using Mirror;
using TrapModifiers;
using UnityEngine;
using Utils;

namespace Round.Obstacles.TrapPressurePlate
{
    public class TrapPressurePlate : NetworkBehaviour
    {
        [SerializeField] private TrapModifier trap;
        
        private bool activated;
        
        public event EventHandler<bool> OnStateChanged;

        private void Awake()
        {
            var spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            spriteRenderer.sprite = trap.icon;
        }

        public override void OnStartServer()
        {
            base.OnStartServer();
            
            var animator = GetComponent<TrapPressurePlateAnimator>();
            animator.OnDisappearAnimationDone += DestroyTrap;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag(Tags.Player))
                return;
            
            OnStateChanged?.Invoke(this, true);
            
            var player = other.GetComponent<Player>();
            
            if (!activated && isServer)
            {
                activated = true;
                TargetEnableTrap(player.connectionToClient);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (!other.CompareTag(Tags.Player))
                return;
            
            OnStateChanged?.Invoke(this, false);
        }
        
        [TargetRpc]
        private void TargetEnableTrap(NetworkConnectionToClient target)
        {
            trap.Enable();
        }
        
        private void DestroyTrap(object sender, EventArgs args)
        {
            NetworkServer.Destroy(gameObject);
        }
    }
}
