using System;
using Mirror;
using UnityEngine;

namespace Modifiers
{
    public abstract class Modifier: ScriptableObject
    {
        public string modifierName;
        public bool isBuff;
        public Modifier other;
        public Modifier synergy;
        
        public abstract void Enable();
        
        public abstract void Disable();
    }
    
    public static class ModifierSerializer 
    {
        public static void WriteRoundConfiguration(this NetworkWriter writer, Modifier modifier)
        {
            if (modifier == null)
                writer.Write("");
            else
            {
                writer.WriteString(modifier.name);
                writer.WriteString(modifier.GetType().ToString());
                writer.WriteBool(modifier.isBuff);
            }
        }

        public static Modifier ReadRoundConfiguration(this NetworkReader reader)
        {
            var modifierName = reader.ReadString();
            
            if (string.IsNullOrEmpty(modifierName))
                return null;
            
            var modifierType= reader.ReadString();
            var isBuff = reader.ReadBool();

            var type = Type.GetType(modifierType);
            var subdir = isBuff ? "Buffs" : "Debuffs";
            
            return Resources.Load($"Modifiers/{subdir}/{modifierName}", type) as Modifier;
        }
    }
}