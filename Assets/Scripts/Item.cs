using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class Item : ScriptableObject, IInteractable
{
    public GameObject model;
    public string itemName;
    public string itemType;

    [SerializeField] private string _prompt;

    public string InteractionPrompt => throw new System.NotImplementedException();

    public bool Interact(Interactor interactor)
    {
        Destroy(this);
        return true;
    }
}
