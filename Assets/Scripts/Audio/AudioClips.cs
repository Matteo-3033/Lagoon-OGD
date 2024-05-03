using UnityEngine;
using UnityEngine.Serialization;

namespace Audio
{
    [CreateAssetMenu(menuName = "ScriptableObjects/AudioClips", fileName = "AudioClips")]
    public class AudioClips: ScriptableObject
    {
        public AudioClip chancellorAlarm;
        public AudioClip[] footsteps;
        public AudioClip[] trapVendingMachine;
        public AudioClip countdown;
        public AudioClip error;
        public AudioClip buffActivation;
        public AudioClip debuffActivation;
        public AudioClip keyFragmentAcquisition;
        public AudioClip trapActivation;
        public AudioClip[] kill;
        public AudioClip roundEnd;
        public AudioClip roundStart;
    }
}