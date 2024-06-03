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

    private void OnSentinelStep()
    {
        PlayClipAtPoint(audioClips.sentinelSteps, transform.position, 0.5F, true);
    }

    public void OnSentinelAlarm()
    {
        PlayClipAtPoint(audioClips.sentinelAlarm, transform.position, 0.5F, true);
    }

    public void OnSentinelEnemyLost()
    {
        PlayClipAtPoint(audioClips.sentinelEnemyLost, transform.position, 0.5F, true);
    }

    public void OnSentinelSearching()
    {
        PlayClipAtPoint(audioClips.sentinelSearchingSound, transform.position, 0.5F, true);
    }
}