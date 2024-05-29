namespace Interaction.Trap
{
    public class TrapSelectionHighlight: SelectionHighlight
    {
        private TrapDispenserInteractable trapInteractable;
        
        protected override void Awake()
        {
            base.Awake();
            
            trapInteractable = GetComponent<TrapDispenserInteractable>();
        }

        public override void OnSelected()
        {
            if (trapInteractable.Working)
                base.OnSelected();
        }
    }
}