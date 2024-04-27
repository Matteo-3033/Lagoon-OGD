namespace Interaction
{
    public interface IInteractable 
    {
        public string InteractionPrompt { get; }
        public bool StartInteraction(Interactor interactor);

        public void StopInteraction(Interactor interactor)
        {
        }
    }
}
