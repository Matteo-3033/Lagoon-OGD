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
    
    private static readonly int TextureId = Shader.PropertyToID("_Texture");

    private void Awake()
    {
        ChancellorEffectsController.OnEffectEnabled += OnAlarm;
    }

    private void Start()
    {
        if (blurCamera.targetTexture != null)
            blurCamera.targetTexture.Release();
        
        blurCamera.targetTexture = new RenderTexture(Screen.width, Screen.height, 24, RenderTextureFormat.ARGB32, 1);
        blurScreen.material.SetTexture(TextureId, blurCamera.targetTexture);
        
        blurScreen.gameObject.SetActive(true);
    }

    private void OnDestroy()
    {
        blurCamera.targetTexture.Release();
        ChancellorEffectsController.OnEffectEnabled -= OnAlarm;
    }
    
    private void OnAlarm(object sender, ChancellorEffectsController.OnEffectEnabledArgs args)
    {
        StartCoroutine(AlarmEffect(args.Duration));
    }

    private IEnumerator AlarmEffect(float duration)
    {
        var startColor = blurScreen.material.color;
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
    }
}
