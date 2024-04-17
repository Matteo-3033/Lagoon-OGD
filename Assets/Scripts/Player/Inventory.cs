using System;
using System.Linq;
using Mirror;
using Modifiers;
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
        public bool OnLocalPlayer;
    }
    
    public class OnStatsUpdatedArgs : EventArgs
    {
        public StatsModifier Modifier;
        public bool OnLocalPlayer;
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
    }
    
    public void AddModifier(StatsModifier modifier)
    {
        Debug.Log("QUI");
        if (modifiers.Contains(modifier))
            return;
        Debug.Log($"Adding modifier {modifier.modifierName} to {player.Username}");
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

    public override void OnStartClient()
    {
        base.OnStartClient();
        modifiers.Callback += OnModifiersChanged;
        traps.Callback += OnTrapsChanged;
    }
    
    private void OnKeyFragmentsUpdated(int oldValue, int newValue)
    {
        OnKeyFragmentUpdated?.Invoke(this, new OnKeyFragmentUpdatedArgs
        {
            OldValue = oldValue,
            NewValue = newValue,
            OnLocalPlayer = player.Username == Player.LocalPlayer.Username
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
            OnLocalPlayer = onLocalPlayer
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
