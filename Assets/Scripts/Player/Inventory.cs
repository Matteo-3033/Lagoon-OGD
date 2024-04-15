using System;
using System.Linq;
using Mirror;
using Modifiers;

public class Inventory : NetworkBehaviour
{
    [field: SyncVar(hook = nameof(OnKeyFragmentsUpdated))]
    public int KeyFragments { get; private set; } = 1;

    private readonly SyncList<Modifier> modifiers = new();
    
    private Player player;

    public class OnKeyFragmentUpdatedArgs : EventArgs
    {
        public int OldValue;
        public int NewValue;
        public bool OnLocalPlayer;
    }
    
    public class OnModifierUpdatedArgs : EventArgs
    {
        public Modifier Modifier;
        public bool OnLocalPlayer;
        public bool Enabled;
    }
    
    public event EventHandler<OnKeyFragmentUpdatedArgs> OnKeyFragmentUpdated;
    public event EventHandler<OnModifierUpdatedArgs> OnModifierUpdate;
    
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
    
    public void AddModifier(Modifier modifier)
    {            
        if (modifiers.Contains(modifier))
            return;
        
        modifiers.Add(modifier);
        
        if (modifier.other == null || modifier.synergy == null) return;
        
        var ok = modifiers.FirstOrDefault(m => m.modifierName == modifier.other.modifierName) != null;
        if (ok) AddModifier(modifier.synergy);
    }
    
    public void RemoveModifier(Modifier modifier)
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
    
    #endregion

    #region CLIENT

    public override void OnStartClient()
    {
        base.OnStartClient();
        modifiers.Callback += OnModifiersChanged;
    }

    private void OnModifiersChanged(SyncList<Modifier>.Operation op, int itemindex, Modifier olditem, Modifier newitem)
    {
        var onLocalPlayer = player.Username == Player.LocalPlayer.Username;
        if (onLocalPlayer)
        {
            switch (op)
            {
                case SyncList<Modifier>.Operation.OP_ADD:
                    newitem.Enable();
                    break;
                case SyncList<Modifier>.Operation.OP_REMOVEAT:
                    olditem.Disable();
                    break;
            }
        }

        OnModifierUpdate?.Invoke(this, new OnModifierUpdatedArgs
        {
            Enabled = op == SyncList<Modifier>.Operation.OP_ADD,
            Modifier = op == SyncList<Modifier>.Operation.OP_ADD ? newitem : olditem,
            OnLocalPlayer = onLocalPlayer
        });
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

    #endregion
}
