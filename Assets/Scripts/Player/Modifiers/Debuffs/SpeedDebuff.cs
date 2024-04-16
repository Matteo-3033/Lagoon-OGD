using UnityEngine;

namespace Modifiers.Debuffs
{
    [CreateAssetMenu(menuName = "ScriptableObjects/Debuffs/Speed", fileName = "Speed")]
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