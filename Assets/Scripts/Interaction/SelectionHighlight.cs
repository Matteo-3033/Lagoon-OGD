using Interaction;
using UnityEngine;

public class SelectionHighlight : MonoBehaviour, ISelectable
{
    [SerializeField] private GameObject[] highlights;

    public virtual void OnSelected() => Show();

    public virtual void OnDeselected() => Hide();

    protected virtual void Awake()
    {
        Hide();
    }

    private void Show()
    {
        foreach (var hl in highlights)
            hl.SetActive(true);
    }
    
    private void Hide()
    {
        foreach (var hl in highlights)
            hl.SetActive(false);
    }
}
