using System;
using System.Linq;
using Mirror;
using TrapModifiers;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Round
{
    public class ChancellorEffectsController : NetworkBehaviour
    {
        [SerializeField] private ChancellorModifier[] effects;
        [SerializeField] private float alarmDuration;
        [SerializeField] private float activateEverySeconds = 60F;
        
        private int minutesPassed;
        
        private const float BASE_PROBABILITY = 1;
        private const float PROBABILITY_INCREASE = 0.2F;
        
        
        public class OnEffectEnabledArgs : EventArgs
        {
            public ChancellorModifier Effect;
            public float Duration;
        }
        
        public static event EventHandler<OnEffectEnabledArgs> OnEffectEnabled; 

        #region SERVER
        
        public override void OnStartServer()
        {
            base.OnStartServer();
            
            if (RoundController.Loaded)
                RegisterRoundControllerCallbacks();
            else
                RoundController.OnRoundLoaded += RegisterRoundControllerCallbacks;
        }

        [Server]
        private void RegisterRoundControllerCallbacks()
        {
            RoundController.Instance.TimerUpdate += OnTimerUpdate;
        }

        [Server]
        private void OnTimerUpdate(int timeLeftSecs)
        {
            Debug.Log($"Time left: {timeLeftSecs}");
            if (timeLeftSecs <= 0)
                return;
            
            var totalSecs = (int) (RoundController.Instance.Round.timeLimitMinutes * 60);
            var timePassed = totalSecs - timeLeftSecs;
            
            Debug.Log($"Time passed: {timePassed}");
            if (timePassed <= 0 || timePassed % activateEverySeconds != 0)
                return;
            
            minutesPassed++;
            
            if (ShouldApplyEffect())
                ApplyRandomEffect();
        }

        [Server]
        private bool ShouldApplyEffect()
        {
            var prob = Mathf.Max(BASE_PROBABILITY + (minutesPassed - 1) * PROBABILITY_INCREASE, 1F);
            return effects.Length > 0 && Random.Range(0, 1) < prob;
        }

        [Server]
        private void ApplyRandomEffect()
        {
            var effect = effects[Random.Range(0, effects.Length)];
            
            effects = effects.Where(val => val != effect).ToArray();
            Debug.Log($"Applying effect {effect.name}");
            
            effect.Enable();
            RpcNotifyEffectEnabled(effect);
        }
        
        #endregion
        
        #region CLIENT
        
        [ClientRpc]
        private void RpcNotifyEffectEnabled(ChancellorModifier effect)
        {
            Debug.Log($"Effect {effect.name} enabled");
            OnEffectEnabled?.Invoke(this, new OnEffectEnabledArgs
            {
                Effect = effect,
                Duration = alarmDuration
            });
        }

        [ContextMenu("Test effects")]
        private void TestEffect()
        {
            OnEffectEnabled?.Invoke(this, new OnEffectEnabledArgs
            {
                Effect = effects[0],
                Duration = alarmDuration
            });
        }
        
        
        #endregion
        
        private void OnDestroy()
        {
            RoundController.OnRoundLoaded -= RegisterRoundControllerCallbacks;
        }
    }
}
