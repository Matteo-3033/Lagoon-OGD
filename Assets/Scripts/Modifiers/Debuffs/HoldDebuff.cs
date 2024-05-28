using UnityEngine;
using Utils.UI;

namespace Modifiers.Buffs
{
    [CreateAssetMenu(menuName = "ScriptableObjects/Debuffs/Hold", fileName = "HoldDebuff")]
    public class HoldDebuff : StatsModifier
    {
        public float reductionFactor = 3;
        
        public override void Enable()
        {
            LongPressButton.HoldFactorModifier += reductionFactor;
        }

        public override void Disable()
        {
            LongPressButton.HoldFactorModifier -= reductionFactor;
        }
    }
}