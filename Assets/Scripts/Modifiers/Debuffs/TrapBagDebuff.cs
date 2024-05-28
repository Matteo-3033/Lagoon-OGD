using UnityEngine;

namespace Modifiers.Buffs
{
    [CreateAssetMenu(menuName = "ScriptableObjects/Debuffs/TrapBag", fileName = "TrapBagDebuff")]
    public class TrapBagDebuff : StatsModifier
    {
        public int slots = 1;
        
        public override void Enable()
        {
            Player.LocalPlayer.Inventory.DecreaseTrapCapacity(slots);
        }

        public override void Disable()
        {
            Player.LocalPlayer.Inventory.IncreaseTrapCapacity(slots);
        }
    }
}