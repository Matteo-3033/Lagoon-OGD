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
        public AudioClip[] kill;
        public AudioClip[] roundEnd;
        public AudioClip[] roundStart;
        public AudioClip fightBegin;
        public AudioClip guardDetection;
        public AudioClip cameraDetection;
        public AudioClip doorOpen;
        public AudioClip doorClose;
    }
}