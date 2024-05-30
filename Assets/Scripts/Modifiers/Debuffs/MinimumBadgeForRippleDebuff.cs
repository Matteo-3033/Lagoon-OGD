using UnityEngine;
using UnityEngine.Serialization;

namespace Modifiers.Debuffs
{
    [CreateAssetMenu(menuName = "ScriptableObjects/Debuffs/Minimap", fileName = "MinimumBadgeForRippleDebuff")]
    public class MinimumBadgeForRippleDebuff : StatsModifier
    {
        public int subtractiveFragments = 1;
        
        public override void Enable()
        {
            Player.LocalPlayer.RippleController.minimumBadgeNumber -= subtractiveFragments;
        }

        public override void Disable()
        {
            Player.LocalPlayer.RippleController.minimumBadgeNumber += subtractiveFragments;
        }
    }
}