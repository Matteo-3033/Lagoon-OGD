using TrapModifiers;
using UnityEngine;

namespace Modifiers.Traps
{
    [CreateAssetMenu(menuName = "ScriptableObjects/Traps/Inkwell", fileName = "Inkwell")]
    public class InkwellTrap: TrapModifier
    {
        public override void Enable()
        {
            Player.LocalPlayer.MakeInvisible();
            base.Enable();
        }

        public override void Disable()
        {
            if (Disabled) return;
            Player.LocalPlayer.MakeVisible();
            base.Disable();
        }
    }
}