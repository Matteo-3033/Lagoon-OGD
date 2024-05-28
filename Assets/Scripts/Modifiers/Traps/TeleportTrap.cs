using TrapModifiers;
using UnityEngine;
using Utils;

namespace Modifiers.Traps
{
    [CreateAssetMenu(menuName = "ScriptableObjects/Traps/Teleport", fileName = "Teleport")]
    public class TeleportTrap: TrapModifier
    {
        public override void Enable()
        {
            Player.LocalPlayer.EnableMovement(false);
            
            FunctionTimer.Create(() =>
            {
                Player.LocalPlayer.ReturnToSpawn();
                Player.LocalPlayer.EnableMovement(true);
            }, 0.5F);
            
            base.Enable();
        }
    }
}