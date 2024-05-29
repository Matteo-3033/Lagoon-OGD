using TrapModifiers;
using UnityEngine;

namespace Modifiers.Traps
{
    [CreateAssetMenu(menuName = "ScriptableObjects/Traps/Spinner", fileName = "Spinner")]
    public class SpinnerTrap: TrapModifier
    {
        public override void Enable()
        {
            Player.LocalPlayer.InvertControls(true);
            base.Enable();
        }

        public override void Disable()
        {
            if (Disabled) return;
            Player.LocalPlayer.InvertControls(false);
            base.Disable();
        }
    }
}