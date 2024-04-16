using Interaction;
using UnityEngine;

public class SelectionHighlight : MonoBehaviour, ISelectable
{
    [SerializeField] private GameObject[] highlights;

    public void OnSelected() => Show();

    public void OnDeselected() => Hide();

    private void Awake()
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
