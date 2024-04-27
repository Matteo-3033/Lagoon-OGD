using Mirror;
using Network;
using UnityEngine;
using Utils;

namespace Interaction.KeyFragments
{
    public class KeyFragmentUI: NetworkBehaviour
    { 
        [SerializeField] private GameObject fillImage;
        
        private void Awake()
        {
            Hide();
            
            var longPressHandler = GetComponentInParent<LongPressButton>();
            
            longPressHandler.OnInteractionStart += (_, _) => Show();
            longPressHandler.OnImageEmptied += (_, _) => Hide();
            longPressHandler.OnInteractionCompleted += (_, _) => Hide();
        }
        
        private void Show()
        {
            fillImage.SetActive(true);
        }
        
        private void Hide()
        {
            fillImage.SetActive(false);
        }
    }
}