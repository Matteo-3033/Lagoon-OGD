using UnityEngine;

public class MinimapDarkArea : MonoBehaviour
{
    private SpriteRenderer _spriteRenderer;
    private Collider _collider;
    
    public bool IsVisible => _spriteRenderer.enabled;

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _collider = GetComponent<Collider>();
    }

    public void Show()
    {
        _spriteRenderer.enabled = true;
        _collider.enabled = true;
    }

    public void Hide()
    {
        _spriteRenderer.enabled = false;
        _collider.enabled = false;
    }
}
