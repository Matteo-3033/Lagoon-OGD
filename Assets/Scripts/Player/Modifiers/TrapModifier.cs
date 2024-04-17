using UnityEngine;
using Utils;

namespace Modifiers
{
    // Do not instantiate
    // Not abstract only for serialization
    public class TrapModifier: Modifier
    {
        public float duration;
        public string description;
        
        [Header("Placement")]
        public Sprite icon;
        public GameObject prefab;

        protected bool Disabled;

        public override void Enable()
        {
            FunctionTimer.Create(Disable, duration);
        }

        public override void Disable()
        {
            Disabled = true;
        }
    }
}