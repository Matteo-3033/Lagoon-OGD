using UnityEngine;

namespace Modifiers.Buffs
{
    [CreateAssetMenu(menuName = "ScriptableObjects/Buffs/Rotation", fileName = "RotationDebuff")]
    public class RotationDebuff : StatsModifier
    {
        public float percentDecrease = 25;
        
        public override void Enable()
        {
            Player.LocalPlayer.RotationController.SubPercentage(percentDecrease);
        }

        public override void Disable()
        {
            Player.LocalPlayer.RotationController.AddPercentage(percentDecrease);
        }
    }
}