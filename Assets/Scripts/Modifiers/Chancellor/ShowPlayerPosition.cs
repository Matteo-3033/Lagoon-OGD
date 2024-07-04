using System.Linq;
using Round;
using TrapModifiers;
using UnityEngine;
using Utils;

namespace Modifiers.Chancellor
{
    [CreateAssetMenu(menuName = "ScriptableObjects/ChancellorEffects/ShowPlayerPosition",
        fileName = "ShowPlayerPosition")]
    public class ShowPlayerPosition : ChancellorModifier
    {
        public override void Enable()
        {
            var players = RoundController.Instance.Players.ToList();

            var player1 = players[0];
            var player2 = players[1];

            FunctionTimer.Create(() =>
            {
                player1.MinimapIcon.RpcShow(true);
                player2.MinimapIcon.RpcShow(true);
            }, 2F);

            FunctionTimer.Create(() =>
            {
                player1.MinimapIcon.RpcHide(false);
                player2.MinimapIcon.RpcHide(false);
            }, duration + 2F);
        }
    }
}