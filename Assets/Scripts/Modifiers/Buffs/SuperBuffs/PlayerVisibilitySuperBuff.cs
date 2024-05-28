using Mirror;
using Network;
using UnityEngine;
using Utils;

namespace Modifiers.Buffs.SuperBuffs
{
    [CreateAssetMenu(menuName = "ScriptableObjects/Buffs/PlayerVisibility", fileName = "PlayerVisibilitySuperBuff")]
    public class PlayerVisibilitySuperBuff: StatsModifier
    {
        public override void Enable()
        {
            SetTrasparent();
        }

        public override void Disable()
        {
            SetOpaque();
        }
        
        [Command(requiresAuthority = false)]
        private void SetTrasparent(NetworkConnectionToClient sender = null)
        {
            sender.Player().RpcSetTransparent(true);
        }
        
        [Command(requiresAuthority = false)]
        private void SetOpaque(NetworkConnectionToClient sender = null)
        {
            sender.Player().RpcSetTransparent(false);
        }
    }
}
