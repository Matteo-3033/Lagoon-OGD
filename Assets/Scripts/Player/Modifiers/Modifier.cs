using System;
using Mirror;
using UnityEngine;

namespace Modifiers
{
    public abstract class Modifier: ScriptableObject
    {
        public string modifierName;
        
        public abstract void Enable();
        
        public abstract void Disable();
    }
    
    public static class ModifierSerializer 
    {
        public static void WriteModifier(this NetworkWriter writer, Modifier modifier)
        {
            if (modifier == null)
                writer.Write("");
            else
            {
                writer.WriteString(modifier.name);
                writer.WriteString(modifier.GetType().ToString());
            }
        }

        public static Modifier ReadModifier(this NetworkReader reader)
        {
            var modifierName = reader.ReadString();
            
            if (string.IsNullOrEmpty(modifierName))
                return null;
            
            var modifierType= reader.ReadString();

            var type = Type.GetType(modifierType);
            
            string dir;
            if (type!.IsSubclassOf(typeof(TrapModifier)))
                dir = "Traps";
            else if (type.IsSubclassOf(typeof(StatsModifier)))
                dir = "Stats";
            else
                throw new Exception($"Invalid modifier type {type}");
            
            return Resources.Load($"Modifiers/{dir}/{modifierName}", type) as Modifier;
        }
    }
}