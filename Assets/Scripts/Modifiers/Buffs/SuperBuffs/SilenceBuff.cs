using UnityEngine;

namespace Modifiers.Buffs.SuperBuffs
{
    [CreateAssetMenu(menuName = "ScriptableObjects/Buffs/Silence", fileName = "SilenceSuperBuff")]
    public class SilenceBuff : StatsModifier
    {
        public override void Enable()
        {
            Player.LocalPlayer.SetSilent(true);
        }

        public override void Disable()
        {
            Player.LocalPlayer.SetSilent(false);
        }
    }
}