namespace Modifiers
{
    // Do not instantiate
    // Not abstract only for serialization
    public class StatsModifier: Modifier
    {
        public bool isBuff;
        public StatsModifier other;
        public StatsModifier synergy;
        
        public override void Enable()
        {
            throw new System.NotImplementedException();
        }

        public override void Disable()
        {
            throw new System.NotImplementedException();
        }
    }
}