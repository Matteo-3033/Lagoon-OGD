using UnityEngine;

namespace Modifiers.Buffs
{
    [CreateAssetMenu(menuName = "ScriptableObjects/Buffs/Minimap", fileName = "MinimumBadgeForRippleBuff")]
    public class MinimumBadgeForRippleBuff : StatsModifier
    {
        public int additionalFragments = 1;
        
        public override void Enable()
        {
            Player.LocalPlayer.RippleController.minimumBadgeNumber += additionalFragments;
        }

        public override void Disable()
        {
            Player.LocalPlayer.RippleController.minimumBadgeNumber -= additionalFragments;
        }
    }
}