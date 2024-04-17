using System;
using System.Linq;
using Mirror;
using Modifiers;
using TrapModifiers;
using UnityEngine;

public class Inventory : NetworkBehaviour
{
    [field: SyncVar(hook = nameof(OnKeyFragmentsUpdated))]
    public int KeyFragments { get; private set; } = 1;

    private readonly SyncList<StatsModifier> modifiers = new();
    private readonly SyncList<TrapModifier> traps = new();
    
    private Player player;

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
        
        modifiers.Callback += OnModifiersChanged;
        traps.Callback += OnTrapsChanged;
    }
    
    #region SERVER

    public void Clear()
    {
        KeyFragments = 1;
        modifiers.Clear();
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
    
    public void AddModifier(StatsModifier modifier)
    {
        if (modifiers.Contains(modifier))
            return;
        modifiers.Add(modifier);
        
        if (modifier.other == null || modifier.synergy == null) return;
        
        var ok = modifiers.FirstOrDefault(m => m.modifierName == modifier.other.modifierName) != null;
        if (ok)
            AddModifier(modifier.synergy);
    }
    
    public void AddTrap(TrapModifier trap)
    {
        traps.Add(trap);
    }
    
    public void RemoveModifier(StatsModifier modifier)
    {
        modifiers.Remove(modifier);
    }

    private bool StealKeyFragment()
    {
        if (KeyFragments == 0)
            return false;
        KeyFragments--;
        return true;
    }
    
    public bool UseTrap(TrapModifier trap)
    {
        if (!traps.Contains(trap))
            return false;
        traps.Remove(trap);

        // TODO: check trap position
        var obj =  Instantiate(trap.prefab, player.transform.position, Quaternion.identity);
        NetworkServer.Spawn(obj);
        
        return true;
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

    private void OnModifiersChanged(SyncList<StatsModifier>.Operation op, int itemIndex, StatsModifier oldItem, StatsModifier newItem)
    {
        var onLocalPlayer = player.Username == Player.LocalPlayer.Username;
        if (onLocalPlayer)
        {
            switch (op)
            {
                case SyncList<StatsModifier>.Operation.OP_ADD:
                    Debug.Log($"Adding modifier {newItem.modifierName} to {player.Username}");
                    Debug.Log(newItem.GetType().ToString());
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
