using System.Collections;
using System.Collections.Generic;
using Audio;
using UnityEngine;

public class SentinelSoundManager : SoundManager
{
    [SerializeField] private AudioClips audioClips;

    public void PlaySentinelStepSound()
    {
        OnSentinelStep();
    }

    public void OnSentinelStep()
    {
        PlayClipAtPoint(audioClips.sentinelSteps, transform.position);
    }

    public void OnSentinelAlarm()
    {
        PlayClipAtPoint(audioClips.sentinelAlarm, transform.position);
    }

    public void OnSentinelEnemyLost()
    {
        PlayClipAtPoint(audioClips.sentinelEnemyLost, transform.position);
    }

    public void OnSentinelSearching()
    {
        PlayClipAtPoint(audioClips.sentinelSearchingSound, transform.position);
    }
}