using UnityEngine;

namespace Modifiers.Buffs
{
    [CreateAssetMenu(menuName = "ScriptableObjects/Debuffs", fileName = "Speed")]
    public class SpeedDebuff : Modifier
    {
        public float divisionFactor = 2;
        
        public override void Enable()
        {
            Player.LocalPlayer.PositionController.AddFactor(1 / divisionFactor);
        }

        public override void Disable()
        {
            Player.LocalPlayer.PositionController.AddFactor(divisionFactor);
        }
    }
}