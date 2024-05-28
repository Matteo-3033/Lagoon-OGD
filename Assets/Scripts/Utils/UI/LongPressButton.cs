using System;
using Interaction;
using UnityEngine;
using UnityEngine.UI;

namespace Utils.UI
{
    public class LongPressButton : MonoBehaviour, IInteractable
    {
        [SerializeField] private string interactionPrompt;
        [SerializeField] private Image fillImage;
        [SerializeField] private float baseRequiredHoldTime = 5F;

        public static float HoldFactorModifier = 0F;
        
        private float RequiredHoldTime => Math.Max(0.1F, baseRequiredHoldTime + HoldFactorModifier);
        
        private bool isInteracting;
        private float interactionTimer;

        public event EventHandler<EventArgs> OnInteractionCompleted;
        public event EventHandler<EventArgs> OnInteractionStart;
        public event EventHandler<EventArgs> OnImageEmptied;

        public string InteractionPrompt => interactionPrompt;

        public bool StartInteraction(Interactor interactor)
        {
            OnInteractionStart?.Invoke(this, EventArgs.Empty);
            isInteracting = true;
            return true;
        }

        public void StopInteraction(Interactor interactor)
        {
            isInteracting = false;
        }
        
        private void Update()
        {
            if (isInteracting)
            {
                interactionTimer += Time.deltaTime;
                if (interactionTimer >= RequiredHoldTime)
                {
                    OnInteractionCompleted?.Invoke(this, EventArgs.Empty);
                    Reset();
                }

                interactionTimer = Mathf.Min(interactionTimer, RequiredHoldTime);
            }
            else if (interactionTimer > 0)
            {
                interactionTimer -= Time.deltaTime;
                
                if (interactionTimer <= 0)
                    OnImageEmptied?.Invoke(this, EventArgs.Empty);

                interactionTimer = Mathf.Max(0F, interactionTimer);
            }
            
            fillImage.fillAmount = interactionTimer / RequiredHoldTime;
        }

        private void Reset()
        {
            isInteracting = false;
            interactionTimer = 0;
            fillImage.fillAmount = interactionTimer / RequiredHoldTime;
        }
    }
}