using System;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using Modifiers;
using TrapModifiers;
using UnityEngine;

public class Inventory : NetworkBehaviour
{
    [SerializeField] private LayerMask obstaclesMask;
    
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
        public bool Enabled;
    }
    
    public class OnTrapsUpdatedArgs : EventArgs
    {
        public TrapModifier Trap;
        public TrapOP Op;
    }
    
    public enum TrapOP
    {
        Acquired,
        Placed,
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
    public void Clear()
    {
        var statsCopy = stats.ToArray();
        foreach (var modifier in statsCopy)
            RemoveStatsModifier(modifier);
        
        traps.Clear();
    }

    [Server]
    public void AddKeyFragment()
    {
        KeyFragments++;
        
        OnKeyFragmentUpdated?.Invoke(this, new OnKeyFragmentUpdatedArgs
        {
            OldValue = KeyFragments - 1,
            NewValue = KeyFragments,
            Player = player
        });
    }
    
    [Server]
    public void AddStatsModifier(StatsModifier modifier)
    {
        if (stats.Contains(modifier))
            return;
        stats.Add(modifier);
        
        if (modifier.other == null || modifier.synergy == null) return;
        
        var ok = stats.FirstOrDefault(m => m.modifierName == modifier.other.modifierName) != null;
        if (ok)
            AddStatsModifier(modifier.synergy);
    }
    
    [Server]
    public bool AddTrap(TrapModifier trap)
    {
        if (traps.Contains(trap))
            return false;
        
        traps.Add(trap);
        return true;
    }
    
    [Server]
    public void RemoveStatsModifier(StatsModifier modifier)
    {
        stats.Remove(modifier);
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
        if (!traps.Contains(trap))
            return;
        
        Debug.Log($"Placing trap {trap}");

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
        
        traps.Remove(trap);
        var obj =  Instantiate(trap.prefab, position, Quaternion.identity);
        NetworkServer.Spawn(obj);
    }
    
    #endregion
    
    #region CLIENT
    
    private void OnKeyFragmentsUpdated(int oldValue, int newValue)
    {
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
        if (isClient && player.Username == Player.LocalPlayer.Username)  // Only the inventory owner applies the effect (and then notifies the server)
        {
            switch (op)
            {
                case SyncList<StatsModifier>.Operation.OP_ADD:
                    newItem.Enable();
                    break;
                case SyncList<StatsModifier>.Operation.OP_REMOVEAT:
                    oldItem.Disable();
                    break;
            }
        }

        OnStatsUpdate?.Invoke(this, new OnStatsUpdatedArgs
        {
            Enabled = op == SyncList<StatsModifier>.Operation.OP_ADD,
            Modifier = op == SyncList<StatsModifier>.Operation.OP_ADD ? newItem : oldItem,
            Player = player
        });
    }
    
    private void OnTrapsChanged(SyncList<TrapModifier>.Operation op, int itemIndex, TrapModifier oldItem, TrapModifier newItem)
    {
        TrapOP trapOp;
        switch (op)
        {
            case SyncList<TrapModifier>.Operation.OP_ADD:
                trapOp = TrapOP.Acquired;
                break;
            case SyncList<TrapModifier>.Operation.OP_CLEAR:
                trapOp = TrapOP.Cleared;
                break;
            case SyncList<TrapModifier>.Operation.OP_REMOVEAT:
                trapOp = TrapOP.Placed;
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
}
