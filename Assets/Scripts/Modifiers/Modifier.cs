using UnityEngine;

namespace Modifiers
{
    public abstract class Modifier: ScriptableObject
    {
        public string modifierName;
        
        public abstract void Enable();
        
        public abstract void Disable();
    }
}