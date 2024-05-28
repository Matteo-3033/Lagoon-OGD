using UnityEngine;
using Utils;

namespace Modifiers.Buffs.SuperBuffs
{
    [CreateAssetMenu(menuName = "ScriptableObjects/Buffs/ModifierVisibility", fileName = "ModifierVisibilitySuperBuff")]
    public class ModifierVisibilitySuperBuff: StatsModifier
    {
        public override void Enable()
        {
            var stats = GameObject.FindGameObjectsWithTag("StatsModifier");
            var trapDispensers = GameObject.FindGameObjectsWithTag("TrapDispenser");
            
            foreach (var obj in stats)
                Layers.SetLayerRecursively(obj.transform.GetChild(0).gameObject, Layers.Default);
            
            foreach (var obj in trapDispensers)
                Layers.SetLayerRecursively(obj.transform.GetChild(0).gameObject, Layers.Default);
        }

        public override void Disable()
        {
            var stats = GameObject.FindGameObjectsWithTag("StatsModifier");
            var trapDispensers = GameObject.FindGameObjectsWithTag("TrapDispenser");
            
            foreach (var obj in stats)
                Layers.SetLayerRecursively(obj.transform.GetChild(0).gameObject, Layers.BehindFieldOfView);
            
            foreach (var obj in trapDispensers)
                Layers.SetLayerRecursively(obj.transform.GetChild(0).gameObject, Layers.BehindFieldOfView);
        }
    }
}
