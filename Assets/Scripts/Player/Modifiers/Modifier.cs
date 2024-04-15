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
        public static void WriteRoundConfiguration(this NetworkWriter writer, Modifier config)
        {
            writer.WriteString(config == null ? "" : config.name);
            writer.WriteString(config == null ? "" : config.GetType().ToString());
            writer.WriteBool(config != null && config.isBuff);
        }

        public static Modifier ReadRoundConfiguration(this NetworkReader reader)
        {
            var modifierName = reader.ReadString();
            var modifierType= reader.ReadString();
            var isBuff = reader.ReadBool();

            var type = Type.GetType(modifierType);
            var subdir = isBuff ? "Buffs" : "Debuffs";
            
            return string.IsNullOrEmpty(modifierName)
                ? null
                : Resources.Load($"Modifiers/{subdir}/{modifierName}", type) as Modifier;
        }
    }
}