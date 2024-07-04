using TrapModifiers;
using UnityEngine;
using Utils;

namespace Modifiers.Chancellor
{
    [CreateAssetMenu(menuName = "ScriptableObjects/ChancellorEffects/HideMinimap", fileName = "HideMinimap")]
    public class HideMinimapEffect : ChancellorModifier
    {
        public override void Enable()
        {
            var camera = GameObject.FindWithTag("MinimapCamera").GetComponent<MinimapCamera>();

            FunctionTimer.Create(() => camera.ShowAllMinimapDarkAreas(), duration);
        }
    }
}