using UnityEngine;

namespace Modifiers.Buffs
{
    [CreateAssetMenu(menuName = "ScriptableObjects/Buffs/TrapBag", fileName = "TrapBagBuff")]
    public class TrapBagBuff : StatsModifier
    {
        public int slots = 1;
        
        public override void Enable()
        {
            Player.LocalPlayer.Inventory.IncreaseTrapCapacity(slots);
        }

        public override void Disable()
        {
            Player.LocalPlayer.Inventory.DecreaseTrapCapacity(slots);
        }
    }
}