using UnityEngine;

public class MinimapDarkArea : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    
    public bool IsVisible => spriteRenderer.enabled;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void Show()
    {
        spriteRenderer.enabled = true;
    }

    public void Hide()
    {
        spriteRenderer.enabled = false;
    }
}
