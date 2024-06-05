using System;
using System.Linq;
using TrapModifiers;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Modifiers.Traps
{
    [CreateAssetMenu(menuName = "ScriptableObjects/Traps/Squid", fileName = "Squid")]
    public class SquidTrap: TrapModifier
    {
        public override void Enable()
        {
            var minimapObjs = Object.FindObjectsOfType<MinimapDarkArea>(true).ToList();
            
            minimapObjs = minimapObjs.Where(obj => obj.IsVisible).OrderBy(x => Guid.NewGuid()).ToList();
            
            for (var i = 0; i < minimapObjs.Count / 2; i++)
                minimapObjs[i].Show();
            
            base.Enable();
        }
    }
}