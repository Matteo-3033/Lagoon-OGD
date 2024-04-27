namespace Interaction.Trap
{
    public class TrapSelectionHighlight: SelectionHighlight
    {
        private TrapInteractable trapInteractable;
        
        protected override void Awake()
        {
            base.Awake();
            
            trapInteractable = GetComponent<TrapInteractable>();
        }

        public override void OnSelected()
        {
            if (trapInteractable.Working)
                base.OnSelected();
        }
    }
}