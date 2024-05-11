using UnityEngine;

namespace Modifiers.Buffs
{
    [CreateAssetMenu(menuName = "ScriptableObjects/Buffs/FieldOfView", fileName = "FieldOfViewBuff")]
    public class FieldOfViewBuff : StatsModifier
    {
        public float angleIncrease = 30;
        public float distanceIncrease = 1.5F;
        
        public override void Enable()
        {
            var fov = Player.LocalPlayer.FieldOfView;
            fov.SetAngle(fov.GetAngle() + angleIncrease);
            fov.SetViewDistance(fov.GetViewDistance() + distanceIncrease);
        }

        public override void Disable()
        {
            var fov = Player.LocalPlayer.FieldOfView;
            fov.SetAngle(fov.GetAngle() - angleIncrease);
            fov.SetViewDistance(fov.GetViewDistance() - distanceIncrease);
        }
    }
}