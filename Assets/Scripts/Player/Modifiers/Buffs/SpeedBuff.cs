using UnityEngine;

namespace Modifiers.Buffs
{
    [CreateAssetMenu(menuName = "ScriptableObjects/Buffs", fileName = "Speed")]
    public class SpeedBuff : Modifier
    {
        public float multiplicationFactor = 2;
        
        public override void Enable()
        {
            Player.LocalPlayer.PositionController.AddFactor(multiplicationFactor);
        }

        public override void Disable()
        {
            Player.LocalPlayer.PositionController.AddFactor(1 / multiplicationFactor);
        }
    }
}