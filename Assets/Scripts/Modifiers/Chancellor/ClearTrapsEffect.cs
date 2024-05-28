using System.Linq;
using Round;
using TrapModifiers;
using UnityEngine;
using Utils;

namespace Modifiers.Chancellor
{
    [CreateAssetMenu(menuName = "ScriptableObjects/ChancellorEffects/ClearTraps", fileName = "ClearTraps")]
    public class ClearTrapsEffect: ChancellorModifier
    {
        public override void Enable()
        {
            var players = RoundController.Instance.Players.ToList();
            
            var player1 = players[0];
            var player2 = players[1];

            FunctionTimer.Create(() =>
            {
                player1.Inventory.ClearTraps();
                player2.Inventory.ClearTraps();
            }, 2F);
        }
    }
}