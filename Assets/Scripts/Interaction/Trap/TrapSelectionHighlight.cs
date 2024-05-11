namespace Interaction.Trap
{
    public class TrapSelectionHighlight: SelectionHighlight
    {
        private TrapVendingMachineInteractable trapInteractable;
        
        protected override void Awake()
        {
            base.Awake();
            
            trapInteractable = GetComponent<TrapVendingMachineInteractable>();
        }

        public override void OnSelected()
        {
            if (trapInteractable.Working)
                base.OnSelected();
        }
    }
}