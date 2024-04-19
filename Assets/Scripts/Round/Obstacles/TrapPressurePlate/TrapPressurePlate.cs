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
        
        public event EventHandler<bool> OnStateChanged;

        private void Awake()
        {
            var spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            spriteRenderer.sprite = trap.icon;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag(Tags.Player))
                return;
            
            OnStateChanged?.Invoke(this, true);
            
            var player = other.GetComponent<Player>();
            
            if (isServer)
            {
                TargetEnableTrap(player.connectionToClient);
                NetworkServer.Destroy(gameObject);
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
    }
}
