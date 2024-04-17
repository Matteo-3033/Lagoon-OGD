using UnityEngine;
using Screen = Utils.Screen;

namespace Round.UI.Main
{
    public class RoundUI: Screen
    {
        [SerializeField] private Timer timer;
        
        public override void OnFocus()
        {
            base.OnFocus();
            timer.StartTimer();
        }
    }
}