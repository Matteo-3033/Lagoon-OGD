using System;
using Mirror;
using Modifiers;
using UnityEngine;
using UnityEngine.Serialization;
using Utils;

namespace TrapModifiers
{
    public abstract class TrapModifier: Modifier
    {
        public float durationSeconds;
        public float respawnAfterSeconds;

        [Header("Placement")]
        public Sprite icon;
        public GameObject prefab;

        protected bool Disabled;

        public override void Enable()
        {
            Disabled = false;
            FunctionTimer.Create(Disable, durationSeconds);
        }

        public override void Disable()
        {
            Disabled = true;
        }
    }
    
    public static class TrapModifierSerializer 
    {
        public static void WriteTrapModifier(this NetworkWriter writer, TrapModifier trapModifier)
        {
            if (trapModifier == null)
                writer.Write("");
            else
            {
                writer.WriteString(trapModifier.name);
                writer.WriteString(trapModifier.GetType().ToString());
            }
        }

        public static TrapModifier ReadTrapModifier(this NetworkReader reader)
        {
            var trapName = reader.ReadString();
            
            if (string.IsNullOrEmpty(trapName))
                return null;
            
            var trapType= reader.ReadString();
            var type = Type.GetType(trapType);
            
            return Resources.Load($"Modifiers/Traps/{trapName}", type) as TrapModifier;
        }
    }
}