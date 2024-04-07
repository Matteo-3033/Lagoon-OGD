using UnityEngine;

namespace Menu.UI.LobbyMenu
{
    public class LobbyMenu : Menu
    {
        [SerializeField] private PlayerPreviewer playerPreviewer;
        
        public override void OnFocus()
        {
            base.OnFocus();
            playerPreviewer.SetMatch(Player.LocalPlayer.Match);
        }
        
        public override void OnUnfocus()
        {
            base.OnUnfocus();
            playerPreviewer.Clear();
        }
    }
}