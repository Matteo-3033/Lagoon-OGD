using TrapModifiers;
using UnityEngine;

namespace Modifiers.Traps
{
    [CreateAssetMenu(menuName = "ScriptableObjects/Traps/Spinner", fileName = "Spinner")]
    public class SpinnerTrap: TrapModifier
    {
        public override void Enable()
        {
            Player.LocalPlayer.GetComponent<IInputHanlder>().Inverted = true;
            base.Enable();
        }

        public override void Disable()
        {
            if (Disabled) return;
            Player.LocalPlayer.GetComponent<IInputHanlder>().Inverted = false;
            base.Disable();
        }
    }
}