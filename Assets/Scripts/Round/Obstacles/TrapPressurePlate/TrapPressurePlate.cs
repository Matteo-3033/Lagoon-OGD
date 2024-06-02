using System;
using System.Linq;
using Mirror;
using TrapModifiers;
using UnityEngine;
using Utils;

namespace Round.Obstacles.TrapPressurePlate
{
    public class TrapPressurePlate : NetworkBehaviour
    {
        public bool sendRpcToOpponent = false;
        [SerializeField] private TrapModifier trap;

        private bool activated;

        public static event EventHandler<bool> OnStateChanged;

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

            if (activated) return;
            activated = true;

            OnStateChanged?.Invoke(this, true);

            Player player = other.GetComponent<Player>();

            if (sendRpcToOpponent)
            {
                player = RoundController.Instance.Players.First(p => p.connectionToClient.connectionId != player.connectionToClient.connectionId);
            }

            if (isServer)
                TargetEnableTrap(player.connectionToClient);
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