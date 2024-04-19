using System;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using Modifiers;
using TrapModifiers;
using UnityEngine;

public class Inventory : NetworkBehaviour
{
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
        public bool Acquired;
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

    public void Clear()
    {
        KeyFragments = 1;
        stats.Clear();
    }

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
    
    public bool AddTrap(TrapModifier trap)
    {
        if (traps.Contains(trap))
            return false;
        
        traps.Add(trap);
        return true;
    }
    
    public void RemoveStatsModifier(StatsModifier modifier)
    {
        stats.Remove(modifier);
    }

    private bool StealKeyFragment()
    {
        if (KeyFragments == 0)
            return false;
        KeyFragments--;
        return true;
    }
    
    [Command(requiresAuthority = false)]
    public void UseTrap(TrapModifier trap)
    {
        if (!traps.Contains(trap))
            return;
        traps.Remove(trap);
        
        Debug.Log($"Placing trap {trap}");

        var playerRadius = player.GetComponent<CapsuleCollider>().radius;
        var position = player.transform.position + player.transform.forward * playerRadius * 3;

        if (Physics.Raycast(transform.position, Vector3.down, out var hit))
            position.y = hit.point.y;
        else
            position.y = 0;
        
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

    private void OnStatsModifiersChanged(SyncList<StatsModifier>.Operation op, int itemIndex, StatsModifier oldItem, StatsModifier newItem)
    {
        var onLocalPlayer = player.Username == Player.LocalPlayer.Username;
        if (onLocalPlayer)
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
        OnTrapsUpdated?.Invoke(this, new OnTrapsUpdatedArgs
        {
            Acquired = op == SyncList<TrapModifier>.Operation.OP_ADD,
            Trap = op == SyncList<TrapModifier>.Operation.OP_ADD ? newItem : oldItem
        });
    }

    #endregion
}
