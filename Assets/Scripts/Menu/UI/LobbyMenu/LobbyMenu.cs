using UnityEngine;

namespace Menu.UI.LobbyMenu
{
    public class LobbyMenu : Menu
    {
        [SerializeField] private PlayerPreviewer playerPreviewer;

        public override void OnFocus()
        {
            base.OnFocus();
            
            playerPreviewer.Clear();
            //playerPreviewer.ShowPlayerPreviews(players);
        }

        public override void OnUnfocus()
        {
            base.OnUnfocus();
            playerPreviewer.Clear();
        }
    }
}