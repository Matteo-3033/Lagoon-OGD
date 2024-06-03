using System.Collections;
using Round;
using UnityEngine;

public class BlurEffect : MonoBehaviour
{
    [SerializeField] private Camera blurCamera;
    [SerializeField] private SpriteRenderer blurScreen;

    [Header("Chancellor alarm")]
    [SerializeField] private Color alarmColor;
    [SerializeField] private float transitionDuration;
    
    private Color defaultColor;
    private bool alarmInProgress;
    
    private static readonly int TextureId = Shader.PropertyToID("_Texture");

    private void Awake()
    {
        ChancellorEffectsController.OnEffectEnabled += OnAlarm;
        defaultColor = blurScreen.material.color;
    }

    private void Start()
    {
        if (blurCamera.targetTexture != null)
            blurCamera.targetTexture.Release();
        
        blurCamera.targetTexture = new RenderTexture(Screen.width, Screen.height, 24, RenderTextureFormat.ARGB32, 1);
        blurScreen.material.SetTexture(TextureId, blurCamera.targetTexture);
        
        blurScreen.gameObject.SetActive(true);
        
        KillController.OnPlayerKilled += OnPlayerKilled;
        KillController.OnPlayerRespawned += OnPlayerRespawned;
    }

    private void OnPlayerKilled(Player player)
    {
        if (!player.isLocalPlayer || alarmInProgress)
            return;

        blurScreen.material.color = alarmColor;
    }
    
    private void OnPlayerRespawned(Player player)
    {
        if (!player.isLocalPlayer || alarmInProgress)   
            return;

        blurScreen.material.color = defaultColor;
    }

    private void OnDestroy()
    {
        if (blurCamera != null && blurCamera.targetTexture != null)
            blurCamera.targetTexture.Release();
        
        ChancellorEffectsController.OnEffectEnabled -= OnAlarm;
        KillController.OnPlayerKilled -= OnPlayerKilled;
        KillController.OnPlayerRespawned -= OnPlayerRespawned;
    }
    
    private void OnAlarm(object sender, ChancellorEffectsController.OnEffectEnabledArgs args)
    {
        StartCoroutine(AlarmEffect(args.Duration));
    }

    private IEnumerator AlarmEffect(float duration)
    {
        alarmInProgress = true;
        
        var startColor = defaultColor;
        var endColor = alarmColor;

        var totalElapsedTime = 0F;
        var transitionsDone = 0;
        
        while (totalElapsedTime < duration || transitionsDone % 2 == 1)
        {
            var elapsedTime = 0F;
            while (elapsedTime < transitionDuration)
            {
                blurScreen.material.color = Color.Lerp(startColor, endColor, elapsedTime / transitionDuration);
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            blurScreen.material.color = endColor;
            (startColor, endColor) = (endColor, startColor);
            totalElapsedTime += elapsedTime;
            
            transitionsDone++;
        }
        
        blurScreen.material.color = Player.LocalPlayer.IsDead ? alarmColor : defaultColor;

        alarmInProgress = false;
    }
}
