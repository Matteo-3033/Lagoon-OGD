using UnityEngine;

public class BlurEffect : MonoBehaviour
{
    [SerializeField] private Camera blurCamera;
    [SerializeField] private SpriteRenderer blurScreen;
    
    private static readonly int TextureId = Shader.PropertyToID("_Texture");

    private void Start()
    {
        if (blurCamera.targetTexture != null)
            blurCamera.targetTexture.Release();
        
        blurCamera.targetTexture = new RenderTexture(Screen.width, Screen.height, 24, RenderTextureFormat.ARGB32, 1);
        blurScreen.material.SetTexture(TextureId, blurCamera.targetTexture);
        
        blurScreen.gameObject.SetActive(true);
    }
}
