using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemProva : MonoBehaviour, IInteractable
{
    public GameObject model;
    //public string itemName;
    //public string itemType;

    [SerializeField] private string _prompt;

    public string InteractionPrompt => throw new System.NotImplementedException();

    public bool Interact(Interactor interactor)
    {
        Destroy(model);
        return true;
    }
}
