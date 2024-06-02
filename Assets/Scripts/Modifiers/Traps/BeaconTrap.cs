using Mirror;
using TrapModifiers;
using UnityEngine;

namespace Modifiers.Traps
{
    [CreateAssetMenu(menuName = "ScriptableObjects/Traps/Beacon", fileName = "Beacon")]
    public class BeaconTrap : TrapModifier
    {
        [Client]
        public override void Enable()
        {
            MinimapIcon minimapIcon = Player.Opponent.MinimapIcon;
            minimapIcon.ClampToMinimapBorder(true);
            minimapIcon.Show(true);
            
            base.Enable();
        }

        [Client]
        public override void Disable()
        {
            if (Disabled) return;
            
            MinimapIcon minimapIcon = Player.Opponent.MinimapIcon;
            minimapIcon.ClampToMinimapBorder(false);
            minimapIcon.Hide(false);
            
            base.Disable();
        }
    }
}