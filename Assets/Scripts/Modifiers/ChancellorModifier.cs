using System;
using Mirror;
using Modifiers;
using UnityEngine;
using UnityEngine.Serialization;
using Utils;

namespace TrapModifiers
{
    public abstract class ChancellorModifier: Modifier
    {
        public string description;

        public override void Disable()
        {
        }
    }
    
    public static class ChancellorModifierSerializer 
    {
        public static void WriteChancellorModifier(this NetworkWriter writer, ChancellorModifier chancellorModifier)
        {
            if (chancellorModifier == null)
                writer.Write("");
            else
            {
                writer.WriteString(chancellorModifier.name);
                writer.WriteString(chancellorModifier.GetType().ToString());
            }
        }

        public static ChancellorModifier ReadChancellorModifier(this NetworkReader reader)
        {
            var modifierName = reader.ReadString();
            
            if (string.IsNullOrEmpty(modifierName))
                return null;
            
            var modifierType= reader.ReadString();
            var type = Type.GetType(modifierType);
            
            return Resources.Load($"Modifiers/Chancellor/{modifierName}", type) as ChancellorModifier;
        }
    }
}