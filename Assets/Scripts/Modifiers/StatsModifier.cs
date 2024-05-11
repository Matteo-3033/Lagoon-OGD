using System;
using Mirror;
using UnityEngine;

namespace Modifiers
{
    public abstract  class StatsModifier: Modifier
    {
        public bool isBuff = true;
        public StatsModifier other;
        public StatsModifier synergy;
        public bool canBeFoundInGame = true;
    }
    
    public static class StatsModifierSerializer 
    {
        public static void WriteStatsModifier(this NetworkWriter writer, StatsModifier statsModifier)
        {
            if (statsModifier == null)
                writer.Write("");
            else
            {
                writer.WriteString(statsModifier.name);
                writer.WriteString(statsModifier.GetType().ToString());
            }
        }

        public static StatsModifier ReadStatsModifier(this NetworkReader reader)
        {
            var statsName = reader.ReadString();
            
            if (string.IsNullOrEmpty(statsName))
                return null;
            
            var statsType= reader.ReadString();
            var type = Type.GetType(statsType);
            
            return Resources.Load($"Modifiers/Stats/{statsName}", type) as StatsModifier;
        }
    }
}