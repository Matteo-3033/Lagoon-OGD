﻿using System.Linq;
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
            Debug.Log("ShowPlayerPosition Enable()");
            var players = RoundController.Instance.Players.ToList();

            var player1 = players[0];
            var player2 = players[1];
            
            FunctionTimer.Create(() =>
            {
                Debug.Log("ShowPlayerPosition Show");
                player1.GetComponent<MinimapIcon>().ClientRpcShow();
                player2.GetComponent<MinimapIcon>().ClientRpcShow();
            }, 2F);
            
            FunctionTimer.Create(() =>
            {
                Debug.Log("ShowPlayerPosition Hide");
                player1.GetComponent<MinimapIcon>().ClientRpcHide();
                player2.GetComponent<MinimapIcon>().ClientRpcHide();
            }, 32F);
        }
    }
}