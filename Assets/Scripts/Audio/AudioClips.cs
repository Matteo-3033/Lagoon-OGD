using UnityEngine;
using UnityEngine.Serialization;

namespace Audio
{
    [CreateAssetMenu(menuName = "ScriptableObjects/AudioClips/Round", fileName = "Round")]
    public class AudioClips: ScriptableObject
    {
        public AudioClip chancellorAlarm;
        public AudioClip[] footsteps;
        public AudioClip[] trapVendingMachine;
        public AudioClip countdown;
        public AudioClip[] error;
        public AudioClip buffActivation;
        public AudioClip superBuffActivation;
        public AudioClip superBuffActivationOnOpponent;
        public AudioClip debuffActivation;
        public AudioClip[] keyFragmentAcquisition;
        public AudioClip[] trapActivation;
        public AudioClip trapPlacement;
        public AudioClip kill;
        public AudioClip stab;
        public AudioClip[] roundEnd;
        public AudioClip[] roundStart;
        public AudioClip killMiniGameBegin;
        public AudioClip doorOpen;
        public AudioClip doorClose;
        public AudioClip[] sentinelSteps;
        public AudioClip sentinelAlarm;
        public AudioClip sentinelEnemyLost;
        public AudioClip sentinelSearchingSound;
    }
}