
using UnityEngine;

namespace Modifiers.Buffs.SuperBuffs
{
    [CreateAssetMenu(menuName = "ScriptableObjects/Buffs/Kill", fileName = "KillSuperBuff")]
    public class KillSuperBuff : StatsModifier
    {
        public override void Enable()
        {
            Player.LocalPlayer.SetCanStealTraps(true);
        }

        public override void Disable()
        {
            Player.LocalPlayer.SetCanStealTraps(false);
        }
    }
}