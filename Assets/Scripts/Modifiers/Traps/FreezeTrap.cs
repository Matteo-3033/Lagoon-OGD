﻿using TrapModifiers;
using UnityEngine;

namespace Modifiers.Traps
{
    [CreateAssetMenu(menuName = "ScriptableObjects/Traps/Freeze", fileName = "Freeze")]
    public class FreezeTrap: TrapModifier
    {
        public override void Enable()
        {
            Player.LocalPlayer.EnableMovement(false);
            base.Enable();
        }

        public override void Disable()
        {
            if (Disabled) return;
            Player.LocalPlayer.EnableMovement(true);
            base.Disable();
        }
    }
}