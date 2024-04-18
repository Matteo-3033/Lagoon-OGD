using System;
using System.Collections.Generic;
using System.Linq;
using TrapModifiers;
using UnityEngine;

public class TrapSelector: MonoBehaviour
{
    private int selectedIndex;
    private List<TrapModifier> GetTraps() => Player.LocalPlayer.Inventory.Traps.ToList();

    public class OnTrapSelectedArgs : EventArgs
    {
        public int Index;
        public TrapModifier Trap;
    }
    
    public event EventHandler<OnTrapSelectedArgs> OnTrapSelected;
    
    private void Awake()
    {
        var inputHandler = gameObject.GetComponent<IInputHanlder>();
        inputHandler.OnPlaceTrap += PlaceTrap;
        inputHandler.OnSelectTrap += OnChangeSelection;
    }

    private void OnChangeSelection(object sender, int dir)
    {
        if (dir == 0)
            return;
        
        var traps = GetTraps();
        
        if (dir > 0)
            selectedIndex++;
        else if (selectedIndex < 0)
            selectedIndex = traps.Count - 1;
        else
            selectedIndex--;

        if (selectedIndex < 0 || selectedIndex >= traps.Count)
            selectedIndex = -1;
        
        OnTrapSelected?.Invoke(this, new OnTrapSelectedArgs
        {
            Index = selectedIndex,
            Trap = selectedIndex > 0 ? traps[selectedIndex] : null
        });
    }

    private void PlaceTrap(object sender, EventArgs args)
    {
        var traps = GetTraps();
        
        if (selectedIndex < 0 || selectedIndex >= traps.Count)
            return;

        Player.LocalPlayer.Inventory.UseTrap(traps[selectedIndex]);
    }
}
