using System;
using System.Collections;
using Mirror;
using UnityEngine;

namespace Round
{
    public class KillController: NetworkBehaviour
    {
        private const int RESPAWN_TIME = 10;

        public static KillController Instance { get; private set; }
        
        public static event Action<Player> OnPlayerKilled;
        public static event Action<Player> OnPlayerRespawned;
        
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Debug.LogWarning("KillController already exists in the scene. Deleting duplicate.");
                Destroy(gameObject);
                return;
            }

            Instance = this;
            Debug.Log("KillController initialized");
        }
        
        [Server]
        public void TryKillPlayer(Player killed, Player by, bool stealTrap)
        {
            if (killed.FieldOfView.CanSeePlayer || !by.FieldOfView.CanSeePlayer)
                return;
            
            if (killed.Inventory.StealKeyFragment())
                by.Inventory.AddKeyFragment();
            
            if (stealTrap && !by.Inventory.IsTrapBagFull() && killed.Inventory.StealTrap(out var trap))
                by.Inventory.AddTrap(trap);
            
            killed.RpcOnKilled();
            OnPlayerKilled?.Invoke(killed);
            RpcPlayerKilled(killed);
            
            StartCoroutine(RespawnPlayer(killed));
        }

        [Server]
        private IEnumerator RespawnPlayer(Player player)
        {
            yield return new WaitForSeconds(RESPAWN_TIME);
            
            player.RpcOnRespawned();
            OnPlayerRespawned?.Invoke(player);
            RpcPlayerRespawned(player);
        }
        
                
        [ClientRpc]
        private void RpcPlayerKilled(Player player)
        {
            OnPlayerKilled?.Invoke(player);
        }
        
        [ClientRpc]
        private void RpcPlayerRespawned(Player player)
        {
            OnPlayerRespawned?.Invoke(player);
        }
        
        private void OnDestroy()
        {
            Debug.Log("Destroying KillController");
            Instance = null;
        }
    }
}