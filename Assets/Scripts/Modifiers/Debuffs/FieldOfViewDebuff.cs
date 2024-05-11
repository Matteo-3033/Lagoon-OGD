using UnityEngine;

namespace Modifiers.Buffs
{
    [CreateAssetMenu(menuName = "ScriptableObjects/Debuffs/FieldOfView", fileName = "FieldOfViewDebuff")]
    public class FieldOfViewDebuff : StatsModifier
    {
        public float angleDecrease = 30;
        public float distanceDecrease = 1.5F;
        
        public override void Enable()
        {
            var fov = Player.LocalPlayer.FieldOfView;
            fov.SetAngle(fov.GetAngle() - angleDecrease);
            fov.SetViewDistance(fov.GetViewDistance() - distanceDecrease);
        }

        public override void Disable()
        {
            var fov = Player.LocalPlayer.FieldOfView;
            fov.SetAngle(fov.GetAngle() + angleDecrease);
            fov.SetViewDistance(fov.GetViewDistance() + distanceDecrease);
        }
    }
}