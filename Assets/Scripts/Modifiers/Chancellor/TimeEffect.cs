using Round;
using TrapModifiers;
using UnityEngine;

namespace Modifiers.Chancellor
{
    [CreateAssetMenu(menuName = "ScriptableObjects/ChancellorEffects/Time", fileName = "Time")]
    public class TimeEffect: ChancellorModifier
    {
        [SerializeField] private int bonusTime = 60;
        
        public override void Enable()
        {
            RoundController.Instance.AddTime(bonusTime);
        }
    }
}