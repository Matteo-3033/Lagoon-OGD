using UnityEngine;

namespace Modifiers.Buffs.SuperBuffs
{
    [CreateAssetMenu(menuName = "ScriptableObjects/Buffs/PlayerVisibility", fileName = "PlayerVisibilitySuperBuff")]
    public class PlayerVisibilitySuperBuff: StatsModifier
    {
        public override void Enable()
        {
            Player.LocalPlayer.CmdSetTransparent(true);
        }

        public override void Disable()
        {
            Player.LocalPlayer.CmdSetTransparent(false);
        }
    }
}
