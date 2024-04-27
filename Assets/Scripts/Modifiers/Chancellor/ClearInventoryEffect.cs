using System.Linq;
using Round;
using TrapModifiers;
using UnityEngine;
using Utils;

namespace Modifiers.Chancellor
{
    [CreateAssetMenu(menuName = "ScriptableObjects/ChancellorEffects/ClearInventory", fileName = "ClearInventory")]
    public class ClearInventoryEffect: ChancellorModifier
    {
        public override void Enable()
        {
            var players = RoundController.Instance.Players.ToList();
            
            var player1 = players[0];
            var player2 = players[1];

            FunctionTimer.Create(() =>
            {
                player1.Inventory.Clear();
                player2.Inventory.Clear();
            }, 2F);
        }
    }
}