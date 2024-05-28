using UnityEngine;

namespace Modifiers.Buffs
{
    [CreateAssetMenu(menuName = "ScriptableObjects/Buffs/Rotation", fileName = "RotationBuff")]
    public class RotationBuff : StatsModifier
    {
        public float percentIncrease = 25;
        
        public override void Enable()
        {
            Player.LocalPlayer.RotationController.AddPercentage(percentIncrease);
        }

        public override void Disable()
        {
            Player.LocalPlayer.RotationController.SubPercentage(percentIncrease);
        }
    }
}