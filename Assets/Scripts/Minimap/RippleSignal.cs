using UnityEngine;

public class RippleSignal : MonoBehaviour
{
    protected ParticleSystem ParticleSystem;
    public float RippleLifetime { get; private set; }
    public float Interval { get; private set; }

    void Awake()
    {
        ParticleSystem = GetComponent<ParticleSystem>();
    }

    public void ConfigureRipple(float rippleLifetime, float interval, float scale, Color color)
    {
        StopRipple();
        transform.localScale = Vector3.one * scale;

        RippleLifetime = rippleLifetime;
        Interval = interval;

        ParticleSystem.MainModule rippleEffectMain = ParticleSystem.main;
        rippleEffectMain.duration = interval;
        rippleEffectMain.loop = false;
        rippleEffectMain.startLifetime = rippleLifetime;
        rippleEffectMain.startColor = color;

        ParticleSystem.emission.SetBursts(new[] { new ParticleSystem.Burst(0, 1, 0, interval) });
    }

    public void ConfigureRipple(RippleConfiguration rippleConfiguration)
    {
        ConfigureRipple(rippleConfiguration.singleRippleDuration,
            rippleConfiguration.interval,
            rippleConfiguration.scale,
            rippleConfiguration.rippleColor);
    }

    public virtual void PlayRipple()
    {
        ParticleSystem.Play();
    }

    public virtual void StopRipple()
    {
        ParticleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }
}