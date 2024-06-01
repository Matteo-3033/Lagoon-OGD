using System;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using Modifiers;
using TrapModifiers;
using UnityEngine;
using UnityEngine.Rendering;

public class Inventory : NetworkBehaviour
{
    [SerializeField] private LayerMask obstaclesMask;
    [SerializeField] private int trapsCapacity = 3;
    
    [field: SyncVar(hook = nameof(OnKeyFragmentsUpdated))]
    public int KeyFragments { get; private set; } = 1;

    private readonly SyncList<StatsModifier> stats = new();
    private readonly SyncList<TrapModifier> traps = new();
    
    private Player player;

    public IEnumerable<StatsModifier> Stats => stats;
    public IEnumerable<TrapModifier> Traps => traps;

    public class OnKeyFragmentUpdatedArgs : EventArgs
    {
        public int OldValue;
        public int NewValue;
        public Player Player;
    }
    
    public class OnStatsUpdatedArgs : EventArgs
    {
        public StatsModifier Modifier;
        public Player Player;
        public InventoryOp Op;
    }
    
    public class OnTrapsUpdatedArgs : EventArgs
    {
        public TrapModifier Trap;
        public InventoryOp Op;
    }
    
    public enum InventoryOp
    {
        Acquired,
        Removed,
        Cleared
    }
    
    public event EventHandler<OnKeyFragmentUpdatedArgs> OnKeyFragmentUpdated;
    public event EventHandler<OnStatsUpdatedArgs> OnStatsUpdate;
    public event EventHandler<OnTrapsUpdatedArgs> OnTrapsUpdated;
    
    
    private void Awake()
    {
        player = gameObject.GetComponent<Player>();
        
        stats.Callback += OnStatsModifiersChanged;
        traps.Callback += OnTrapsChanged;
    }
    
    #region SERVER

    [Server]
    public void ClearTraps()
    {
        traps.Clear();
    }
    
    [Server]
    public void ClearStats()
    {
        stats.Clear();
    }

    [Server]
    public void AddKeyFragment()
    {
        Debug.Log($"Adding key fragment to {player.Username}");
        
        KeyFragments++;
        
        OnKeyFragmentUpdated?.Invoke(this, new OnKeyFragmentUpdatedArgs
        {
            OldValue = KeyFragments - 1,
            NewValue = KeyFragments,
            Player = player
        });
    }
    
    [Server]
    public bool AddStatsModifier(StatsModifier modifier)
    {
        Debug.Log($"Adding modifier {modifier} to {player.Username}");
        
        if (stats.Contains(modifier))
            return false;
        stats.Add(modifier);
        
        if (modifier.other == null || modifier.synergy == null) return true;
        
        var ok = stats.FirstOrDefault(m => m.modifierName == modifier.other.modifierName) != null;
        if (ok)
            AddStatsModifier(modifier.synergy);
        return true;
    }
    
    [Server]
    public bool AddTrap(TrapModifier trap)
    {
        Debug.Log($"Adding trap {trap} to {player.Username}");
        
        if (traps.Count >= trapsCapacity || traps.Contains(trap))
            return false;
        
        traps.Add(trap);
        return true;
    }

    [Server]
    public bool StealKeyFragment()
    {
        if (KeyFragments == 0)
            return false;
        KeyFragments--;
        
        OnKeyFragmentUpdated?.Invoke(this, new OnKeyFragmentUpdatedArgs
        {
            OldValue = KeyFragments + 1,
            NewValue = KeyFragments,
            Player = player
        });
        
        return true;
    }
    
    [Command(requiresAuthority = false)]
    public void UseTrap(TrapModifier trap)
    {
        Debug.Log($"Using trap {trap} from {player.Username}");
        if (!traps.Contains(trap))
            return;
        
        var playerRadius = player.GetComponent<CapsuleCollider>().radius;
        var position = player.transform.position + player.transform.forward * playerRadius * 3;

        if (Physics.Raycast(transform.position, Vector3.down, out var hit))
        {
            position.y = hit.point.y;
            
            var obstacles = Physics.OverlapBoxNonAlloc(position, new Vector3(0.5F, 0.5F, 0.5F), new Collider[5],  Quaternion.identity, obstaclesMask);
            if (obstacles > 0)
                return;
        }
        else
            position.y = 0;

        lock (traps)
        {
            if (!traps.Contains(trap))
                return;
            traps.Remove(trap);
        }

        var obj =  Instantiate(trap.prefab, position, Quaternion.identity);
        NetworkServer.Spawn(obj);
    }
    
    #endregion
    
    #region CLIENT
    
    private void OnKeyFragmentsUpdated(int oldValue, int newValue)
    {
        Debug.Log($"{player.Username} fragments: {oldValue} -> {newValue}");
        OnKeyFragmentUpdated?.Invoke(this, new OnKeyFragmentUpdatedArgs
        {
            OldValue = oldValue,
            NewValue = newValue,
            Player = player
        });
    }
    
    #endregion

    #region CALLBACKS (CLIENT AND SERVER)
    
    private void OnStatsModifiersChanged(SyncList<StatsModifier>.Operation op, int itemIndex, StatsModifier oldItem, StatsModifier newItem)
    {
        InventoryOp inventoryOp;
        switch (op)
        {
            case SyncList<StatsModifier>.Operation.OP_ADD:
                inventoryOp = InventoryOp.Acquired;
                break;
            case SyncList<StatsModifier>.Operation.OP_REMOVEAT:
                inventoryOp = InventoryOp.Removed;
                break;
            case SyncList<StatsModifier>.Operation.OP_CLEAR:
                inventoryOp = InventoryOp.Cleared;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(op), op, null);
        }
        
        if (isClient && player.Username == Player.LocalPlayer.Username)  // Only the inventory owner applies the effect
        {
            switch (inventoryOp)
            {
                case InventoryOp.Acquired:
                    newItem.Enable();
                    break;
                case InventoryOp.Removed:
                    oldItem.Disable();
                    break;
                case InventoryOp.Cleared:
                    foreach (var modifier in stats)
                        modifier.Disable();
                    break;
            }
        }

        OnStatsUpdate?.Invoke(this, new OnStatsUpdatedArgs
        {
            Op = inventoryOp,
            Modifier = inventoryOp == InventoryOp.Acquired ? newItem : oldItem,
            Player = player
        });
    }

    private void OnTrapsChanged(SyncList<TrapModifier>.Operation op, int itemIndex, TrapModifier oldItem, TrapModifier newItem)
    {
        InventoryOp trapOp;
        switch (op)
        {
            case SyncList<TrapModifier>.Operation.OP_ADD:
                trapOp = InventoryOp.Acquired;
                break;
            case SyncList<TrapModifier>.Operation.OP_CLEAR:
                trapOp = InventoryOp.Cleared;
                break;
            case SyncList<TrapModifier>.Operation.OP_REMOVEAT:
                trapOp = InventoryOp.Removed;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(op), op, null);
        }
        
        OnTrapsUpdated?.Invoke(this, new OnTrapsUpdatedArgs
        {
            Op = trapOp,
            Trap = op == SyncList<TrapModifier>.Operation.OP_ADD ? newItem : oldItem
        });
    }

    #endregion
    
    public void IncreaseTrapCapacity(int slots)
    {
        trapsCapacity += slots;
    }
    
    public void DecreaseTrapCapacity(int slots)
    {
        if (trapsCapacity == 0)
            return;
        
        trapsCapacity -= slots;
        if (trapsCapacity < 0)
            trapsCapacity = 0;
        
        if (traps.Count > trapsCapacity)
            traps.TryRemoveElementsInRange(trapsCapacity, traps.Count - trapsCapacity, out _);
    }
    
    public bool IsTrapBagFull() => traps.Count >= trapsCapacity;

    public void UpdateKeyFragments(int roundKeyFragments)
    {
        OnKeyFragmentUpdated?.Invoke(this, new OnKeyFragmentUpdatedArgs
        {
            OldValue = KeyFragments,
            NewValue = KeyFragments,
            Player = player
        });
    }
}
