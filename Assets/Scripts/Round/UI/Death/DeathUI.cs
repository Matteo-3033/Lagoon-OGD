using UnityEngine;
using Screen = Utils.UI.Screen;

namespace Round.UI.Death
{
    public class DeathUI: Screen
    {
        [SerializeField] private RespawnTimer timer;
        
        public override void OnFocus()
        {
            base.OnFocus();
            timer.gameObject.SetActive(true);
            timer.StartTimer();
        }

        public override void OnUnfocus()
        {
            base.OnUnfocus();
            timer.StopTimer();
            timer.gameObject.SetActive(false);
        }
    }
}