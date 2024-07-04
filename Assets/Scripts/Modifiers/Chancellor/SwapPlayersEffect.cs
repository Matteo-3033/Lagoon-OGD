using System.Linq;
using Round;
using TrapModifiers;
using UnityEngine;
using Utils;

namespace Modifiers.Chancellor
{
    [CreateAssetMenu(menuName = "ScriptableObjects/ChancellorEffects/SwapPlayers", fileName = "SwapPlayers")]
    public class SwapPlayersEffect: ChancellorModifier
    {
        private int playersReady;
        
        public override void Enable()
        {
            var players = RoundController.Instance.Players.ToList();
            playersReady = 0;
            
            var player1 = players[0];
            var player2 = players[1];
            
            player1.TargetEnableMovement(false);
            player2.TargetEnableMovement(false);

            FunctionTimer.Create(() =>
            {
                player1.OnPositionChanged += OnPlayerReady;
                player2.OnPositionChanged += OnPlayerReady;
            
                player1.TargetGoTo(player2.transform.position);
                player2.TargetGoTo(player1.transform.position);
            }, duration);
        }

        private void OnPlayerReady(object sender, Vector3 args)
        {
            if (sender is not Player player)
                return;
            
            player.OnPositionChanged -= OnPlayerReady;
            
            playersReady++;
            if (playersReady == 2)
                FunctionTimer.Create(EnableMovement, 1F);
        }

        private void EnableMovement()
        {
            var players = RoundController.Instance.Players.ToList();
            players[0].TargetEnableMovement(true);
            players[1].TargetEnableMovement(true);
        }
    }
}