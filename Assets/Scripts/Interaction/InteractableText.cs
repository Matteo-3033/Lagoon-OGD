using TMPro;
using UnityEngine;

namespace Interaction
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class InteractableText: MonoBehaviour
    {
        private void Awake()
        {
            var interactable = GetComponentInParent<IInteractable>();
            var text = GetComponent<TextMeshProUGUI>();

            if (interactable == null)
            {
                Debug.LogWarning("No ModifierInteractable found");
                text.text = "No Interactable";
            }
            else
                text.text = interactable.InteractionPrompt;
        }
    }
}