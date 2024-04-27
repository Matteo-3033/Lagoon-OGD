using System;
using System.Collections.Generic;
using System.Linq;
using TrapModifiers;
using UnityEngine;

public class TrapSelector: MonoBehaviour
{
    private int selectedIndex = -1;
    private List<TrapModifier> GetTraps() => Player.LocalPlayer.Inventory.Traps.ToList();
    
    [SerializeField] private AnimationClip selectTrapAnimation;
    private float timeSinceAnimationStart;
    
    public class OnSelectedTrapIndexChangedArgs : EventArgs
    {
        public int Index;
        public bool IndexIncreased;
    }
    
    public event EventHandler<OnSelectedTrapIndexChangedArgs> OnSelectedTrapIndexChanged;
    
    private void Awake()
    {
        var inputHandler = gameObject.GetComponent<IInputHanlder>();
        inputHandler.OnPlaceTrap += PlaceTrap;
        inputHandler.OnSelectTrap += OnChangeSelection;
        selectedIndex = -1;
    }

    private void OnChangeSelection(object sender, int direction)
    {
        if (AnimationInProgress() || direction == 0)
            return;
        
        timeSinceAnimationStart = Time.time;
        var traps = GetTraps();
        
        var newIndex = selectedIndex;
        if (direction > 0)
            newIndex++;
        else if (newIndex < 0)
            newIndex = traps.Count - 1;
        else
            newIndex--;

        if (newIndex < 0 || newIndex >= traps.Count)
            newIndex = -1;
        
        if (newIndex == selectedIndex)
            return;
        
        selectedIndex = newIndex;
        
        OnSelectedTrapIndexChanged?.Invoke(this, new OnSelectedTrapIndexChangedArgs
        {
            Index = selectedIndex,
            IndexIncreased = direction > 0
        });
    }

    private void PlaceTrap(object sender, EventArgs args)
    {
        if (AnimationInProgress())
            return;
        
        var traps = GetTraps();
        
        if (selectedIndex < 0 || selectedIndex >= traps.Count)
            return;

        Player.LocalPlayer.Inventory.UseTrap(traps[selectedIndex]);
    }
    
    private bool AnimationInProgress()
    {
        return Time.time - timeSinceAnimationStart < selectTrapAnimation.length;
    }
}
